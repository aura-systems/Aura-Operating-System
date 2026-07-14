/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - shared disk/partition helpers
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    /// <summary>
    /// Shared plumbing for the gen3 filesystem commands (df, lsblk/fdisk, mount,
    /// umount, mkfs): partition resolution, filesystem detection and size formatting.
    /// </summary>
    internal static class DiskHelpers
    {
        public const ulong BytesPerMiB = 1024 * 1024;

        /// <summary>
        /// Resolves a (disk, per-disk partition) pair to its index in
        /// StorageManager.Partitions — the index VfsManager/PartitionManager expect.
        /// Partition numbers are 1-based on the command line.
        /// </summary>
        public static bool TryResolvePartition(int diskNumber, int partitionNumber, out int globalIndex, out Partition partition)
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

                if (local == partitionNumber - 1)
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
        /// Names the partition table written on the device.
        /// </summary>
        public static string DescribePartitionTable(IBlockDevice device)
        {
            if (Gpt.IsGpt(device))
            {
                return "GPT";
            }

            if (Mbr.IsMbr(device))
            {
                return "MBR";
            }

            return "no partition table";
        }

        /// <summary>
        /// Names the filesystem on a partition by parsing its boot sector.
        /// </summary>
        public static string DetectFilesystem(Partition partition)
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
                        return "fat12";
                    case FatType.Fat16:
                        return "fat16";
                    case FatType.Fat32:
                        return "fat32";
                    default:
                        return "fat";
                }
            }

            return "unknown";
        }

        /// <summary>
        /// True when the given global partition index is recorded as the source of an
        /// active mount. mount.Source is the index at mount time and goes stale after
        /// partition create/delete rescans, hence the "reboot before" advice.
        /// </summary>
        public static bool IsMounted(int globalIndex, out string mountPoint)
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
        /// Formats a byte count as "N MB".
        /// </summary>
        public static string FormatMiB(ulong bytes)
        {
            return (bytes / BytesPerMiB) + " MB";
        }
    }
}
