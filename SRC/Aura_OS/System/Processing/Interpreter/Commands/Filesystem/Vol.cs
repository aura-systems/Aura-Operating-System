/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandVol : ICommand
    {
        /// <summary>
        /// Conventional first partition LBA on an MBR disk (1 MiB alignment).
        /// </summary>
        private const ulong MbrFirstPartitionLba = 2048;

        /// <summary>
        /// MBR system ID byte (FAT32 LBA) stamped on partitions created by /mp.
        /// </summary>
        private const byte MbrFat32LbaSystemId = 0x0B;

        private const ulong BytesPerMiB = 1024 * 1024;

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
            if (arguments[0].Equals("/l") || arguments[0].Equals("/list"))
            {
                return ListVolumes();
            }
            else if (arguments[0].Equals("/cv") || arguments[0].Equals("/changevol"))
            {
                return ChangeVolume(arguments[1]);
            }
            else if (arguments[0].Equals("/fp") || arguments[0].Equals("/formatpartiton"))
            {
                return FormatVolume(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else if (arguments[0].Equals("/lp") || arguments[0].Equals("/listpartitions"))
            {
                return ListDisk();
            }
            else if (arguments[0].Equals("/mp") || arguments[0].Equals("/makepartition"))
            {
                return MakeDisk(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else if (arguments[0].Equals("/dp") || arguments[0].Equals("/deletepartition"))
            {
                return DeleteDisk(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
        }

        public ReturnInfo DeleteDisk(int disknumber, int index)
        {
            index--;

            if (!TryResolvePartition(disknumber, index, out int globalIndex, out Partition partition))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            if (IsMounted(globalIndex, out string mountPoint))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition is mounted at " + mountPoint + ". Reboot before deleting it.");
            }

            IBlockDevice host = partition.Host;

            if (!PartitionManager.Delete(host, new PartitionManager.PartitionLocation(partition.StartSector, partition.BlockCount)))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to delete partition.");
            }

            StorageManager.RescanPartitions(host);

            Console.WriteLine("Partition #" + (index + 1) + " deleted on disk #" + disknumber + "!");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo MakeDisk(int disknumber, int size)
        {
            IBlockDevice device = StorageManager.GetDevice(disknumber);

            if (device == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            if (size <= 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition size must be greater than 0 MB.");
            }

            bool isGpt = Gpt.IsGpt(device);
            bool isMbr = !isGpt && Mbr.IsMbr(device);

            if (!isGpt && !isMbr)
            {
                // gen2 could partition a blank disk directly; write a fresh MBR first.
                Mbr.Create(device);
                StorageManager.RescanPartitions(device);
                isMbr = true;
            }

            ulong firstUsable = isGpt ? Gpt.FirstUsableLba : MbrFirstPartitionLba;
            ulong sectorsPerMb = BytesPerMiB / device.BlockSize;
            ulong sectorCount = (ulong)size * sectorsPerMb;

            // Append after the last existing partition on this disk.
            ulong startSector = firstUsable;
            IReadOnlyList<Partition> partitions = StorageManager.Partitions;
            for (int i = 0; i < partitions.Count; i++)
            {
                if (!ReferenceEquals(partitions[i].Host, device))
                {
                    continue;
                }

                ulong end = partitions[i].StartSector + partitions[i].BlockCount;
                if (end > startSector)
                {
                    startSector = end;
                }
            }

            if (startSector + sectorCount > device.BlockCount)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition does not fit on disk.");
            }

            if (!PartitionManager.Create(device, startSector, sectorCount, mbrSystemId: MbrFat32LbaSystemId, gptType: Gpt.BasicDataPartitionType))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to create partition (no free slot or bad geometry).");
            }

            StorageManager.RescanPartitions(device);

            Console.WriteLine("Partition created on disk #" + disknumber + "!");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo DiskInfo(int disknumber)
        {
            IBlockDevice device = StorageManager.GetDevice(disknumber);

            if (device == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            DisplayInformation(disknumber, device);

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListDisk()
        {
            if (StorageManager.DeviceCount == 0)
            {
                Console.WriteLine("No storage devices detected.");
                return new ReturnInfo(this, ReturnCode.OK);
            }

            for (int counter = 0; counter < StorageManager.DeviceCount; counter++)
            {
                IBlockDevice device = StorageManager.GetDevice(counter);

                if (device == null)
                {
                    continue;
                }

                string type = DescribePartitionTable(device);

                Console.WriteLine();
                Console.WriteLine("Disk #: " + counter + " (" + type + ")");

                DisplayInformation(counter, device);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo FormatVolume(int driveName, int partition)
        {
            partition--;

            if (!TryResolvePartition(driveName, partition, out int globalIndex, out Partition target))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find our drive.");
            }

            if (IsMounted(globalIndex, out string mountPoint))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition is mounted at " + mountPoint + ". Reboot before reformatting.");
            }

            if (!VfsManager.TryFormat("fat", globalIndex.ToString(), new FatFormatOptions { Type = FatType.Fat32 }))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Format failed (partition may be too small for FAT32).");
            }

            Console.WriteLine("Partition #" + (partition + 1) + " formatted to FAT32 on disk #" + driveName + "!");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ChangeVolume(string volume)
        {
            try
            {
                // gen3 volumes are Unix mount points (e.g. /mnt), not drive letters.
                if (!volume.StartsWith("/"))
                {
                    volume = "/" + volume;
                }

                bool exist = false;

                foreach (VfsManager.VfsMount mount in VfsManager.Mounts)
                {
                    if (mount.MountPoint == volume)
                    {
                        exist = true;
                        Kernel.CurrentVolume = mount.MountPoint + "/";
                        Kernel.CurrentDirectory = Kernel.CurrentVolume;
                    }
                }
                if (!exist)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "The specified drive is not found.");
                }
            }
            catch
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "The specified drive is not found.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public ReturnInfo ListVolumes()
        {
            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;

            Console.WriteLine();
            Console.WriteLine("  Volume ###\tFormat\tSize");
            Console.WriteLine("  ----------\t------\t--------");

            foreach (VfsManager.VfsMount mount in mounts)
            {
                string format = mount.Name.ToUpper();
                string size = "?";
                string source = mount.Source;

                if (int.TryParse(mount.Source, out int globalIndex) && globalIndex >= 0 && globalIndex < StorageManager.Partitions.Count)
                {
                    Partition partition = StorageManager.Partitions[globalIndex];
                    format = DetectFilesystem(partition);
                    source = partition.Name;
                }

                if (mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stats))
                {
                    size = (stats.Blocks * stats.BlockSize / BytesPerMiB).ToString();
                }

                if (Kernel.CurrentVolume.StartsWith(mount.MountPoint) && mounts.Count > 1)
                {
                    Console.WriteLine(" >" + mount.MountPoint + "\t   \t" + format + " \t" + size + " MB\t" + source);
                }
                else
                {
                    Console.WriteLine("  " + mount.MountPoint + "\t   \t" + format + " \t" + size + " MB\t" + source);
                }
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Prints the geometry and partitions of a disk.
        /// </summary>
        private void DisplayInformation(int disknumber, IBlockDevice device)
        {
            ulong totalBytes = device.BlockCount * device.BlockSize;

            Console.WriteLine("Name:      " + device.Name);
            Console.WriteLine("Sectors:   " + device.BlockCount + " (" + device.BlockSize + " B each)");
            Console.WriteLine("Capacity:  " + (totalBytes / BytesPerMiB) + " MB");

            int local = 0;
            IReadOnlyList<Partition> partitions = StorageManager.Partitions;
            for (int i = 0; i < partitions.Count; i++)
            {
                if (!ReferenceEquals(partitions[i].Host, device))
                {
                    continue;
                }

                Partition partition = partitions[i];
                ulong sizeBytes = partition.BlockCount * partition.BlockSize;

                Console.WriteLine("  Partition #" + (local + 1) + ": " + partition.Name + " - " + (sizeBytes / BytesPerMiB) + " MB (" + DetectFilesystem(partition) + ")");

                local++;
            }

            if (local == 0)
            {
                Console.WriteLine("  No partition.");
            }
        }

        /// <summary>
        /// Names the partition table written on the device.
        /// </summary>
        private static string DescribePartitionTable(IBlockDevice device)
        {
            if (Gpt.IsGpt(device))
            {
                return "GPT";
            }

            if (Mbr.IsMbr(device))
            {
                return "MBR";
            }

            return "None";
        }

        /// <summary>
        /// Names the filesystem on a partition by parsing its boot sector.
        /// </summary>
        private static string DetectFilesystem(Partition partition)
        {
            byte[] boot = new byte[partition.BlockSize];

            try
            {
                partition.ReadBlock(FatBootSector.BootSectorLba, 1, boot);
            }
            catch
            {
                return "unreadable";
            }

            if (FatBootSector.TryParse(boot, out FatBootSector bootSector) && bootSector != null)
            {
                switch (bootSector.Type)
                {
                    case FatType.Fat12:
                        return "FAT12";
                    case FatType.Fat16:
                        return "FAT16";
                    case FatType.Fat32:
                        return "FAT32";
                    default:
                        return "FAT";
                }
            }

            return "unknown";
        }

        /// <summary>
        /// Resolves a (disk, per-disk partition) pair to its index in
        /// StorageManager.Partitions — the index VfsManager/PartitionManager expect.
        /// </summary>
        private static bool TryResolvePartition(int diskNumber, int partitionNumber, out int globalIndex, out Partition partition)
        {
            globalIndex = -1;
            partition = null;

            IBlockDevice device = StorageManager.GetDevice(diskNumber);
            if (device == null)
            {
                return false;
            }

            int local = 0;
            IReadOnlyList<Partition> all = StorageManager.Partitions;
            for (int i = 0; i < all.Count; i++)
            {
                if (!ReferenceEquals(all[i].Host, device))
                {
                    continue;
                }

                if (local == partitionNumber)
                {
                    globalIndex = i;
                    partition = all[i];
                    return true;
                }

                local++;
            }

            return false;
        }

        /// <summary>
        /// True when the given global partition index is recorded as the source of an
        /// active mount. mount.Source is the index at mount time and goes stale after
        /// partition create/delete rescans, hence the "reboot before" advice.
        /// </summary>
        private static bool IsMounted(int globalIndex, out string mountPoint)
        {
            mountPoint = null;

            for (int i = 0; i < VfsManager.Mounts.Count; i++)
            {
                VfsManager.VfsMount mount = VfsManager.Mounts[i];

                if (int.TryParse(mount.Source, out int mountedIndex) && mountedIndex == globalIndex)
                {
                    mountPoint = mount.MountPoint;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- vol /l                                  List volumes");
            Console.WriteLine("- vol /cv {volume}                        Change current volume (e.g. /mnt)");
            Console.WriteLine("- vol /lp                                 List partitions");
            Console.WriteLine("- vol /fp {disknumber} {partitionnumber}  Format partition to FAT32");
            Console.WriteLine("- vol /mp {disknumber} {size}             Make MBR partition");
            Console.WriteLine("- vol /dp {disknumber} {partitionnumber}  Delete partition");
        }
    }
}
