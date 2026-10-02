/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Interpreter;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandVol : ICommand
    {
        // gen2 numbering: the N-th FAT volume is mounted at /N (gen2 "N:\").
        // Templates: Cosmos DevKernel Storage/StorageView.cs, Commands/PartitionCommands.cs, MountCommands.cs.

        /// <summary>MBR system ID of the partitions vol /mp creates (FAT32 LBA).</summary>
        private const byte MbrFat32LbaSystemId = 0x0C;

        /// <summary>First partition LBA on an MBR disk (1 MiB alignment).</summary>
        private const ulong MbrFirstPartitionLba = 2048;

        private const ulong BytesPerMiB = 1024UL * 1024UL;

        private const string FormatLabel = "AURAOS";

        // static readonly, NOT const (C16): enum values from a Cosmos assembly.
        private static readonly FatType Fat32 = FatType.Fat32;
        private static readonly FatType FatAuto = FatType.Unknown;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVol(string[] commandvalues) : base(commandvalues)
        {
            Description = "to change, list and edit volumes";
        }

        public override ReturnInfo Execute()
        {
            return ListVolumes();
        }

        /// <summary>
        /// CommandChangeVol
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return ListVolumes();
            }

            try
            {
                if (arguments[0].Equals("/l") || arguments[0].Equals("/list"))
                {
                    return ListVolumes();
                }
                else if (arguments[0].Equals("/cv") || arguments[0].Equals("/changevol"))
                {
                    if (arguments.Count < 2)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    return ChangeVolume(arguments[1]);
                }
                else if (arguments[0].Equals("/fp") || arguments[0].Equals("/formatpartiton"))
                {
                    int disk, partition;
                    if (!TryParseTwo(arguments, out disk, out partition))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    return FormatVolume(disk, partition);
                }
                else if (arguments[0].Equals("/lp") || arguments[0].Equals("/listpartitions"))
                {
                    return ListDisk();
                }
                else if (arguments[0].Equals("/mp") || arguments[0].Equals("/makepartition"))
                {
                    int disk, size;
                    if (!TryParseTwo(arguments, out disk, out size))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    return MakeDisk(disk, size);
                }
                else if (arguments[0].Equals("/dp") || arguments[0].Equals("/deletepartition"))
                {
                    int disk, partition;
                    if (!TryParseTwo(arguments, out disk, out partition))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    return DeleteDisk(disk, partition);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
            }
            catch (Exception ex)
            {
                // A device error (e.g. an NVMe timeout) throws from ReadBlock/WriteBlock.
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        private static bool TryParseTwo(List<string> arguments, out int first, out int second)
        {
            first = 0;
            second = 0;

            return arguments.Count >= 3
                && int.TryParse(arguments[1], out first)
                && int.TryParse(arguments[2], out second);
        }

        public ReturnInfo DeleteDisk(int disknumber, int index)
        {
            IBlockDevice disk = StorageManager.GetDevice(disknumber);
            if (disk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);
            if (index < 1 || index > partitions.Count)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find partition #" + index + " on disk #" + disknumber + ".");
            }

            Partition target = partitions[index - 1];

            if (IsSuperfloppy(partitions))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Disk #" + disknumber + " has no partition table (whole-disk FAT volume).");
            }

            string error;
            if (!ReleasePartition(target, out error))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, error);
            }

            // Identity is (start, count), not an index.
            if (!PartitionManager.Delete(disk, new PartitionManager.PartitionLocation(target.StartSector, target.BlockCount)))
            {
                Volumes.MountAll();
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to delete partition #" + index + " on disk #" + disknumber + ".");
            }

            Console.WriteLine("Partition #" + index + " deleted on disk #" + disknumber + "!");

            StorageManager.RescanPartitions(disk);
            Volumes.MountAll();

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo MakeDisk(int disknumber, int size)
        {
            if (size <= 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition size must be greater than 0 MB.");
            }

            IBlockDevice disk = StorageManager.GetDevice(disknumber);
            if (disk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            // Divisor check: a null/zero divide halts the gen3 kernel.
            if (disk.BlockSize == 0 || disk.BlockSize > BytesPerMiB)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Unsupported sector size on disk #" + disknumber + ".");
            }

            // GEN3-GAP(mbr): Mbr.IsMbr only checks 0x55AA, which a FAT "superfloppy" (no partition table)
            // also carries: a new entry would be written into its boot sector.
            if (IsSuperfloppy(StorageManager.GetPartitions(disk)))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Disk #" + disknumber + " holds a whole-disk FAT volume (no partition table), refusing to create a partition.");
            }

            // GPT first: a GPT disk also carries a protective MBR.
            bool isGpt = Gpt.IsGpt(disk);
            if (!isGpt && !Mbr.IsMbr(disk))
            {
                // Blank disk: write an empty MBR first, then rescan before computing the append point.
                Mbr.Create(disk);
                StorageManager.RescanPartitions(disk);
                Console.WriteLine("No partition table on disk #" + disknumber + ", MBR created.");
            }

            ulong start = isGpt ? Gpt.FirstUsableLba : MbrFirstPartitionLba;

            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);
            for (int i = 0; i < partitions.Count; i++)
            {
                ulong end = partitions[i].StartSector + partitions[i].BlockCount;
                if (end > start)
                {
                    start = end;
                }
            }

            ulong sectors = (ulong)size * (BytesPerMiB / disk.BlockSize);

            if (start >= disk.BlockCount || sectors > disk.BlockCount - start)
            {
                ulong free = start < disk.BlockCount ? (disk.BlockCount - start) * disk.BlockSize / BytesPerMiB : 0;
                return new ReturnInfo(this, ReturnCode.ERROR, "Not enough free space on disk #" + disknumber + " (" + free + " MB available).");
            }

            if (!PartitionManager.Create(disk, start, sectors, MbrFat32LbaSystemId, Gpt.BasicDataPartitionType))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to create partition (no free slot or bad geometry).");
            }

            StorageManager.RescanPartitions(disk);

            Console.WriteLine("Partition created on disk #" + disknumber + "!");

            // A fresh partition holds no filesystem: use vol /fp to format it.
            Volumes.MountAll();

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo DiskInfo(int disknumber)
        {
            IBlockDevice disk = StorageManager.GetDevice(disknumber);
            if (disk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            DisplayInformation(disknumber, disk);

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListDisk()
        {
            if (!StorageManager.IsEnabled)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Storage support is disabled.");
            }

            IReadOnlyList<IBlockDevice> disks = StorageManager.Devices;

            if (disks.Count == 0)
            {
                Console.WriteLine("No disk detected (only SATA/AHCI, NVMe and USB disks are supported).");
                return new ReturnInfo(this, ReturnCode.OK);
            }

            for (int i = 0; i < disks.Count; i++)
            {
                DisplayInformation(i, disks[i]);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo FormatVolume(int driveName, int partition)
        {
            IBlockDevice disk = StorageManager.GetDevice(driveName);
            if (disk == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);
            if (partition < 1 || partition > partitions.Count)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find partition #" + partition + " on disk #" + driveName + ".");
            }

            Partition target = partitions[partition - 1];

            string error;
            if (!ReleasePartition(target, out error))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, error);
            }

            // FAT32 first (gen2 behaviour); a partition too small for FAT32 gets FAT12/16.
            bool formatted = VfsManager.TryFormat(Volumes.FatDriver, target, new FatFormatOptions { Type = Fat32, VolumeLabel = FormatLabel })
                || VfsManager.TryFormat(Volumes.FatDriver, target, new FatFormatOptions { Type = FatAuto, VolumeLabel = FormatLabel });

            if (!formatted)
            {
                Volumes.MountAll();
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to format partition #" + partition + " on disk #" + driveName + ".");
            }

            string label;
            Console.WriteLine("Partition #" + partition + " formatted to " + DetectFilesystem(target, out label) + " on disk #" + driveName + "!");

            Volumes.MountAll();

            VfsManager.VfsMount mount = FindMount(target);
            if (mount != null)
            {
                Console.WriteLine("Mounted at " + AuraPath.AsDirectory(mount.MountPoint));
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ChangeVolume(string volume)
        {
            // Accepts "1", "1:", "1:\" (gen2) and "/1", "/1/".
            string number = volume.Trim().TrimEnd('\\', '/').TrimEnd(':').TrimStart('/');

            VfsManager.VfsMount mount;
            if (number.Length == 0 || !VfsManager.TryGetMount("/" + number, out mount))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The specified drive is not found.");
            }

            Kernel.CurrentVolume = AuraPath.AsDirectory(mount.MountPoint);
            Kernel.CurrentDirectory = Kernel.CurrentVolume;

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListVolumes()
        {
            IReadOnlyList<VfsManager.VfsMount> vols = VfsManager.Mounts;

            Console.WriteLine();
            Console.WriteLine("  Volume ###\tFormat\tSize\t\tLabel\t\tPartition");
            Console.WriteLine("  ----------\t------\t--------\t-----------\t---------");

            if (vols.Count == 0)
            {
                Console.WriteLine("  No volume mounted.");
            }

            for (int i = 0; i < vols.Count; i++)
            {
                VfsManager.VfsMount vol = vols[i];
                string name = AuraPath.AsDirectory(vol.MountPoint);
                string label = "";
                string format = vol.Partition != null ? DetectFilesystem(vol.Partition, out label) : vol.Name;
                string parent = vol.Partition != null ? vol.Partition.Name : vol.Source;

                // GEN3-GAP(driveinfo): TryStatFs sweeps the whole FAT (fine for a user command).
                ulong freeBytes, totalBytes;
                string size = Volumes.TryGetSpace(vol.MountPoint, out freeBytes, out totalBytes)
                    ? (totalBytes / BytesPerMiB) + " MB"
                    : "? MB";

                string marker = name == Kernel.CurrentVolume && vols.Count > 1 ? " >" : "  ";

                Console.WriteLine(marker + name + "\t   \t" + format + " \t" + size + "\t\t" + label + "\t\t" + parent);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Prints one disk, its table type and its partitions (1-based, the numbering of /fp and /dp).
        /// </summary>
        private static void DisplayInformation(int disknumber, IBlockDevice disk)
        {
            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);

            Console.WriteLine();
            Console.WriteLine("Disk #: " + disknumber + " (" + DescribePartitionTable(disk, partitions) + ")");
            Console.WriteLine("  Name: " + disk.Name + ", " + (disk.BlockCount * disk.BlockSize / BytesPerMiB) + " MB, "
                + disk.BlockCount + " sectors of " + disk.BlockSize + " bytes");

            if (partitions.Count == 0)
            {
                Console.WriteLine("  (no partitions)");
            }

            for (int i = 0; i < partitions.Count; i++)
            {
                Partition part = partitions[i];
                string label;
                string format = DetectFilesystem(part, out label);
                VfsManager.VfsMount mount = FindMount(part);

                Console.WriteLine("  Partition #" + (i + 1) + ": " + part.Name
                    + "  Start=" + part.StartSector
                    + "  Sectors=" + part.BlockCount
                    + "  " + (part.BlockCount * part.BlockSize / BytesPerMiB) + " MB"
                    + "  " + format
                    + (label.Length > 0 ? " \"" + label + "\"" : "")
                    + (mount != null ? "  mounted at " + AuraPath.AsDirectory(mount.MountPoint) : ""));
            }
        }

        /// <summary>
        /// GEN3-GAP(mbr): GPT first, then MBR; a superfloppy also passes Mbr.IsMbr.
        /// </summary>
        private static string DescribePartitionTable(IBlockDevice disk, IReadOnlyList<Partition> partitions)
        {
            try
            {
                if (Gpt.IsGpt(disk))
                {
                    return "GPT";
                }

                if (IsSuperfloppy(partitions))
                {
                    return "None (whole-disk FAT)";
                }

                if (Mbr.IsMbr(disk))
                {
                    return "MBR";
                }
            }
            catch (Exception)
            {
                return "unreadable";
            }

            return "None";
        }

        /// <summary>
        /// A FAT volume formatted straight onto the disk: StorageManager surfaces it as one partition at LBA 0.
        /// </summary>
        private static bool IsSuperfloppy(IReadOnlyList<Partition> partitions)
        {
            return partitions.Count == 1 && partitions[0].StartSector == 0;
        }

        /// <summary>
        /// FAT type from the boot sector (DevKernel StorageView.DetectFilesystem).
        /// GEN3-GAP(fat-label): no API reads the volume label, so it comes from the BPB
        /// (11 bytes at 0x2B on FAT12/16, 0x47 on FAT32, present when the boot signature is 0x29).
        /// </summary>
        private static string DetectFilesystem(Partition partition, out string label)
        {
            label = "";

            if (partition == null || partition.BlockSize < 512 || partition.BlockSize > 4096)
            {
                return "unknown";
            }

            byte[] boot = new byte[partition.BlockSize];
            try
            {
                partition.ReadBlock(FatBootSector.BootSectorLba, 1, boot);
            }
            catch (Exception)
            {
                return "unreadable";
            }

            FatBootSector bootSector;
            if (!FatBootSector.TryParse(boot, out bootSector) || bootSector == null)
            {
                return "unknown";
            }

            string type;
            bool fat32 = false;

            switch (bootSector.Type)
            {
                case FatType.Fat12:
                    type = "FAT12";
                    break;
                case FatType.Fat16:
                    type = "FAT16";
                    break;
                case FatType.Fat32:
                    type = "FAT32";
                    fat32 = true;
                    break;
                default:
                    type = "FAT";
                    break;
            }

            int signatureOffset = fat32 ? 0x42 : 0x26;
            int labelOffset = fat32 ? 0x47 : 0x2B;

            if (boot[signatureOffset] == 0x29)
            {
                label = Encoding.ASCII.GetString(boot, labelOffset, 11).Trim();
            }

            return type;
        }

        /// <summary>
        /// The mount of a partition, by location (Host + StartSector): a rescan creates new Partition objects.
        /// </summary>
        private static VfsManager.VfsMount FindMount(Partition partition)
        {
            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
            for (int i = 0; i < mounts.Count; i++)
            {
                Partition p = mounts[i].Partition;
                if (p != null && ReferenceEquals(p.Host, partition.Host) && p.StartSector == partition.StartSector)
                {
                    return mounts[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Unmounts (flush + detach) every mount of a partition before a format or a delete.
        /// GEN3-GAP(mounts): VfsManager guards format by Partition reference only, so do it by location here.
        /// Refuses the system volume and the volume holding the current directory.
        /// </summary>
        private static bool ReleasePartition(Partition target, out string error)
        {
            error = null;

            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
            List<string> mountPoints = new List<string>();

            for (int i = 0; i < mounts.Count; i++)
            {
                Partition p = mounts[i].Partition;
                if (p == null || !ReferenceEquals(p.Host, target.Host) || p.StartSector != target.StartSector)
                {
                    continue;
                }

                string volume = AuraPath.AsDirectory(mounts[i].MountPoint);

                // Only an installed system is protected: in live mode AuraPaths.SystemVolume falls back to
                // "/0/" without any System/settings.ini, and that disk must stay formattable before setup.
                if (AuraPaths.SystemVolume != null && volume == AuraPaths.SystemVolume
                    && File.Exists(volume + "System/settings.ini"))
                {
                    error = "This partition holds the system volume (" + volume + ").";
                    return false;
                }

                if (volume == Kernel.CurrentVolume
                    || AuraPath.AsDirectory(Kernel.CurrentDirectory).StartsWith(volume, StringComparison.Ordinal))
                {
                    error = "This partition holds the current directory (" + volume + "), leave it first (cd / or vol /cv).";
                    return false;
                }

                mountPoints.Add(mounts[i].MountPoint);
            }

            for (int i = 0; i < mountPoints.Count; i++)
            {
                try
                {
                    VfsManager.TryUnmount(mountPoints[i]);
                }
                catch (Exception ex)
                {
                    // The mount is already removed from the table; only its last writes may be lost.
                    Console.WriteLine("Warning: " + mountPoints[i] + " unmounted, but its last writes may be lost: " + ex.Message);
                }
            }

            if (mountPoints.Count > 0)
            {
                Volumes.RefreshSystemVolume();
            }

            return true;
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- vol /l                                  List volumes");
            Console.WriteLine("- vol /cv {volume}                        Change current volume (e.g. vol /cv 1)");
            Console.WriteLine("- vol /lp                                 List partitions");
            Console.WriteLine("- vol /fp {disknumber} {partitionnumber}  Format partition to FAT32");
            Console.WriteLine("- vol /mp {disknumber} {size}             Make partition (size in MB)");
            Console.WriteLine("- vol /dp {disknumber} {partitionnumber}  Delete partition");
        }
    }
}