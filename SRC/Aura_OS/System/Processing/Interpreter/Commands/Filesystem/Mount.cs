/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - mount/umount
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandMount : ICommand
    {
        public CommandMount(string[] commandvalues) : base(commandvalues, CommandType.Disk)
        {
            Description = "to list mounts or mount a partition";
        }

        public override ReturnInfo Execute()
        {
            // Like Unix mount(8): no arguments lists the mount table.
            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;

            if (mounts.Count == 0)
            {
                Console.WriteLine("No filesystem mounted.");
                return new ReturnInfo(this, ReturnCode.OK);
            }

            foreach (VfsManager.VfsMount mount in mounts)
            {
                string source = mount.Source;

                if (int.TryParse(mount.Source, out int globalIndex) && globalIndex >= 0 && globalIndex < StorageManager.Partitions.Count)
                {
                    source = StorageManager.Partitions[globalIndex].Name;
                }

                Console.WriteLine(source + " on " + mount.MountPoint + " type " + mount.Name);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 3)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            if (!int.TryParse(arguments[0], out int diskNumber) || !int.TryParse(arguments[1], out int partitionNumber))
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string mountPoint = arguments[2];
            if (!mountPoint.StartsWith("/"))
            {
                mountPoint = "/" + mountPoint;
            }
            mountPoint = mountPoint.TrimEnd('/');

            if (mountPoint.Length == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Cannot mount on /.");
            }

            if (VfsManager.TryGetMount(mountPoint, out _))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, mountPoint + " is already a mount point.");
            }

            if (!DiskHelpers.TryResolvePartition(diskNumber, partitionNumber, out int globalIndex, out Partition partition))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to find partition #" + partitionNumber + " on disk #" + diskNumber + ".");
            }

            if (DiskHelpers.IsMounted(globalIndex, out string existing))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Partition is already mounted at " + existing + ".");
            }

            // gen3 only ships a FAT driver; try it directly.
            if (!VfsManager.TryMount("fat", globalIndex.ToString(), MountFlags.None, mountPoint, out _))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Mount failed — no FAT filesystem on " + partition.Name + "? Format it with: mkfs " + diskNumber + " " + partitionNumber);
            }

            Console.WriteLine(partition.Name + " mounted on " + mountPoint + ".");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- mount                                          List mounted filesystems");
            Console.WriteLine("- mount {disknumber} {partitionnumber} {path}    Mount a FAT partition (e.g. mount 0 1 /mnt1)");
        }
    }

    class CommandUmount : ICommand
    {
        public CommandUmount(string[] commandvalues) : base(commandvalues, CommandType.Disk)
        {
            Description = "to unmount a filesystem";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            string mountPoint = arguments[0];
            if (!mountPoint.StartsWith("/"))
            {
                mountPoint = "/" + mountPoint;
            }
            mountPoint = mountPoint.TrimEnd('/');

            if (!VfsManager.TryGetMount(mountPoint, out _))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, mountPoint + " is not a mount point.");
            }

            if (!VfsManager.TryUnmount(mountPoint))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Failed to unmount " + mountPoint + " (files still open?).");
            }

            // Don't leave the shell standing in a directory that no longer exists.
            if (Kernel.CurrentDirectory.StartsWith(mountPoint + "/") || Kernel.CurrentDirectory == mountPoint)
            {
                IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
                string fallback = mounts.Count > 0 ? mounts[0].MountPoint + "/" : Kernel.RootVolume + "/";
                Kernel.CurrentVolume = fallback;
                Kernel.CurrentDirectory = fallback;
            }

            Console.WriteLine(mountPoint + " unmounted.");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- umount {path}    Unmount the filesystem mounted at {path}");
        }
    }
}
