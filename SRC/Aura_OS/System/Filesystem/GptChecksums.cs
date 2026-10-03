/*
* PROJECT:          Aura Operating System Development
* CONTENT:          GPT checksums and backup copy
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

// File-scope usings resolve from the global namespace. Inside `namespace Aura_OS.System.*` the bare
// identifier `System` binds to `Aura_OS.System`, so never write `System.IO.X` in the body.
using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// GEN3-GAP(gpt-crc): Cosmos writes the GPT with no checksums and no backup copy, and leaves both
    /// stale when it edits a table another system wrote. Other systems then take the table as damaged,
    /// or read the old backup instead. Update writes them again from the primary table, after each
    /// change Aura makes to a GPT disk.
    /// GEN3-GAP(gpt-type): no API changes a partition's type: SetPartitionType does, for a format.
    /// </summary>
    internal static class GptChecksums
    {
        private const ulong PrimaryHeaderLba = 1;

        // "EFI PART"
        private const ulong Signature = 0x5452415020494645UL;

        // The header's fields (UEFI spec 5.3.2).
        private const int SignatureOffset = 0;
        private const int HeaderSizeOffset = 12;
        private const int HeaderCrcOffset = 16;
        private const int MyLbaOffset = 24;
        private const int AlternateLbaOffset = 32;
        private const int LastUsableOffset = 48;
        private const int EntryLbaOffset = 72;
        private const int EntryCountOffset = 80;
        private const int EntrySizeOffset = 84;
        private const int EntryArrayCrcOffset = 88;

        // An entry's first sector, after its type and unique GUIDs.
        private const int EntryStartOffset = 32;

        private const int MinHeaderSize = 92;

        // Bounds of a sane entry array: 128 entries of 128 bytes is the usual one.
        private const uint MinEntrySize = 128;
        private const uint MaxEntrySize = 1024;
        private const uint MaxEntryCount = 1024;

        private static uint[] s_table;

        /// <summary>
        /// Writes the checksums of the disk's primary GPT header and entry array, then the backup copy
        /// of both at the end of the disk (when the table leaves room for it there). Nothing for a
        /// disk whose LBA 1 holds no GPT header.
        /// </summary>
        public static void Update(IBlockDevice disk)
        {
            int blockSize = (int)disk.BlockSize;
            byte[] header = new byte[blockSize];
            disk.ReadBlock(PrimaryHeaderLba, 1, header);

            if (BitConverter.ToUInt64(header, SignatureOffset) != Signature)
            {
                return;
            }

            uint headerSize = BitConverter.ToUInt32(header, HeaderSizeOffset);
            uint entryCount = BitConverter.ToUInt32(header, EntryCountOffset);
            uint entrySize = BitConverter.ToUInt32(header, EntrySizeOffset);
            ulong entryLba = BitConverter.ToUInt64(header, EntryLbaOffset);
            ulong lastUsable = BitConverter.ToUInt64(header, LastUsableOffset);

            if (headerSize < MinHeaderSize || headerSize > blockSize || entrySize < MinEntrySize || entrySize > MaxEntrySize
                || entryCount == 0 || entryCount > MaxEntryCount || entryLba <= PrimaryHeaderLba)
            {
                return;
            }

            int arrayBytes = (int)(entryCount * entrySize);
            ulong arraySectors = (ulong)((arrayBytes + blockSize - 1) / blockSize);

            if (entryLba + arraySectors > disk.BlockCount)
            {
                return;
            }

            byte[] entries = new byte[(int)arraySectors * blockSize];
            disk.ReadBlock(entryLba, arraySectors, entries);

            ulong lastLba = disk.BlockCount - 1;

            // The backup array sits right before the backup header, after the last usable sector.
            ulong backupEntryLba = lastLba - arraySectors;
            bool backup = lastLba > arraySectors && backupEntryLba > lastUsable;

            WriteUInt32(header, EntryArrayCrcOffset, Crc32(entries, arrayBytes));

            if (backup)
            {
                WriteUInt64(header, AlternateLbaOffset, lastLba);
            }

            Seal(header, (int)headerSize);
            disk.WriteBlock(PrimaryHeaderLba, 1, header);

            if (backup)
            {
                disk.WriteBlock(backupEntryLba, arraySectors, entries);

                WriteUInt64(header, MyLbaOffset, lastLba);
                WriteUInt64(header, AlternateLbaOffset, PrimaryHeaderLba);
                WriteUInt64(header, EntryLbaOffset, backupEntryLba);
                Seal(header, (int)headerSize);
                disk.WriteBlock(lastLba, 1, header);
            }

            disk.Flush();
        }

        /// <summary>
        /// Gives the partition that starts at that sector the type, then writes the checksums and the
        /// backup (Update). Nothing when it has that type already, or the table has no such partition.
        /// </summary>
        public static void SetPartitionType(IBlockDevice disk, ulong start, Guid type)
        {
            int blockSize = (int)disk.BlockSize;
            byte[] header = new byte[blockSize];
            disk.ReadBlock(PrimaryHeaderLba, 1, header);

            if (BitConverter.ToUInt64(header, SignatureOffset) != Signature)
            {
                return;
            }

            uint entryCount = BitConverter.ToUInt32(header, EntryCountOffset);
            uint entrySize = BitConverter.ToUInt32(header, EntrySizeOffset);
            ulong entryLba = BitConverter.ToUInt64(header, EntryLbaOffset);

            if (entrySize < MinEntrySize || entrySize > MaxEntrySize || entryCount == 0 || entryCount > MaxEntryCount
                || entryLba <= PrimaryHeaderLba)
            {
                return;
            }

            ulong arraySectors = (ulong)(((int)(entryCount * entrySize) + blockSize - 1) / blockSize);

            if (entryLba + arraySectors > disk.BlockCount)
            {
                return;
            }

            byte[] entries = new byte[(int)arraySectors * blockSize];
            disk.ReadBlock(entryLba, arraySectors, entries);
            byte[] guid = type.ToByteArray();

            for (int offset = 0; offset + (int)entrySize <= (int)(entryCount * entrySize); offset += (int)entrySize)
            {
                if (BitConverter.ToUInt64(entries, offset + EntryStartOffset) != start || IsEmptyType(entries, offset))
                {
                    continue;
                }

                bool same = true;

                for (int i = 0; i < 16; i++)
                {
                    same = same && entries[offset + i] == guid[i];
                }

                if (!same)
                {
                    Array.Copy(guid, 0, entries, offset, 16);
                    disk.WriteBlock(entryLba, arraySectors, entries);
                    Update(disk);
                }

                return;
            }
        }

        /// <summary>
        /// An entry whose type GUID is all zeros is unused.
        /// </summary>
        private static bool IsEmptyType(byte[] entries, int offset)
        {
            for (int i = 0; i < 16; i++)
            {
                if (entries[offset + i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether the disk's last sector holds a GPT header: the backup of a GPT disk, left over after an
        /// MBR replaced it.
        /// </summary>
        public static bool HasBackupHeader(IBlockDevice disk)
        {
            if (disk.BlockCount < 2)
            {
                return false;
            }

            byte[] sector = new byte[(int)disk.BlockSize];
            disk.ReadBlock(disk.BlockCount - 1, 1, sector);
            return BitConverter.ToUInt64(sector, SignatureOffset) == Signature;
        }

        /// <summary>
        /// The header's checksum, computed with its own field at zero.
        /// </summary>
        private static void Seal(byte[] header, int headerSize)
        {
            WriteUInt32(header, HeaderCrcOffset, 0);
            WriteUInt32(header, HeaderCrcOffset, Crc32(header, headerSize));
        }

        /// <summary>
        /// The CRC-32 GPT uses (IEEE 802.3, reflected) of the first count bytes.
        /// </summary>
        internal static uint Crc32(byte[] data, int count)
        {
            uint[] table = s_table ?? (s_table = BuildTable());
            uint crc = 0xFFFFFFFFu;

            for (int i = 0; i < count; i++)
            {
                crc = table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }

            return crc ^ 0xFFFFFFFFu;
        }

        private static uint[] BuildTable()
        {
            uint[] table = new uint[256];

            for (uint i = 0; i < 256; i++)
            {
                uint value = i;

                for (int bit = 0; bit < 8; bit++)
                {
                    value = (value & 1) != 0 ? 0xEDB88320u ^ (value >> 1) : value >> 1;
                }

                table[i] = value;
            }

            return table;
        }

        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            for (int i = 0; i < 4; i++)
            {
                buffer[offset + i] = (byte)(value >> (8 * i));
            }
        }

        private static void WriteUInt64(byte[] buffer, int offset, ulong value)
        {
            for (int i = 0; i < 8; i++)
            {
                buffer[offset + i] = (byte)(value >> (8 * i));
            }
        }
    }
}
