/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ext2 volumes: format, label, the ones Aura mounts
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

// File-scope usings resolve from the global namespace. Inside `namespace Aura_OS.System.*` the bare
// identifier `System` binds to `Aura_OS.System`, so never write `System.IO.X` in the body.
using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Storage;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// The ext2 volumes Aura writes and mounts, Cosmos's driver reading and writing their files.
    /// GEN3-GAP(ext2-format): past one block group (8 MB), Cosmos's formatter puts each group's bitmaps
    /// where the superblock's backups go, and e2fsck refuses the volume. Format lays it out as mke2fs
    /// does instead: 4 KB blocks, backups in groups 0, 1 and the powers of 3, 5 and 7, a lost+found folder.
    /// GEN3-GAP(ext2-1k): the driver allocates blocks without the first data block's offset, which is 1
    /// on a volume of 1 KB blocks: its writes there land one block early, over the metadata.
    /// GEN3-GAP(ext2-features): the driver mounts any ext2, ext3 or ext4 superblock and ignores
    /// MountFlags.ReadOnly. CanMount keeps to the volumes it writes right.
    /// </summary>
    internal static class Ext2Volume
    {
        /// <summary>The longest ext2 volume label.</summary>
        public const int MaxLabelLength = 16;

        /// <summary>The smallest volume Format writes.</summary>
        public const ulong MinimumBytes = 1024UL * 1024UL;

        private const int BlockSize = 4096;

        // One block of bitmap covers a group.
        private const uint BlocksPerGroup = 8 * BlockSize;
        private const int InodeSize = 128;
        private const int InodesPerBlock = BlockSize / InodeSize;
        private const int DescriptorSize = 32;

        // mke2fs's bytes of volume per inode: an inode a block under 512 MB, one per 16 KB up to 16 GB,
        // one per 64 KB past it.
        private const ulong SmallBytesPerInode = 4096;
        private const ulong BytesPerInode = 16384;
        private const ulong BigBytesPerInode = 65536;
        private const ulong SmallVolumeBytes = 512UL * 1024UL * 1024UL;
        private const ulong BigVolumeBytes = 16UL * 1024UL * 1024UL * 1024UL;

        // A last group that small holds no data once its metadata is in: mke2fs leaves it out.
        private const uint MinimumLastGroupData = 50;

        // Inodes 1 to 10 are reserved, the root folder is 2; lost+found takes the first one left.
        private const uint RootInode = 2;
        private const uint FirstInode = 11;
        private const uint LostFoundInode = 11;

        // The superblock: at byte 1024 of the volume, 1024 bytes long.
        private const int SuperblockOffset = 1024;
        private const int SuperblockSize = 1024;
        private const int InodesCountOffset = 0;
        private const int BlocksCountOffset = 4;
        private const int FreeBlocksOffset = 12;
        private const int FreeInodesOffset = 16;
        private const int FirstDataBlockOffset = 20;
        private const int LogBlockSizeOffset = 24;
        private const int LogFragmentSizeOffset = 28;
        private const int BlocksPerGroupOffset = 32;
        private const int FragmentsPerGroupOffset = 36;
        private const int InodesPerGroupOffset = 40;
        private const int WriteTimeOffset = 48;
        private const int MaxMountCountOffset = 54;
        private const int MagicOffset = 56;
        private const int StateOffset = 58;
        private const int ErrorsOffset = 60;
        private const int LastCheckOffset = 64;
        private const int RevisionOffset = 76;
        private const int FirstInodeOffset = 84;
        private const int InodeSizeOffset = 88;
        private const int GroupNumberOffset = 90;
        private const int CompatibleOffset = 92;
        private const int IncompatibleOffset = 96;
        private const int ReadOnlyCompatibleOffset = 100;
        private const int UuidOffset = 104;
        private const int LabelOffset = 120;
        private const int CreationTimeOffset = 264;

        private const ushort Magic = 0xEF53;
        private const ushort StateClean = 1;
        private const ushort ErrorsContinue = 1;
        private const uint DynamicRevision = 1;

        // The features Format writes, and the ones the driver handles: entries with a file type
        // (incompatible), sparse superblock backups and large files (read-only compatible).
        private const uint IncompatibleFileType = 0x2;
        private const uint ReadOnlySparseSuper = 0x1;
        private const uint ReadOnlyLargeFile = 0x2;
        private const uint CompatibleJournal = 0x4;

        // Extents, 64-bit and flexible block groups: ext4's.
        private const uint Ext4Incompatible = 0x2C0;
        private const uint SupportedIncompatible = IncompatibleFileType;
        private const uint SupportedReadOnly = ReadOnlySparseSuper | ReadOnlyLargeFile;

        // A group descriptor's fields.
        private const int BlockBitmapOffset = 0;
        private const int InodeBitmapOffset = 4;
        private const int InodeTableOffset = 8;
        private const int GroupFreeBlocksOffset = 12;
        private const int GroupFreeInodesOffset = 14;
        private const int GroupFoldersOffset = 16;

        // An inode's fields, and the folders' modes: drwxr-xr-x for the root, drwx------ for lost+found.
        private const int ModeOffset = 0;
        private const int SizeOffset = 4;
        private const int AccessTimeOffset = 8;
        private const int ChangeTimeOffset = 12;
        private const int ModifyTimeOffset = 16;
        private const int LinksOffset = 26;
        private const int SectorsOffset = 28;
        private const int FirstBlockOffset = 40;
        private const ushort RootMode = 0x41ED;
        private const ushort LostFoundMode = 0x41C0;

        // A folder entry: inode, entry length, name length, file type (2: folder), name.
        private const byte FolderType = 2;

        // The zeros an inode table is cleared with, at most that many blocks a write.
        private const int ClearBlocks = 64;

        /// <summary>
        /// Writes a new ext2 volume on the whole partition. Its files are lost.
        /// </summary>
        /// <param name="label">The volume label (CheckLabel), "" or null for none.</param>
        /// <exception cref="InvalidOperationException">The partition is too small, or its sectors do not divide 4 KB.</exception>
        public static void Format(Partition partition, string label)
        {
            string text = CheckLabel(label);
            int sectorSize = (int)partition.BlockSize;

            if (sectorSize <= 0 || BlockSize % sectorSize != 0)
            {
                throw new InvalidOperationException("Aura writes ext2 on disks of 512 to 4096-byte sectors.");
            }

            ulong bytes = partition.BlockCount * (ulong)sectorSize;

            if (bytes < MinimumBytes)
            {
                throw new InvalidOperationException("An ext2 volume takes 1 MB or more.");
            }

            uint totalBlocks = (uint)Math.Min(bytes / BlockSize, uint.MaxValue);
            ulong bytesPerInode = bytes < SmallVolumeBytes ? SmallBytesPerInode : bytes < BigVolumeBytes ? BytesPerInode : BigBytesPerInode;
            uint groups, descriptorBlocks, inodesPerGroup, tableBlocks;

            // The last group too small for its metadata and some data is left out, which changes the rest.
            while (true)
            {
                groups = (totalBlocks + BlocksPerGroup - 1) / BlocksPerGroup;
                descriptorBlocks = (uint)((groups * (ulong)DescriptorSize + BlockSize - 1) / BlockSize);

                ulong wanted = (ulong)totalBlocks * BlockSize / bytesPerInode;
                ulong perGroup = (wanted + groups - 1) / groups;
                perGroup = (perGroup + InodesPerBlock - 1) / InodesPerBlock * InodesPerBlock;
                inodesPerGroup = (uint)Math.Min(Math.Max(perGroup, (ulong)InodesPerBlock), 8UL * BlockSize);
                tableBlocks = inodesPerGroup / InodesPerBlock;

                uint last = groups - 1;
                uint lastBlocks = totalBlocks - last * BlocksPerGroup;

                if (groups > 1 && lastBlocks < Overhead(last, descriptorBlocks, tableBlocks) + MinimumLastGroupData)
                {
                    totalBlocks = last * BlocksPerGroup;
                    continue;
                }

                break;
            }

            if (Math.Min(totalBlocks, BlocksPerGroup) < Overhead(0, descriptorBlocks, tableBlocks) + 16)
            {
                throw new InvalidOperationException("An ext2 volume takes 1 MB or more.");
            }

            uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            byte[] descriptors = new byte[(int)descriptorBlocks * BlockSize];
            byte[] zeros = new byte[ClearBlocks * BlockSize];
            uint freeBlocks = 0;
            uint rootBlock = 0;
            uint tableStart = 0;

            for (uint group = 0; group < groups; group++)
            {
                uint start = group * BlocksPerGroup;
                uint blocks = group == groups - 1 ? totalBlocks - start : BlocksPerGroup;
                uint used = Overhead(group, descriptorBlocks, tableBlocks);
                uint blockBitmap = start + used - tableBlocks - 2 - (group == 0 ? 2u : 0u);
                uint inodeTable = blockBitmap + 2;

                byte[] bitmaps = new byte[2 * BlockSize];
                SetBits(bitmaps, 0, 0, (int)used);
                SetBits(bitmaps, 0, (int)blocks, 8 * BlockSize);
                SetBits(bitmaps, BlockSize, 0, group == 0 ? (int)LostFoundInode : 0);
                SetBits(bitmaps, BlockSize, (int)inodesPerGroup, 8 * BlockSize);
                WriteBlocks(partition, blockBitmap, bitmaps, 2);

                for (uint cleared = 0; cleared < tableBlocks; cleared += ClearBlocks)
                {
                    WriteBlocks(partition, inodeTable + cleared, zeros, (int)Math.Min(ClearBlocks, tableBlocks - cleared));
                }

                int descriptor = (int)group * DescriptorSize;
                Write32(descriptors, descriptor + BlockBitmapOffset, blockBitmap);
                Write32(descriptors, descriptor + InodeBitmapOffset, blockBitmap + 1);
                Write32(descriptors, descriptor + InodeTableOffset, inodeTable);
                Write16(descriptors, descriptor + GroupFreeBlocksOffset, (ushort)(blocks - used));
                Write16(descriptors, descriptor + GroupFreeInodesOffset, (ushort)(inodesPerGroup - (group == 0 ? LostFoundInode : 0)));
                Write16(descriptors, descriptor + GroupFoldersOffset, (ushort)(group == 0 ? 2 : 0));
                freeBlocks += blocks - used;

                if (group == 0)
                {
                    tableStart = inodeTable;
                    rootBlock = inodeTable + tableBlocks;
                }
            }

            WriteFolders(partition, tableStart, rootBlock, now);

            uint inodes = groups * inodesPerGroup;
            byte[] superblock = new byte[SuperblockSize];
            Write32(superblock, InodesCountOffset, inodes);
            Write32(superblock, BlocksCountOffset, totalBlocks);
            Write32(superblock, FreeBlocksOffset, freeBlocks);
            Write32(superblock, FreeInodesOffset, inodes - LostFoundInode);
            Write32(superblock, FirstDataBlockOffset, 0);
            Write32(superblock, LogBlockSizeOffset, 2);
            Write32(superblock, LogFragmentSizeOffset, 2);
            Write32(superblock, BlocksPerGroupOffset, BlocksPerGroup);
            Write32(superblock, FragmentsPerGroupOffset, BlocksPerGroup);
            Write32(superblock, InodesPerGroupOffset, inodesPerGroup);
            Write32(superblock, WriteTimeOffset, now);
            Write16(superblock, MaxMountCountOffset, 0xFFFF);
            Write16(superblock, MagicOffset, Magic);
            Write16(superblock, StateOffset, StateClean);
            Write16(superblock, ErrorsOffset, ErrorsContinue);
            Write32(superblock, LastCheckOffset, now);
            Write32(superblock, RevisionOffset, DynamicRevision);
            Write32(superblock, FirstInodeOffset, FirstInode);
            Write16(superblock, InodeSizeOffset, InodeSize);
            Write32(superblock, IncompatibleOffset, IncompatibleFileType);
            Write32(superblock, ReadOnlyCompatibleOffset, ReadOnlySparseSuper);
            Array.Copy(NewUuid(partition), 0, superblock, UuidOffset, 16);
            Array.Copy(LabelBytes(text), 0, superblock, LabelOffset, MaxLabelLength);
            Write32(superblock, CreationTimeOffset, now);

            // The backups first, the primary superblock last: the volume is whole once it is there.
            for (uint group = 1; group < groups; group++)
            {
                if (HasSuperblock(group))
                {
                    byte[] backup = new byte[BlockSize];
                    Array.Copy(superblock, backup, SuperblockSize);
                    Write16(backup, GroupNumberOffset, (ushort)group);
                    WriteBlocks(partition, group * BlocksPerGroup, backup, 1);
                    WriteBlocks(partition, group * BlocksPerGroup + 1, descriptors, (int)descriptorBlocks);
                }
            }

            WriteBlocks(partition, 1, descriptors, (int)descriptorBlocks);

            byte[] first = new byte[BlockSize];
            Array.Copy(superblock, 0, first, SuperblockOffset, SuperblockSize);
            WriteBlocks(partition, 0, first, 1);
            partition.Flush();
        }

        /// <summary>
        /// The root folder (".", "..", lost+found) and lost+found (".", ".."), in the two blocks after
        /// group 0's inode table; their inodes in its first block.
        /// </summary>
        private static void WriteFolders(Partition partition, uint inodeTable, uint rootBlock, uint now)
        {
            byte[] table = new byte[BlockSize];
            WriteFolderInode(table, RootInode, RootMode, 3, rootBlock, now);
            WriteFolderInode(table, LostFoundInode, LostFoundMode, 2, rootBlock + 1, now);

            byte[] folders = new byte[2 * BlockSize];
            int offset = WriteEntry(folders, 0, RootInode, ".", 12);
            offset = WriteEntry(folders, offset, RootInode, "..", 12);
            WriteEntry(folders, offset, LostFoundInode, "lost+found", BlockSize - offset);

            offset = WriteEntry(folders, BlockSize, LostFoundInode, ".", 12);
            WriteEntry(folders, offset, RootInode, "..", 2 * BlockSize - offset);

            WriteBlocks(partition, rootBlock, folders, 2);
            WriteBlocks(partition, inodeTable, table, 1);
        }

        private static void WriteFolderInode(byte[] table, uint inode, ushort mode, ushort links, uint block, uint now)
        {
            int offset = (int)(inode - 1) * InodeSize;
            Write16(table, offset + ModeOffset, mode);
            Write32(table, offset + SizeOffset, BlockSize);
            Write32(table, offset + AccessTimeOffset, now);
            Write32(table, offset + ChangeTimeOffset, now);
            Write32(table, offset + ModifyTimeOffset, now);
            Write16(table, offset + LinksOffset, links);
            Write32(table, offset + SectorsOffset, BlockSize / 512);
            Write32(table, offset + FirstBlockOffset, block);
        }

        /// <summary>
        /// A folder entry at that offset, that long: the offset of the next one.
        /// </summary>
        private static int WriteEntry(byte[] block, int offset, uint inode, string name, int length)
        {
            Write32(block, offset, inode);
            Write16(block, offset + 4, (ushort)length);
            block[offset + 6] = (byte)name.Length;
            block[offset + 7] = FolderType;

            for (int i = 0; i < name.Length; i++)
            {
                block[offset + 8 + i] = (byte)name[i];
            }

            return offset + length;
        }

        /// <summary>
        /// The blocks at the start of a group its metadata takes: the superblock and descriptors' backup
        /// (in some groups), the two bitmaps, the inode table; group 0 also holds the two folders.
        /// </summary>
        private static uint Overhead(uint group, uint descriptorBlocks, uint tableBlocks)
        {
            return (HasSuperblock(group) ? 1 + descriptorBlocks : 0) + 2 + tableBlocks + (group == 0 ? 2u : 0u);
        }

        /// <summary>
        /// sparse_super: a group holds a superblock (and descriptors) when it is 0, 1, or a power of 3, 5 or 7.
        /// </summary>
        private static bool HasSuperblock(uint group)
        {
            return group <= 1 || IsPower(group, 3) || IsPower(group, 5) || IsPower(group, 7);
        }

        private static bool IsPower(uint value, uint radix)
        {
            ulong power = radix;

            while (power < value)
            {
                power *= radix;
            }

            return power == value;
        }

        /// <summary>
        /// The label of an ext2 volume: up to 16 ASCII letters, digits, spaces and punctuation. "" for none.
        /// </summary>
        /// <exception cref="InvalidOperationException">Another character, or too long.</exception>
        public static string CheckLabel(string label)
        {
            string text = (label ?? "").Trim();

            if (text.Length > MaxLabelLength)
            {
                throw new InvalidOperationException("An ext2 label has " + MaxLabelLength + " characters at most.");
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] < 0x20 || text[i] > 0x7E)
                {
                    throw new InvalidOperationException("An ext2 label has ASCII letters, digits, spaces and punctuation only.");
                }
            }

            return text;
        }

        /// <summary>
        /// Changes the label in the volume's superblock. The volume is not mounted: the driver would write
        /// its own copy of the superblock over it.
        /// </summary>
        public static void SetLabel(Partition partition, string label)
        {
            string text = CheckLabel(label);
            int sectorSize = (int)partition.BlockSize;
            ulong lba = (ulong)(SuperblockOffset / sectorSize);
            int sectors = Math.Max(1, SuperblockSize / sectorSize);
            int offset = SuperblockOffset % sectorSize;
            byte[] data = new byte[sectors * sectorSize];

            partition.ReadBlock(lba, (ulong)sectors, data);

            if (BitConverter.ToUInt16(data, offset + MagicOffset) != Magic)
            {
                throw new InvalidOperationException("There is no ext2 volume on it.");
            }

            Array.Copy(LabelBytes(text), 0, data, offset + LabelOffset, MaxLabelLength);
            partition.WriteBlock(lba, (ulong)sectors, data);
            partition.Flush();
        }

        /// <summary>
        /// Whether the start of a partition (its first 2 KB or more) holds an ext superblock: then its
        /// filesystem ("ext2", "ext3" with a journal, "ext4" with extents, 64-bit or flexible block
        /// groups), its label ("" for none) and the sectors of that size the volume takes.
        /// </summary>
        public static bool TryRead(byte[] head, int sectorSize, out string filesystem, out string label, out ulong volumeSectors)
        {
            filesystem = null;
            label = "";
            volumeSectors = 0;

            if (!IsExt(head))
            {
                return false;
            }

            label = Disks.ReadText(head, SuperblockOffset + LabelOffset, MaxLabelLength);

            uint log = BitConverter.ToUInt32(head, SuperblockOffset + LogBlockSizeOffset);
            ulong bytes = log > 6 ? 0 : (ulong)BitConverter.ToUInt32(head, SuperblockOffset + BlocksCountOffset) * (1024UL << (int)log);
            volumeSectors = (bytes + (ulong)sectorSize - 1) / (ulong)sectorSize;

            uint compatible = BitConverter.ToUInt32(head, SuperblockOffset + CompatibleOffset);
            uint incompatible = BitConverter.ToUInt32(head, SuperblockOffset + IncompatibleOffset);

            if ((incompatible & Ext4Incompatible) != 0)
            {
                filesystem = "ext4";
            }
            else
            {
                filesystem = (compatible & CompatibleJournal) != 0 ? "ext3" : Disks.Ext2;
            }

            return true;
        }

        private static bool IsExt(byte[] head)
        {
            return head.Length >= SuperblockOffset + SuperblockSize && BitConverter.ToUInt16(head, SuperblockOffset + MagicOffset) == Magic;
        }

        /// <summary>
        /// Whether the driver can mount the ext volume of that partition and write it right; why not, else.
        /// </summary>
        public static bool CanMount(IBlockDevice partition, out string reason)
        {
            reason = null;
            int sectorSize = (int)partition.BlockSize;
            int sectors = (SuperblockOffset + SuperblockSize + sectorSize - 1) / sectorSize;
            byte[] head = new byte[sectors * sectorSize];

            if (partition.BlockCount < (ulong)sectors)
            {
                reason = "There is no ext2 volume on it.";
                return false;
            }

            partition.ReadBlock(0, (ulong)sectors, head);

            if (!IsExt(head))
            {
                reason = "There is no ext2 volume on it.";
                return false;
            }

            uint compatible = BitConverter.ToUInt32(head, SuperblockOffset + CompatibleOffset);
            uint incompatible = BitConverter.ToUInt32(head, SuperblockOffset + IncompatibleOffset);
            uint readOnly = BitConverter.ToUInt32(head, SuperblockOffset + ReadOnlyCompatibleOffset);
            uint log = BitConverter.ToUInt32(head, SuperblockOffset + LogBlockSizeOffset);
            uint firstDataBlock = BitConverter.ToUInt32(head, SuperblockOffset + FirstDataBlockOffset);

            if ((compatible & CompatibleJournal) != 0)
            {
                reason = "Aura mounts ext2 volumes: this one has a journal (ext3 or ext4).";
            }
            else if ((incompatible & ~SupportedIncompatible) != 0 || (readOnly & ~SupportedReadOnly) != 0)
            {
                reason = "This volume uses ext3 or ext4 features Aura cannot write.";
            }
            else if (log == 0 || firstDataBlock != 0)
            {
                reason = "Aura mounts ext2 volumes of 2 or 4 KB blocks: its driver writes those of 1 KB blocks wrong.";
            }
            else if (log > 2)
            {
                reason = "Aura mounts ext2 volumes of 2 or 4 KB blocks: this one has larger ones.";
            }

            return reason == null;
        }

        /// <summary>
        /// The label's 16 bytes, padded with zeros.
        /// </summary>
        private static byte[] LabelBytes(string text)
        {
            byte[] name = new byte[MaxLabelLength];

            for (int i = 0; i < text.Length && i < MaxLabelLength; i++)
            {
                name[i] = (byte)text[i];
            }

            return name;
        }

        /// <summary>
        /// A random (version 4) UUID, from the clock and the partition: Guid.NewGuid needs the platform's
        /// random source.
        /// </summary>
        private static byte[] NewUuid(Partition partition)
        {
            ulong state = (ulong)DateTime.Now.Ticks ^ (partition.StartSector << 17) ^ partition.BlockCount;
            byte[] uuid = new byte[16];

            for (int i = 0; i < 16; i += 8)
            {
                // SplitMix64.
                state += 0x9E3779B97F4A7C15UL;
                ulong value = state;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                value ^= value >> 31;

                for (int j = 0; j < 8; j++)
                {
                    uuid[i + j] = (byte)(value >> (8 * j));
                }
            }

            uuid[6] = (byte)((uuid[6] & 0x0F) | 0x40);
            uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);
            return uuid;
        }

        /// <summary>
        /// Sets the bits from first to end (excluded) of the bitmap at that offset.
        /// </summary>
        private static void SetBits(byte[] bitmap, int offset, int first, int end)
        {
            for (int bit = first; bit < end; bit++)
            {
                bitmap[offset + bit / 8] |= (byte)(1 << (bit % 8));
            }
        }

        /// <summary>
        /// Writes count 4 KB blocks of data from that block on.
        /// </summary>
        private static void WriteBlocks(Partition partition, uint block, byte[] data, int count)
        {
            ulong sectorsPerBlock = (ulong)(BlockSize / (int)partition.BlockSize);
            partition.WriteBlock(block * sectorsPerBlock, (ulong)count * sectorsPerBlock, data.AsSpan(0, count * BlockSize));
        }

        private static void Write16(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        private static void Write32(byte[] buffer, int offset, uint value)
        {
            for (int i = 0; i < 4; i++)
            {
                buffer[offset + i] = (byte)(value >> (8 * i));
            }
        }
    }
}
