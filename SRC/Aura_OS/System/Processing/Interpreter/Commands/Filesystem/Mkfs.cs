/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - mkfs (format a partition)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandMkfs : ICommand
    {
        public CommandMkfs(string[] commandvalues) : base(commandvalues, CommandType.Disk)
        {
            Description = "to format a partition to FAT32";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            if (!int.TryParse(arguments[0], out int diskNumber) || !int.TryParse(arguments[1], out int partitionNumber))
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            if (!DiskHelpers.TryResolvePartition(diskNumber, partitionNumber, out int globalIndex, out Partition partition))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find partition #" + partitionNumber + " on disk #" + diskNumber + ".");
            }

            if (DiskHelpers.IsMounted(globalIndex, out string mountPoint))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition is mounted at " + mountPoint + ". umount it (and reboot) before reformatting.");
            }

            if (!VfsManager.TryFormat("fat", globalIndex.ToString(), new FatFormatOptions { Type = FatType.Fat32 }))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Format failed (partition may be too small for FAT32).");
            }

            Console.WriteLine(partition.Name + " formatted to FAT32. Mount it with: mount " + diskNumber + " " + partitionNumber + " /mnt1");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- mkfs {disknumber} {partitionnumber}    Format the partition to FAT32");
        }
    }
}
