/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Interpreter;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandVol : ICommand
    {
        // gen2 numbering: the N-th FAT volume is mounted at /N (gen2 "N:\").
        // The partitions are changed by Disks, as in the Disk Manager app.

        private const ulong BytesPerMiB = Disks.BytesPerMiB;

        private const string FormatLabel = "AURAOS";

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

            Disks.DeletePartition(disk, partitions[index - 1]);

            Console.WriteLine("Partition #" + index + " deleted on disk #" + disknumber + "!");

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

            PartitionTable table = Disks.TableOf(disk);

            if (table == PartitionTable.Whole)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Disk #" + disknumber + " holds a whole-disk FAT volume (no partition table), refusing to create a partition.");
            }

            if (table == PartitionTable.None)
            {
                Disks.CreateTable(disk, false);
                table = PartitionTable.Mbr;
                Console.WriteLine("No partition table on disk #" + disknumber + ", MBR created.");
            }

            ulong first, end;
            Disks.UsableRange(disk, table, out first, out end);

            // After the last partition, on a 1 MiB boundary.
            ulong alignment = BytesPerMiB / disk.BlockSize;
            ulong start = first;

            IReadOnlyList<Partition> partitions = StorageManager.GetPartitions(disk);
            for (int i = 0; i < partitions.Count; i++)
            {
                ulong partitionEnd = partitions[i].StartSector + partitions[i].BlockCount;
                if (partitionEnd > start)
                {
                    start = partitionEnd;
                }
            }

            start = (start + alignment - 1) / alignment * alignment;

            ulong sectors = (ulong)size * alignment;

            if (start >= end || sectors > end - start)
            {
                ulong free = start < end ? (end - start) * disk.BlockSize / BytesPerMiB : 0;
                return new ReturnInfo(this, ReturnCode.ERROR, "Not enough free space on disk #" + disknumber + " (" + free + " MB available).");
            }

            Disks.CreatePartition(disk, start, sectors, Disks.Unformatted, null);

            // A fresh partition holds no filesystem: use vol /fp to format it.
            Console.WriteLine("Partition created on disk #" + disknumber + "!");

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

            // FAT32 first (gen2 behaviour); a partition too small for FAT32 gets FAT16 or FAT12.
            Disks.Format(disk, target, Disks.Fat, FormatLabel);

            string label;
            Console.WriteLine("Partition #" + partition + " formatted to " + Disks.DetectFilesystem(target, out label) + " on disk #" + driveName + "!");

            VfsManager.VfsMount mount = Volumes.MountOfPartition(target);
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
                string format = vol.Partition != null ? Disks.DetectFilesystem(vol.Partition, out label) : vol.Name;
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
            Console.WriteLine("Disk #: " + disknumber + " (" + DescribePartitionTable(disk) + ")");
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
                string format = Disks.DetectFilesystem(part, out label);
                VfsManager.VfsMount mount = Volumes.MountOfPartition(part);

                Console.WriteLine("  Partition #" + (i + 1) + ": " + part.Name
                    + "  Start=" + part.StartSector
                    + "  Sectors=" + part.BlockCount
                    + "  " + (part.BlockCount * part.BlockSize / BytesPerMiB) + " MB"
                    + "  " + format
                    + (label.Length > 0 ? " \"" + label + "\"" : "")
                    + (mount != null ? "  mounted at " + AuraPath.AsDirectory(mount.MountPoint) : ""));
            }
        }

        private static string DescribePartitionTable(IBlockDevice disk)
        {
            try
            {
                switch (Disks.TableOf(disk))
                {
                    case PartitionTable.Gpt:
                        return "GPT";
                    case PartitionTable.Mbr:
                        return "MBR";
                    case PartitionTable.Whole:
                        return "None (whole-disk FAT)";
                }
            }
            catch (Exception)
            {
                return "unreadable";
            }

            return "None";
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