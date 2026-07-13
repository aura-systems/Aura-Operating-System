/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Filesystem utils
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Vfs;

namespace Aura_OS.System.Filesystem
{
    public class Utils
    {
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }

            // gen2 clamped at the drive root ("X:\"); gen3 clamps at the mount point.
            if (path.Length <= Kernel.RootVolume.Length)
            {
                return Kernel.RootVolume + "/";
            }

            int lastSeparatorIndex = path.LastIndexOf('/');
            if (lastSeparatorIndex <= 0)
            {
                return path + "/";
            }

            string parent = path.Substring(0, lastSeparatorIndex);
            if (parent.Length < Kernel.RootVolume.Length)
            {
                parent = Kernel.RootVolume;
            }
            return parent + "/";
        }

        public static string GetFreeSpace()
        {
            // gen2: Kernel.VirtualFileSystem.GetAvailableFreeSpace(volume);
            // gen3: no DriveInfo plug — read the numbers from the mounted superblock (StatFs).
            if (VfsManager.TryGetMount(Kernel.CurrentVolume, out VfsManager.VfsMount mount) &&
                mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs statFs))
            {
                return ConvertSize((long)(statFs.Bfree * statFs.BlockSize));
            }

            return ConvertSize(0);
        }

        public static string GetCapacity()
        {
            // gen2: Kernel.VirtualFileSystem.GetTotalSize(volume); gen3: StatFs (see GetFreeSpace).
            if (VfsManager.TryGetMount(Kernel.CurrentVolume, out VfsManager.VfsMount mount) &&
                mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs statFs))
            {
                return ConvertSize((long)(statFs.Blocks * statFs.BlockSize));
            }

            return ConvertSize(0);
        }

        public static string ConvertSize(long bytes)
        {
            string suffix = " Bytes";
            double size = bytes;

            if (size >= 1024 * 1024 * 1024)
            {
                size /= 1024 * 1024 * 1024;
                suffix = "GB";
            }
            else if (size >= 1024 * 1024)
            {
                size /= 1024 * 1024;
                suffix = "MB";
            }
            else if (size >= 1024)
            {
                size /= 1024;
                suffix = "KB";
            }

            return $"{Round(size)}{suffix}";
        }

        private static string Round(double number)
        {
            string numStr = number.ToString();
            int dotIndex = numStr.IndexOf('.');

            if (dotIndex != -1)
            {
                return numStr.Substring(0, dotIndex);
            }
            return numStr;
        }
    }
}
