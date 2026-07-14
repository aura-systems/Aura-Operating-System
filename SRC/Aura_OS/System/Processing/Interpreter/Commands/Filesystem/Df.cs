/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - df (mounted filesystems and free space)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandDf : ICommand
    {
        public CommandDf(string[] commandvalues) : base(commandvalues, CommandType.Disk)
        {
            Description = "to list mounted filesystems and their free space";
        }

        public override ReturnInfo Execute()
        {
            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;

            if (mounts.Count == 0)
            {
                Console.WriteLine("No filesystem mounted. Use lsblk to inspect disks, mkfs to format, mount to mount.");
                return new ReturnInfo(this, ReturnCode.OK);
            }

            Console.WriteLine();
            Console.WriteLine("Source        Type    Size      Used      Avail     Mounted on");
            Console.WriteLine("------        ----    ----      ----      -----     ----------");

            foreach (VfsManager.VfsMount mount in mounts)
            {
                string type = mount.Name;
                string size = "?";
                string used = "?";
                string avail = "?";
                string source = mount.Source;

                if (int.TryParse(mount.Source, out int globalIndex) && globalIndex >= 0 && globalIndex < StorageManager.Partitions.Count)
                {
                    Partition partition = StorageManager.Partitions[globalIndex];
                    type = DiskHelpers.DetectFilesystem(partition);
                    source = partition.Name;
                }

                if (mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stats))
                {
                    ulong totalBytes = stats.Blocks * stats.BlockSize;
                    ulong freeBytes = stats.Bfree * stats.BlockSize;
                    size = DiskHelpers.FormatMiB(totalBytes);
                    avail = DiskHelpers.FormatMiB(freeBytes);
                    used = DiskHelpers.FormatMiB(totalBytes - freeBytes);
                }

                string marker = Kernel.CurrentVolume.TrimEnd('/') == mount.MountPoint ? " <- current" : "";

                Console.WriteLine(Pad(source, 14) + Pad(type, 8) + Pad(size, 10) + Pad(used, 10) + Pad(avail, 10) + mount.MountPoint + marker);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            return Execute();
        }

        private static string Pad(string value, int width)
        {
            if (value.Length >= width)
            {
                return value + " ";
            }

            return value + new string(' ', width - value.Length);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- df    list mounted filesystems with size, used and available space");
        }
    }
}
