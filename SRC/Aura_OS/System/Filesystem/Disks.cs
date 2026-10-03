/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Disks: partition tables, partitions and their filesystems
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

// File-scope usings resolve from the global namespace. Inside `namespace Aura_OS.System.*` the bare
// identifier `System` binds to `Aura_OS.System`, so never write `System.IO.X` in the body.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// The partition table of a disk, as Aura tells them apart.
    /// </summary>
    public enum PartitionTable
    {
        /// <summary>No partition table: a blank disk, or one Aura cannot read.</summary>
        None,

        /// <summary>No partition table, but a filesystem on the whole disk (a "superfloppy").</summary>
        Whole,

        Mbr,

        Gpt,
    }

    /// <summary>
    /// What Aura knows of a partition: its filesystem, where it is mounted, its table entry.
    /// </summary>
    public sealed class PartitionInfo
    {
        public Partition Partition;

        /// <summary>"FAT32", "FAT16", "FAT12", "ext4"..., "unformatted", "unknown" (DetectFilesystem).</summary>
        public string Filesystem;

        /// <summary>The volume label, "" for none.</summary>
        public string Label;

        /// <summary>"/1/", null when not mounted.</summary>
        public string MountPoint;

        /// <summary>Aura runs from it: its volume holds the installed System folder.</summary>
        public bool System;

        /// <summary>A logical partition, in the extended partition of an MBR disk.</summary>
        public bool Logical;

        /// <summary>The MBR's active (boot) flag.</summary>
        public bool Boot;

        /// <summary>The MBR system ID (0x0C...), -1 for none (GPT, no table).</summary>
        public int MbrType = -1;

        /// <summary>The GPT partition type GUID, null for none (MBR, no table).</summary>
        public Guid? GptType;
    }

    /// <summary>
    /// What Aura knows of a disk: its table, where partitions can go, and its partitions in on-disk order.
    /// </summary>
    public sealed class DiskInfo
    {
        public IBlockDevice Device;
        public PartitionTable Table;

        /// <summary>The sectors partitions can take: from FirstUsable to EndUsable (excluded).</summary>
        public ulong FirstUsable;
        public ulong EndUsable;

        /// <summary>The extended partition of an MBR disk, which holds the logical ones; 0 sectors for none.</summary>
        public ulong ExtendedStart;
        public ulong ExtendedSectors;

        /// <summary>The MBR's primary entries in use, the extended one too; 0 for a GPT disk.</summary>
        public int PrimaryCount;

        public List<PartitionInfo> Partitions = new List<PartitionInfo>();

        /// <summary>Why the disk could not be read, null when it could.</summary>
        public string Error;
    }

    /// <summary>
    /// What the Disk Manager (aura.disks) and vol do to the disks: partition tables, partitions and
    /// their FAT volumes. A disk is named by its device name ("sata0"), a partition by the sector it
    /// starts at: a rescan makes new Partition objects and may renumber them. An action that cannot be
    /// done throws an InvalidOperationException saying why; a device error throws too. Each one
    /// unmounts what it changes first (never the system volume) and mounts the FAT volumes again after.
    /// </summary>
    public static class Disks
    {
        /// <summary>The filesystems Format and CreatePartition write.</summary>
        public const string Fat32 = "FAT32";
        public const string Fat16 = "FAT16";
        public const string Fat12 = "FAT12";

        /// <summary>The largest FAT that fits: FAT32, else FAT16, else FAT12.</summary>
        public const string Fat = "FAT";

        /// <summary>No filesystem: the start of the partition wiped.</summary>
        public const string Unformatted = "unformatted";

        public const ulong BytesPerMiB = 1024UL * 1024UL;

        /// <summary>The longest FAT volume label.</summary>
        public const int MaxLabelLength = 11;

        // MBR system IDs of the partitions Aura writes: FAT32 (LBA), FAT16 (LBA), FAT12, and Linux for an
        // unformatted one, as GParted.
        private const byte MbrFat32 = 0x0C;
        private const byte MbrFat16 = 0x0E;
        private const byte MbrFat12 = 0x01;
        private const byte MbrUnformatted = 0x83;

        // The MBR sector's partition table.
        private const int MbrTableOffset = 446;
        private const int MbrEntrySize = 16;
        private const int MbrSlots = 4;
        private const byte MbrActive = 0x80;

        // Partitions start after the MBR sector; an MBR entry addresses 32-bit sectors.
        private const ulong MbrFirstUsable = 1;

        // The bytes read at the start of a partition to tell its filesystem, and those a new or formatted
        // one has wiped: the boot sectors and superblocks of FAT, NTFS, exFAT, ext and Linux swap.
        private const int SignatureBytes = 8192;

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

        private const string NoLabel = "NO NAME";

        // static readonly, NOT const (C16): enum values from a Cosmos assembly.
        private static readonly FatType FatType12 = FatType.Fat12;
        private static readonly FatType FatType16 = FatType.Fat16;
        private static readonly FatType FatType32 = FatType.Fat32;

        #region Reading

        /// <summary>
        /// The disks, in StorageManager's order (vol's disk numbers), each with its partitions.
        /// </summary>
        public static List<DiskInfo> List()
        {
            IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;
            List<DiskInfo> disks = new List<DiskInfo>(devices.Count);

            for (int i = 0; i < devices.Count; i++)
            {
                disks.Add(Describe(devices[i]));
            }

            return disks;
        }

        /// <summary>
        /// The disk named so ("sata0").
        /// </summary>
        /// <exception cref="InvalidOperationException">No disk has that name anymore.</exception>
        public static IBlockDevice FindDisk(string name)
        {
            IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;

            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Name == name)
                {
                    return devices[i];
                }
            }

            throw new InvalidOperationException("There is no disk named " + name + " anymore.");
        }

        /// <summary>
        /// The disk's partition that starts at that sector.
        /// </summary>
        /// <exception cref="InvalidOperationException">It has none there anymore.</exception>
        public static Partition FindPartition(IBlockDevice disk, ulong start)
        {
            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);

            for (int i = 0; i < partitions.Count; i++)
            {
                if (partitions[i].StartSector == start)
                {
                    return partitions[i];
                }
            }

            throw new InvalidOperationException("That partition is not on " + disk.Name + " anymore.");
        }

        /// <summary>
        /// GEN3-GAP(mbr): GPT first, as the scan; a whole-disk filesystem also passes Mbr.IsMbr, its boot
        /// sector carrying the 0xAA55 signature.
        /// </summary>
        public static PartitionTable TableOf(IBlockDevice disk)
        {
            if (Gpt.IsGpt(disk))
            {
                return PartitionTable.Gpt;
            }

            if (IsWholeDisk(StorageManager.GetPartitions(disk)))
            {
                return PartitionTable.Whole;
            }

            return Mbr.IsMbr(disk) ? PartitionTable.Mbr : PartitionTable.None;
        }

        /// <summary>
        /// A filesystem written straight onto the disk: StorageManager gives it as one partition at LBA 0.
        /// </summary>
        public static bool IsWholeDisk(IReadOnlyList<Partition> partitions)
        {
            return partitions.Count == 1 && partitions[0].StartSector == 0;
        }

        private static DiskInfo Describe(IBlockDevice disk)
        {
            DiskInfo info = new DiskInfo();
            info.Device = disk;

            try
            {
                info.Table = TableOf(disk);
                UsableRange(disk, info.Table, out info.FirstUsable, out info.EndUsable);

                IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);
                List<MbrPartitionEntry> logicals = null;
                byte[] mbr = null;
                List<GptPartitionEntry> gpt = null;

                if (info.Table == PartitionTable.Mbr)
                {
                    mbr = ReadSector(disk, 0);
                    info.PrimaryCount = UsedSlots(mbr);

                    ulong extendedStart, extendedSectors;
                    if (Mbr.TryGetExtendedPartition(disk, out extendedStart, out extendedSectors))
                    {
                        info.ExtendedStart = extendedStart;
                        info.ExtendedSectors = extendedSectors;
                        logicals = Ebr.Parse(disk, extendedStart);
                    }
                }
                else if (info.Table == PartitionTable.Gpt)
                {
                    gpt = Gpt.Parse(disk);
                }

                for (int i = 0; i < partitions.Count; i++)
                {
                    info.Partitions.Add(DescribePartition(partitions[i], mbr, logicals, gpt));
                }
            }
            catch (Exception ex)
            {
                // A device error (an NVMe timeout) throws from ReadBlock.
                info.Error = ex.Message;
            }

            return info;
        }

        private static PartitionInfo DescribePartition(Partition partition, byte[] mbr, List<MbrPartitionEntry> logicals, List<GptPartitionEntry> gpt)
        {
            PartitionInfo info = new PartitionInfo();
            info.Partition = partition;
            info.Filesystem = DetectFilesystem(partition, out info.Label);

            VfsManager.VfsMount mount = Volumes.MountOfPartition(partition);
            if (mount != null)
            {
                info.MountPoint = AuraPath.AsDirectory(mount.MountPoint);
                info.System = IsSystemVolume(info.MountPoint);
            }

            if (mbr != null)
            {
                for (int slot = 0; slot < MbrSlots; slot++)
                {
                    int offset = MbrTableOffset + slot * MbrEntrySize;

                    if (mbr[offset + 4] != 0 && BitConverter.ToUInt32(mbr, offset + 8) == partition.StartSector)
                    {
                        info.MbrType = mbr[offset + 4];
                        info.Boot = mbr[offset] == MbrActive;
                    }
                }

                for (int i = 0; logicals != null && i < logicals.Count; i++)
                {
                    if (logicals[i].StartSector == partition.StartSector)
                    {
                        info.MbrType = logicals[i].SystemId;
                        info.Logical = true;
                    }
                }
            }

            for (int i = 0; gpt != null && i < gpt.Count; i++)
            {
                if (gpt[i].StartSector == partition.StartSector)
                {
                    info.GptType = gpt[i].PartitionType;
                }
            }

            return info;
        }

        /// <summary>
        /// The sectors partitions can take on a disk with that table: from first to end (excluded). A
        /// GPT keeps its own header's range; an MBR entry addresses 32-bit sectors.
        /// </summary>
        public static void UsableRange(IBlockDevice disk, PartitionTable table, out ulong first, out ulong end)
        {
            if (table == PartitionTable.Gpt)
            {
                byte[] header = ReadSector(disk, Gpt.PrimaryHeaderLba);
                ulong firstUsable = BitConverter.ToUInt64(header, 40);
                ulong lastUsable = BitConverter.ToUInt64(header, 48);

                first = Math.Max(firstUsable, Gpt.FirstUsableLba);
                end = lastUsable < disk.BlockCount ? lastUsable + 1 : disk.BlockCount;
            }
            else if (table == PartitionTable.Mbr)
            {
                first = MbrFirstUsable;
                end = Math.Min(disk.BlockCount, uint.MaxValue);
            }
            else
            {
                first = 0;
                end = disk.BlockCount;
            }

            if (end < first)
            {
                end = first;
            }
        }

        /// <summary>
        /// The filesystem of a partition from its first bytes: "FAT32", "FAT16", "FAT12" (with its label),
        /// "ext2", "ext3", "ext4" (with its label), "NTFS", "exFAT", "linux-swap", "unformatted" (only
        /// zeros) or "unknown"; "unreadable" when the read fails.
        /// GEN3-GAP(fat-label): no API reads a FAT label, so it comes from the BPB (11 bytes at 0x2B on
        /// FAT12/16, 0x47 on FAT32, there when the extended boot signature is 0x29).
        /// </summary>
        public static string DetectFilesystem(Partition partition, out string label)
        {
            label = "";

            if (partition == null || partition.BlockSize < 512 || partition.BlockSize > 4096 || partition.BlockCount == 0)
            {
                return "unknown";
            }

            int blockSize = (int)partition.BlockSize;
            ulong sectors = Math.Min(partition.BlockCount, (ulong)((SignatureBytes + blockSize - 1) / blockSize));
            byte[] head = new byte[(int)sectors * blockSize];

            try
            {
                partition.ReadBlock(0, sectors, head);
            }
            catch (Exception)
            {
                return "unreadable";
            }

            if (HasText(head, 3, "NTFS    "))
            {
                return "NTFS";
            }

            if (HasText(head, 3, "EXFAT   "))
            {
                return "exFAT";
            }

            byte[] boot = new byte[blockSize];
            Array.Copy(head, boot, blockSize);

            FatBootSector bootSector;
            if (FatBootSector.TryParse(boot, out bootSector) && bootSector != null)
            {
                return FatName(bootSector, boot, out label);
            }

            // ext2/3/4: the superblock at byte 1024, its magic 0xEF53 at 56.
            if (head.Length >= 2048 && head[1024 + 56] == 0x53 && head[1024 + 57] == 0xEF)
            {
                label = ReadText(head, 1024 + 120, 16);

                uint compatible = BitConverter.ToUInt32(head, 1024 + 92);
                uint incompatible = BitConverter.ToUInt32(head, 1024 + 96);

                // Extents, 64-bit or flexible block groups: ext4; a journal: ext3.
                if ((incompatible & 0x2C0) != 0)
                {
                    return "ext4";
                }

                return (compatible & 0x4) != 0 ? "ext3" : "ext2";
            }

            // Linux swap: its signature ends the first 4 KiB page.
            if (head.Length >= 4096 && (HasText(head, 4086, "SWAPSPACE2") || HasText(head, 4086, "SWAP-SPACE")))
            {
                label = ReadText(head, 1024 + 28, 16);
                return "linux-swap";
            }

            for (int i = 0; i < head.Length; i++)
            {
                if (head[i] != 0)
                {
                    return "unknown";
                }
            }

            return Unformatted;
        }

        private static string FatName(FatBootSector bootSector, byte[] boot, out string label)
        {
            label = "";
            bool fat32 = bootSector.Type == FatType32;

            if (boot[fat32 ? Fat32SignatureOffset : Fat1216SignatureOffset] == ExtendedBootSignature)
            {
                label = ReadText(boot, fat32 ? Fat32LabelOffset : Fat1216LabelOffset, MaxLabelLength);

                // What FAT formatters write for no label.
                if (label == NoLabel)
                {
                    label = "";
                }
            }

            switch (bootSector.Type)
            {
                case FatType.Fat12:
                    return Fat12;
                case FatType.Fat16:
                    return Fat16;
                case FatType.Fat32:
                    return Fat32;
                default:
                    return Fat;
            }
        }

        /// <summary>
        /// Whether that volume ("/0/") is the one Aura runs from: the installed System folder is on it.
        /// In live mode SystemVolume falls back to "/0/" without settings.ini, and that one can change.
        /// </summary>
        public static bool IsSystemVolume(string volume)
        {
            return AuraPaths.SystemVolume != null && volume == AuraPaths.SystemVolume
                && File.Exists(volume + "System/settings.ini");
        }

        #endregion

        #region Actions

        /// <summary>
        /// Writes a new, empty partition table: every partition of the disk is lost.
        /// </summary>
        public static void CreateTable(IBlockDevice disk, bool gpt)
        {
            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);

            // All of them checked before any is unmounted.
            for (int i = 0; i < partitions.Count; i++)
            {
                CheckNotSystem(partitions[i]);
            }

            for (int i = 0; i < partitions.Count; i++)
            {
                Release(partitions[i]);
            }

            try
            {
                if (gpt)
                {
                    Gpt.Create(disk);
                    GptChecksums.Update(disk);
                }
                else
                {
                    Mbr.Create(disk);

                    // The backup GPT header of a GPT disk would make other systems take it for a damaged GPT.
                    if (GptChecksums.HasBackupHeader(disk))
                    {
                        disk.WriteBlock(disk.BlockCount - 1, 1, new byte[(int)disk.BlockSize]);
                    }
                }

                disk.Flush();
            }
            finally
            {
                StorageManager.RescanPartitions(disk);
                Volumes.MountAll();
            }
        }

        /// <summary>
        /// Adds a partition in free space, then writes its filesystem: FAT32, FAT16, FAT12, FAT (the
        /// largest that fits) or unformatted (its start wiped). On an MBR disk, free space inside the
        /// extended partition gets a logical partition, which goes after the last one.
        /// </summary>
        /// <param name="label">The FAT volume label, "" or null for none.</param>
        /// <returns>The sector the partition starts at: a logical one's is the one after its EBR.</returns>
        public static ulong CreatePartition(IBlockDevice disk, ulong start, ulong sectors, string filesystem, string label)
        {
            byte mbrType = MbrTypeOf(filesystem);
            CheckLabel(label);

            PartitionTable table = TableOf(disk);

            if (table == PartitionTable.None)
            {
                throw new InvalidOperationException(disk.Name + " has no partition table: create one first.");
            }

            if (table == PartitionTable.Whole)
            {
                throw new InvalidOperationException(disk.Name + " has a filesystem on the whole disk, with no partition table: create one first (its files will be lost).");
            }

            ulong first, end;
            UsableRange(disk, table, out first, out end);

            if (sectors == 0 || start < first || start >= end || sectors > end - start)
            {
                throw new InvalidOperationException("The partition does not fit in the disk's free space.");
            }

            ulong begin = start;
            ulong extendedStart, extendedSectors;

            if (table == PartitionTable.Mbr && Mbr.TryGetExtendedPartition(disk, out extendedStart, out extendedSectors)
                && start >= extendedStart && start < extendedStart + extendedSectors)
            {
                if (!PartitionManager.TryCreateLogical(disk, mbrType, sectors, out begin))
                {
                    throw new InvalidOperationException("There is no room for it after the last logical partition.");
                }
            }
            else
            {
                if (table == PartitionTable.Mbr && UsedSlots(ReadSector(disk, 0)) >= MbrSlots)
                {
                    throw new InvalidOperationException("An MBR disk holds 4 primary partitions at most: delete one first, or use a GPT.");
                }

                if (!PartitionManager.Create(disk, start, sectors, mbrType, Gpt.BasicDataPartitionType))
                {
                    throw new InvalidOperationException("The partition table refused it: it overlaps another partition, or the table is full.");
                }

                if (table == PartitionTable.Gpt)
                {
                    GptChecksums.Update(disk);
                }
            }

            disk.Flush();
            StorageManager.RescanPartitions(disk);

            try
            {
                Partition partition = FindPartition(disk, begin);
                Wipe(partition);

                if (filesystem != Unformatted)
                {
                    FormatFat(partition, filesystem, label);

                    // "FAT" may have written FAT16 or FAT12.
                    if (table == PartitionTable.Mbr)
                    {
                        MatchMbrType(disk, partition);
                    }
                }
            }
            catch (Exception ex)
            {
                // A partition without the filesystem asked for is not what was asked for: it goes again.
                RemoveCreated(disk, table, begin, sectors);
                Volumes.MountAll();
                throw new InvalidOperationException(ex.Message, ex);
            }

            Volumes.MountAll();
            return begin;
        }

        /// <summary>
        /// Takes a partition CreatePartition added out of the table again, when its filesystem could not
        /// be written. Best effort: the error that led here is the one to report.
        /// </summary>
        private static void RemoveCreated(IBlockDevice disk, PartitionTable table, ulong start, ulong sectors)
        {
            try
            {
                if (PartitionManager.Delete(disk, new PartitionManager.PartitionLocation(start, sectors)) && table == PartitionTable.Gpt)
                {
                    GptChecksums.Update(disk);
                }

                disk.Flush();
                StorageManager.RescanPartitions(disk);
            }
            catch (Exception)
            {
                // The partition stays, unformatted.
            }
        }

        /// <summary>
        /// Removes a partition from the table. Its files stay on the disk, out of reach.
        /// </summary>
        public static void DeletePartition(IBlockDevice disk, Partition partition)
        {
            PartitionTable table = TableOf(disk);

            if (table == PartitionTable.Whole)
            {
                throw new InvalidOperationException("The filesystem takes the whole of " + disk.Name + ", with no partition table: create one to remove it.");
            }

            Release(partition);

            try
            {
                if (!PartitionManager.Delete(disk, new PartitionManager.PartitionLocation(partition.StartSector, partition.BlockCount)))
                {
                    throw new InvalidOperationException("The partition table did not let Aura delete it.");
                }

                if (table == PartitionTable.Gpt)
                {
                    GptChecksums.Update(disk);
                }

                disk.Flush();
            }
            finally
            {
                StorageManager.RescanPartitions(disk);
                Volumes.MountAll();
            }
        }

        /// <summary>
        /// Writes a new filesystem on a partition (FAT32, FAT16, FAT12, FAT or unformatted): its files are
        /// lost. The MBR entry of a primary partition gets the matching system ID.
        /// </summary>
        public static void Format(IBlockDevice disk, Partition partition, string filesystem, string label)
        {
            // Both checked before the partition is unmounted.
            MbrTypeOf(filesystem);
            CheckLabel(label);
            Release(partition);

            try
            {
                Wipe(partition);

                if (filesystem != Unformatted)
                {
                    FormatFat(partition, filesystem, label);
                }

                if (TableOf(disk) == PartitionTable.Mbr)
                {
                    MatchMbrType(disk, partition);
                }
            }
            finally
            {
                Volumes.MountAll();
            }
        }

        /// <summary>
        /// Moves and resizes a partition: the move copies its sectors (slow for a large one), the resize
        /// only changes the table. A FAT volume keeps its size, so it cannot get smaller than that.
        /// </summary>
        public static void Resize(IBlockDevice disk, Partition partition, ulong newStart, ulong newSectors)
        {
            ulong start = partition.StartSector;
            ulong sectors = partition.BlockCount;

            if (newStart == start && newSectors == sectors)
            {
                return;
            }

            PartitionTable table = TableOf(disk);

            if (table != PartitionTable.Mbr && table != PartitionTable.Gpt)
            {
                throw new InvalidOperationException(disk.Name + " has no partition table: there is nothing to resize.");
            }

            ulong first, end;
            UsableRange(disk, table, out first, out end);

            if (newSectors == 0 || newStart < first || newStart >= end || newSectors > end - newStart)
            {
                throw new InvalidOperationException("The partition does not fit there.");
            }

            ulong volumeSectors = FatVolumeSectors(partition);

            if (newSectors < volumeSectors)
            {
                throw new InvalidOperationException("Its FAT volume takes " + (volumeSectors * partition.BlockSize / BytesPerMiB)
                    + " MB, and Aura cannot shrink a FAT volume: format it first.");
            }

            Release(partition);

            string failed = null;

            try
            {
                PartitionManager.PartitionLocation location = new PartitionManager.PartitionLocation(start, sectors);

                // Shrunk first when it gets smaller, moved first when it gets larger: either way the sectors
                // it takes in between are its own or free.
                if (newSectors < sectors)
                {
                    if (!PartitionManager.Resize(disk, location, newSectors))
                    {
                        failed = "Aura could not resize it.";
                    }
                    else if (newStart != start && !PartitionManager.MoveWithData(disk, new PartitionManager.PartitionLocation(start, newSectors), newStart))
                    {
                        failed = "Aura resized it, but could not move it.";
                    }
                }
                else
                {
                    if (newStart != start && !PartitionManager.MoveWithData(disk, location, newStart))
                    {
                        failed = "Aura could not move it there.";
                    }
                    else if (newSectors != sectors && !PartitionManager.Resize(disk, new PartitionManager.PartitionLocation(newStart, sectors), newSectors))
                    {
                        failed = newStart != start ? "Aura moved it, but could not resize it." : "Aura could not resize it.";
                    }
                }

                if (table == PartitionTable.Gpt)
                {
                    GptChecksums.Update(disk);
                }

                disk.Flush();
            }
            finally
            {
                StorageManager.RescanPartitions(disk);
                Volumes.MountAll();
            }

            if (failed != null)
            {
                throw new InvalidOperationException(failed + " The space around it may be taken, or the table refused it.");
            }
        }

        /// <summary>
        /// Changes the label of a FAT volume (up to 11 letters, digits, spaces, - and _, written in capitals;
        /// "" for none): in its boot sector, its FAT32 backup, and its root folder's label entry, which
        /// other systems show.
        /// </summary>
        public static void SetLabel(Partition partition, string label)
        {
            string text = CheckLabel(label);
            byte[] boot = ReadPartitionSector(partition, 0);

            FatBootSector bootSector;
            if (!FatBootSector.TryParse(boot, out bootSector) || bootSector == null)
            {
                throw new InvalidOperationException("Aura only labels FAT volumes.");
            }

            bool fat32 = bootSector.Type == FatType32;
            int labelOffset = fat32 ? Fat32LabelOffset : Fat1216LabelOffset;

            if (boot[fat32 ? Fat32SignatureOffset : Fat1216SignatureOffset] != ExtendedBootSignature)
            {
                throw new InvalidOperationException("This FAT volume has no room for a label.");
            }

            Release(partition);

            try
            {
                byte[] name = LabelBytes(text.Length > 0 ? text : NoLabel);

                Array.Copy(name, 0, boot, labelOffset, MaxLabelLength);
                partition.WriteBlock(0, 1, boot);

                if (fat32)
                {
                    ulong backup = BitConverter.ToUInt16(boot, Fat32BackupBootOffset);

                    if (backup > 0 && backup < bootSector.ReservedSectorCount)
                    {
                        byte[] backupBoot = ReadPartitionSector(partition, backup);
                        Array.Copy(name, 0, backupBoot, labelOffset, MaxLabelLength);
                        partition.WriteBlock(backup, 1, backupBoot);
                    }
                }

                SetRootLabel(partition, bootSector, text);
                partition.Flush();
            }
            finally
            {
                Volumes.MountAll();
            }
        }

        /// <summary>
        /// Mounts a FAT partition at the next free /N: its mount point ("/1/").
        /// </summary>
        public static string Mount(Partition partition)
        {
            string mountPoint = Volumes.Mount(partition);

            if (mountPoint == null)
            {
                throw new InvalidOperationException("Aura only mounts FAT volumes.");
            }

            return mountPoint;
        }

        /// <summary>
        /// Unmounts a partition, its last writes flushed, until a reboot or the next change to its disk.
        /// </summary>
        public static void Unmount(Partition partition)
        {
            if (Volumes.MountOfPartition(partition) == null)
            {
                throw new InvalidOperationException("It is not mounted.");
            }

            Release(partition);
        }

        /// <summary>
        /// Unmounts a partition (flushed) before it changes. The current directory leaves it.
        /// GEN3-GAP(mounts): VfsManager guards a format by Partition reference only, so by location here.
        /// </summary>
        /// <exception cref="InvalidOperationException">It holds the system volume.</exception>
        public static void Release(Partition partition)
        {
            CheckNotSystem(partition);

            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
            List<string> mountPoints = new List<string>();

            for (int i = 0; i < mounts.Count; i++)
            {
                Partition p = mounts[i].Partition;

                if (p != null && ReferenceEquals(p.Host, partition.Host) && p.StartSector == partition.StartSector)
                {
                    mountPoints.Add(mounts[i].MountPoint);
                }
            }

            for (int i = 0; i < mountPoints.Count; i++)
            {
                try
                {
                    VfsManager.TryUnmount(mountPoints[i]);
                }
                catch (Exception)
                {
                    // The mount is already out of the table; only its last writes may be lost.
                }
            }

            if (mountPoints.Count > 0)
            {
                Volumes.RefreshSystemVolume();
                Volumes.LeaveVanishedVolume();
            }
        }

        private static void CheckNotSystem(Partition partition)
        {
            VfsManager.VfsMount mount = Volumes.MountOfPartition(partition);

            if (mount != null && IsSystemVolume(AuraPath.AsDirectory(mount.MountPoint)))
            {
                throw new InvalidOperationException("Aura runs from this partition (" + AuraPath.AsDirectory(mount.MountPoint) + "): it cannot change while Aura uses it.");
            }
        }

        #endregion

        #region Filesystems

        /// <summary>
        /// FAT32, FAT16 or FAT12 with the smallest clusters that fit, or the largest of them that fits for
        /// "FAT". The formatter writes nothing for a geometry it refuses, so each one is tried in turn.
        /// </summary>
        private static void FormatFat(Partition partition, string filesystem, string label)
        {
            string text = CheckLabel(label);
            string volumeLabel = text.Length > 0 ? text : null;
            bool formatted;

            switch (filesystem)
            {
                case Fat32:
                    formatted = TryFormat(partition, FatType32, volumeLabel);
                    break;
                case Fat16:
                    formatted = TryFormat(partition, FatType16, volumeLabel);
                    break;
                case Fat12:
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
                string size = (partition.BlockCount * partition.BlockSize / BytesPerMiB) + " MB";
                throw new InvalidOperationException("No " + (filesystem == Fat ? "FAT" : filesystem) + " volume fits in " + size
                    + ": FAT12 takes up to 127 MB, FAT16 3 MB to 2 GB, FAT32 33 MB and more.");
            }

            // The formatter writes the label in the boot sector only: other systems read the root folder's
            // label entry, and fsck takes a boot sector label without one for a mistake.
            FatBootSector bootSector;
            if (text.Length > 0 && FatBootSector.TryParse(ReadPartitionSector(partition, 0), out bootSector) && bootSector != null)
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
        /// The sectors the FAT volume of a partition takes, 0 when it holds none.
        /// </summary>
        private static ulong FatVolumeSectors(Partition partition)
        {
            FatBootSector bootSector;
            byte[] boot = ReadPartitionSector(partition, 0);
            return FatBootSector.TryParse(boot, out bootSector) && bootSector != null ? bootSector.TotalSectorCount : 0;
        }

        private static byte MbrTypeOf(string filesystem)
        {
            switch (filesystem)
            {
                case Fat32:
                case Fat:
                    return MbrFat32;
                case Fat16:
                    return MbrFat16;
                case Fat12:
                    return MbrFat12;
                case Unformatted:
                    return MbrUnformatted;
            }

            throw new InvalidOperationException("Aura writes FAT32, FAT16, FAT12 or unformatted partitions, not '" + filesystem + "'.");
        }

        /// <summary>
        /// Gives a primary partition's MBR entry the system ID of the filesystem it holds now. Nothing for
        /// a logical one: its entry is in an EBR.
        /// </summary>
        private static void MatchMbrType(IBlockDevice disk, Partition partition)
        {
            string label;
            byte systemId;

            switch (DetectFilesystem(partition, out label))
            {
                case Fat32:
                    systemId = MbrFat32;
                    break;
                case Fat16:
                    systemId = MbrFat16;
                    break;
                case Fat12:
                    systemId = MbrFat12;
                    break;
                case Unformatted:
                    systemId = MbrUnformatted;
                    break;
                default:
                    return;
            }

            SetMbrType(disk, partition, systemId);
        }

        private static void SetMbrType(IBlockDevice disk, Partition partition, byte systemId)
        {
            byte[] mbr = ReadSector(disk, 0);

            for (int slot = 0; slot < MbrSlots; slot++)
            {
                int offset = MbrTableOffset + slot * MbrEntrySize;
                byte current = mbr[offset + 4];

                if (current != 0 && current != systemId && BitConverter.ToUInt32(mbr, offset + 8) == partition.StartSector
                    && BitConverter.ToUInt32(mbr, offset + 12) == partition.BlockCount)
                {
                    mbr[offset + 4] = systemId;
                    disk.WriteBlock(0, 1, mbr);
                    disk.Flush();
                    return;
                }
            }
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

        /// <summary>
        /// Zeroes the start of a partition, where filesystems keep their signatures: a stale one would
        /// make the new partition, or its new filesystem, look like the old one.
        /// </summary>
        private static void Wipe(Partition partition)
        {
            int blockSize = (int)partition.BlockSize;
            ulong sectors = Math.Min(partition.BlockCount, (ulong)((SignatureBytes + blockSize - 1) / blockSize));

            if (sectors > 0)
            {
                partition.WriteBlock(0, sectors, new byte[(int)sectors * blockSize]);
                partition.Flush();
            }
        }

        #endregion

        #region Sectors

        private static byte[] ReadSector(IBlockDevice disk, ulong lba)
        {
            byte[] sector = new byte[(int)disk.BlockSize];
            disk.ReadBlock(lba, 1, sector);
            return sector;
        }

        private static byte[] ReadPartitionSector(Partition partition, ulong lba)
        {
            byte[] sector = new byte[(int)partition.BlockSize];
            partition.ReadBlock(lba, 1, sector);
            return sector;
        }

        /// <summary>
        /// The MBR's entries in use, the extended one too.
        /// </summary>
        private static int UsedSlots(byte[] mbr)
        {
            int used = 0;

            for (int slot = 0; slot < MbrSlots; slot++)
            {
                if (mbr[MbrTableOffset + slot * MbrEntrySize + 4] != 0)
                {
                    used++;
                }
            }

            return used;
        }

        private static bool HasText(byte[] data, int offset, string text)
        {
            if (offset + text.Length > data.Length)
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (data[offset + i] != text[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A label field: its printable ASCII characters, trimmed (ext and swap pad with zeros, FAT with spaces).
        /// </summary>
        private static string ReadText(byte[] data, int offset, int length)
        {
            StringBuilder text = new StringBuilder(length);

            for (int i = 0; i < length && offset + i < data.Length; i++)
            {
                byte b = data[offset + i];

                if (b == 0)
                {
                    break;
                }

                text.Append(b >= 0x20 && b < 0x7F ? (char)b : '?');
            }

            return text.ToString().Trim();
        }

        #endregion
    }
}
