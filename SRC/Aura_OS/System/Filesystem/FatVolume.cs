/*
* PROJECT:          Aura Operating System Development
* CONTENT:          FAT volumes: format, label, what their boot sector tells
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

// File-scope usings resolve from the global namespace. Inside `namespace Aura_OS.System.*` the bare
// identifier `System` binds to `Aura_OS.System`, so never write `System.IO.X` in the body.
using System;
using System.Text;
using Cosmos.Kernel.System.FileSystem.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.FileSystem;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// The FAT volumes Aura writes, Cosmos's FAT driver formatting them and reading and writing their files.
    /// GEN3-GAP(fat-label): no API reads or changes a FAT label, and the formatter writes it in the boot
    /// sector only, so the label is read from the boot sector (11 bytes at 0x2B on FAT12/16, 0x47 on
    /// FAT32, there when the extended boot signature is 0x29) and written there and in the root folder's
    /// label entry, which other systems show.
    /// </summary>
    internal static class FatVolume
    {
        /// <summary>The longest FAT volume label.</summary>
        public const int MaxLabelLength = 11;

        // The bytes of a FAT volume's label in its boot sector, after the extended boot signature 0x29.
        private const int Fat1216SignatureOffset = 0x26;
        private const int Fat1216LabelOffset = 0x2B;
        private const int Fat32SignatureOffset = 0x42;
        private const int Fat32LabelOffset = 0x47;
        private const int Fat32BackupBootOffset = 0x32;
        private const byte ExtendedBootSignature = 0x29;

        // A directory entry, and the attribute of the root directory's volume label entry.
        private const int DirectoryEntrySize = 32;
        private const int AttributesOffset = 11;
        private const byte VolumeIdAttribute = 0x08;
        private const byte LongNameAttributes = 0x0F;
        private const byte DeletedEntry = 0xE5;

        // What FAT formatters write for no label.
        private const string NoLabel = "NO NAME";

        // static readonly, NOT const (C16): enum values from a Cosmos assembly.
        private static readonly FatType FatType12 = FatType.Fat12;
        private static readonly FatType FatType16 = FatType.Fat16;
        private static readonly FatType FatType32 = FatType.Fat32;

        /// <summary>
        /// Writes a new FAT volume on the whole partition: FAT32, FAT16 or FAT12 (Disks.Fat32...) with the
        /// smallest clusters that fit, or the largest of them that fits for Disks.Fat. The formatter
        /// writes nothing for a geometry it refuses, so each one is tried in turn.
        /// </summary>
        /// <param name="label">The volume label (CheckLabel), "" or null for none.</param>
        /// <exception cref="InvalidOperationException">No such FAT fits in the partition.</exception>
        public static void Format(Partition partition, string filesystem, string label)
        {
            string text = CheckLabel(label);
            string volumeLabel = text.Length > 0 ? text : null;
            bool formatted;

            switch (filesystem)
            {
                case Disks.Fat32:
                    formatted = TryFormat(partition, FatType32, volumeLabel);
                    break;
                case Disks.Fat16:
                    formatted = TryFormat(partition, FatType16, volumeLabel);
                    break;
                case Disks.Fat12:
                    formatted = TryFormat(partition, FatType12, volumeLabel);
                    break;
                default:
                    formatted = TryFormat(partition, FatType32, volumeLabel)
                        || TryFormat(partition, FatType16, volumeLabel)
                        || TryFormat(partition, FatType12, volumeLabel);
                    break;
            }

            if (!formatted)
            {
                string size = (partition.BlockCount * partition.BlockSize / Disks.BytesPerMiB) + " MB";
                throw new InvalidOperationException("No " + (filesystem == Disks.Fat ? "FAT" : filesystem) + " volume fits in " + size
                    + ": FAT12 takes up to 127 MB, FAT16 3 MB to 2 GB, FAT32 33 MB and more.");
            }

            // The formatter writes the label in the boot sector only: other systems read the root folder's
            // label entry, and fsck takes a boot sector label without one for a mistake.
            FatBootSector bootSector;
            if (text.Length > 0 && FatBootSector.TryParse(ReadSector(partition, 0), out bootSector) && bootSector != null)
            {
                SetRootLabel(partition, bootSector, text);
                partition.Flush();
            }
        }

        private static bool TryFormat(Partition partition, FatType type, string label)
        {
            // FAT32: the formatter's cluster size, Windows' table.
            if (type == FatType32)
            {
                return VfsManager.TryFormat(Volumes.FatDriver, partition, new FatFormatOptions { Type = type, VolumeLabel = label });
            }

            for (int sectorsPerCluster = 1; sectorsPerCluster <= 64; sectorsPerCluster *= 2)
            {
                FatFormatOptions options = new FatFormatOptions { Type = type, SectorsPerCluster = (byte)sectorsPerCluster, VolumeLabel = label };

                if (VfsManager.TryFormat(Volumes.FatDriver, partition, options))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether a partition's first sector is a FAT boot sector: then its type (Disks.Fat32, Fat16,
        /// Fat12, or Disks.Fat for one the parser cannot tell), its label ("" for none) and the sectors
        /// the volume takes.
        /// </summary>
        public static bool TryRead(byte[] boot, out string filesystem, out string label, out ulong volumeSectors)
        {
            filesystem = null;
            label = "";
            volumeSectors = 0;

            FatBootSector bootSector;
            if (!FatBootSector.TryParse(boot, out bootSector) || bootSector == null)
            {
                return false;
            }

            bool fat32 = bootSector.Type == FatType32;

            if (boot[fat32 ? Fat32SignatureOffset : Fat1216SignatureOffset] == ExtendedBootSignature)
            {
                label = Disks.ReadText(boot, fat32 ? Fat32LabelOffset : Fat1216LabelOffset, MaxLabelLength);

                if (label == NoLabel)
                {
                    label = "";
                }
            }

            volumeSectors = bootSector.TotalSectorCount;

            switch (bootSector.Type)
            {
                case FatType.Fat12:
                    filesystem = Disks.Fat12;
                    break;
                case FatType.Fat16:
                    filesystem = Disks.Fat16;
                    break;
                case FatType.Fat32:
                    filesystem = Disks.Fat32;
                    break;
                default:
                    filesystem = Disks.Fat;
                    break;
            }

            return true;
        }

        /// <summary>
        /// The label of a FAT volume, in capitals: up to 11 letters, digits, spaces, - and _. "" for none.
        /// </summary>
        /// <exception cref="InvalidOperationException">Another character, or too long.</exception>
        public static string CheckLabel(string label)
        {
            string text = (label ?? "").Trim();

            if (text.Length > MaxLabelLength)
            {
                throw new InvalidOperationException("A FAT label has " + MaxLabelLength + " characters at most.");
            }

            StringBuilder capitals = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c >= 'a' && c <= 'z')
                {
                    c = (char)(c - 'a' + 'A');
                }

                if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '-' || c == '_'))
                {
                    throw new InvalidOperationException("A FAT label has letters, digits, spaces, - and _ only.");
                }

                capitals.Append(c);
            }

            return capitals.ToString();
        }

        /// <summary>
        /// Whether the FAT volume of that partition has room for a label (an extended boot signature);
        /// why not, else.
        /// </summary>
        public static bool CanLabel(Partition partition, out string reason)
        {
            reason = null;
            byte[] boot = ReadSector(partition, 0);

            FatBootSector bootSector;
            if (!FatBootSector.TryParse(boot, out bootSector) || bootSector == null)
            {
                reason = "There is no FAT volume on it.";
            }
            else if (boot[bootSector.Type == FatType32 ? Fat32SignatureOffset : Fat1216SignatureOffset] != ExtendedBootSignature)
            {
                reason = "This FAT volume has no room for a label.";
            }

            return reason == null;
        }

        /// <summary>
        /// Changes the label (CheckLabel) in the boot sector, its FAT32 backup, and the root folder's label
        /// entry. The volume is not mounted (CanLabel first).
        /// </summary>
        public static void SetLabel(Partition partition, string label)
        {
            string text = CheckLabel(label);
            byte[] boot = ReadSector(partition, 0);

            FatBootSector bootSector;
            if (!FatBootSector.TryParse(boot, out bootSector) || bootSector == null)
            {
                throw new InvalidOperationException("There is no FAT volume on it.");
            }

            bool fat32 = bootSector.Type == FatType32;
            int labelOffset = fat32 ? Fat32LabelOffset : Fat1216LabelOffset;
            byte[] name = LabelBytes(text.Length > 0 ? text : NoLabel);

            Array.Copy(name, 0, boot, labelOffset, MaxLabelLength);
            partition.WriteBlock(0, 1, boot);

            if (fat32)
            {
                ulong backup = BitConverter.ToUInt16(boot, Fat32BackupBootOffset);

                if (backup > 0 && backup < bootSector.ReservedSectorCount)
                {
                    byte[] backupBoot = ReadSector(partition, backup);
                    Array.Copy(name, 0, backupBoot, labelOffset, MaxLabelLength);
                    partition.WriteBlock(backup, 1, backupBoot);
                }
            }

            SetRootLabel(partition, bootSector, text);
            partition.Flush();
        }

        /// <summary>
        /// The label's 11 bytes, padded with spaces.
        /// </summary>
        private static byte[] LabelBytes(string text)
        {
            byte[] name = new byte[MaxLabelLength];

            for (int i = 0; i < MaxLabelLength; i++)
            {
                name[i] = (byte)(i < text.Length ? text[i] : ' ');
            }

            return name;
        }

        /// <summary>
        /// The volume label entry of the root folder (its first cluster on FAT32): renamed, deleted for no
        /// label, or added in its first free entry.
        /// </summary>
        private static void SetRootLabel(Partition partition, FatBootSector bootSector, string text)
        {
            ulong lba;
            ulong count;

            if (bootSector.Type == FatType32)
            {
                lba = bootSector.ClusterToLba(bootSector.RootCluster);
                count = bootSector.SectorsPerCluster;
            }
            else
            {
                lba = bootSector.RootStartLba;
                count = bootSector.RootSectorCount;
            }

            if (count == 0 || lba >= partition.BlockCount || count > partition.BlockCount - lba)
            {
                return;
            }

            int blockSize = (int)partition.BlockSize;
            byte[] sector = new byte[blockSize];
            ulong freeLba = 0;
            int freeOffset = -1;

            for (ulong s = 0; s < count; s++)
            {
                partition.ReadBlock(lba + s, 1, sector);

                for (int offset = 0; offset + DirectoryEntrySize <= blockSize; offset += DirectoryEntrySize)
                {
                    byte first = sector[offset];
                    byte attributes = sector[offset + AttributesOffset];

                    if (first == 0 || first == DeletedEntry)
                    {
                        if (freeOffset < 0)
                        {
                            freeLba = lba + s;
                            freeOffset = offset;
                        }

                        // 0: no entry after this one.
                        if (first == 0)
                        {
                            s = count;
                            break;
                        }

                        continue;
                    }

                    if ((attributes & LongNameAttributes) == LongNameAttributes || (attributes & VolumeIdAttribute) == 0)
                    {
                        continue;
                    }

                    if (text.Length > 0)
                    {
                        Array.Copy(LabelBytes(text), 0, sector, offset, MaxLabelLength);
                    }
                    else
                    {
                        sector[offset] = DeletedEntry;
                    }

                    partition.WriteBlock(lba + s, 1, sector);
                    return;
                }
            }

            if (text.Length == 0 || freeOffset < 0)
            {
                return;
            }

            partition.ReadBlock(freeLba, 1, sector);
            Array.Clear(sector, freeOffset, DirectoryEntrySize);
            Array.Copy(LabelBytes(text), 0, sector, freeOffset, MaxLabelLength);
            sector[freeOffset + AttributesOffset] = VolumeIdAttribute;
            partition.WriteBlock(freeLba, 1, sector);
        }

        private static byte[] ReadSector(Partition partition, ulong lba)
        {
            byte[] sector = new byte[(int)partition.BlockSize];
            partition.ReadBlock(lba, 1, sector);
            return sector;
        }
    }
}
