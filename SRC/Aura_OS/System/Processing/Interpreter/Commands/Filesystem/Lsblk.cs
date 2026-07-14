/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - lsblk/fdisk (disks and partitions)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Storage;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandLsblk : ICommand
    {
        /// <summary>
        /// Conventional first partition LBA on an MBR disk (1 MiB alignment).
        /// </summary>
        private const ulong MbrFirstPartitionLba = 2048;

        /// <summary>
        /// MBR system ID byte (FAT32 LBA) stamped on partitions created by /mp.
        /// </summary>
        private const byte MbrFat32LbaSystemId = 0x0B;

        public CommandLsblk(string[] commandvalues) : base(commandvalues, CommandType.Disk)
        {
            Description = "to list disks and manage partitions";
        }

        public override ReturnInfo Execute()
        {
            return ListDisks();
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0].Equals("/l") || arguments[0].Equals("/list"))
            {
                return ListDisks();
            }
            else if (arguments[0].Equals("/mp") || arguments[0].Equals("/makepartition"))
            {
                if (arguments.Count < 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                return MakePartition(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }
            else if (arguments[0].Equals("/dp") || arguments[0].Equals("/deletepartition"))
            {
                if (arguments.Count < 3)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                return DeletePartition(int.Parse(arguments[1]), int.Parse(arguments[2]));
            }

            return new ReturnInfo(this, ReturnCode.ERROR_ARG);
        }

        private ReturnInfo ListDisks()
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

                ulong totalBytes = device.BlockCount * device.BlockSize;

                Console.WriteLine();
                Console.WriteLine("Disk #" + counter + ": " + device.Name + " - " + DiskHelpers.FormatMiB(totalBytes) + " (" + DiskHelpers.DescribePartitionTable(device) + ")");
                Console.WriteLine("  Sectors: " + device.BlockCount + " (" + device.BlockSize + " B each)");

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
                    string mounted = DiskHelpers.IsMounted(i, out string mountPoint) ? " mounted on " + mountPoint : "";

                    Console.WriteLine("  Partition #" + (local + 1) + ": " + partition.Name + " - " + DiskHelpers.FormatMiB(sizeBytes) + " (" + DiskHelpers.DetectFilesystem(partition) + ")" + mounted);

                    local++;
                }

                if (local == 0)
                {
                    Console.WriteLine("  No partition. Create one with: lsblk /mp " + counter + " {sizeMB}");
                }
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        private ReturnInfo MakePartition(int diskNumber, int sizeMb)
        {
            IBlockDevice device = StorageManager.GetDevice(diskNumber);

            if (device == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find disk #" + diskNumber + ".");
            }

            if (sizeMb <= 0)
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
            ulong sectorsPerMb = DiskHelpers.BytesPerMiB / device.BlockSize;
            ulong sectorCount = (ulong)sizeMb * sectorsPerMb;

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

            Console.WriteLine("Partition created on disk #" + diskNumber + ". Format it with: mkfs " + diskNumber + " {partitionnumber}");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        private ReturnInfo DeletePartition(int diskNumber, int partitionNumber)
        {
            if (!DiskHelpers.TryResolvePartition(diskNumber, partitionNumber, out int globalIndex, out Partition partition))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find partition #" + partitionNumber + " on disk #" + diskNumber + ".");
            }

            if (DiskHelpers.IsMounted(globalIndex, out string mountPoint))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition is mounted at " + mountPoint + ". umount it (and reboot) before deleting it.");
            }

            IBlockDevice host = partition.Host;

            if (!PartitionManager.Delete(host, new PartitionManager.PartitionLocation(partition.StartSector, partition.BlockCount)))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to delete partition.");
            }

            StorageManager.RescanPartitions(host);

            Console.WriteLine("Partition #" + partitionNumber + " deleted on disk #" + diskNumber + "!");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- lsblk                                     List disks and partitions");
            Console.WriteLine("- lsblk /mp {disknumber} {sizeMB}           Make partition (writes MBR on blank disks)");
            Console.WriteLine("- lsblk /dp {disknumber} {partitionnumber}  Delete partition");
            Console.WriteLine("(fdisk is an alias of lsblk)");
        }
    }
}
