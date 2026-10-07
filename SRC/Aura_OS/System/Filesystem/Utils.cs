/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Filesystem utils
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.FileSystem;
using System;
using System.IO;

namespace Aura_OS.System.Filesystem
{
    public class Utils
    {
        /// <summary>
        /// Parent directory, always ending with '/': "/0/Users/bob/" -> "/0/Users/", "/0/" -> "/" (volume list),
        /// "/" -> "/".
        /// </summary>
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string trimmed = path.TrimEnd(AuraPath.Separator);
            if (trimmed.Length == 0)
            {
                return "/";
            }

            string parent = Path.GetDirectoryName(trimmed);
            if (parent == null)
            {
                return "/";
            }
            if (parent.Length == 0)
            {
                return trimmed + AuraPath.Separator;
            }
            return AuraPath.AsDirectory(parent);
        }

        public static string GetFreeSpace()
        {
            ulong freeBytes, totalBytes;
            TryGetCurrentVolumeSpace(out freeBytes, out totalBytes);
            return ConvertSize(freeBytes);
        }

        public static string GetCapacity()
        {
            ulong freeBytes, totalBytes;
            TryGetCurrentVolumeSpace(out freeBytes, out totalBytes);
            return ConvertSize(totalBytes);
        }

        // GEN3-GAP(driveinfo): TryStatFs sweeps the whole FAT on every call, so the result is cached
        // per mount point for a few seconds.
        private const long SpaceCacheMs = 5000;
        private static string _spaceMountPoint;
        private static long _spaceTick;
        private static ulong _spaceFree;
        private static ulong _spaceTotal;

        private static bool TryGetCurrentVolumeSpace(out ulong freeBytes, out ulong totalBytes)
        {
            freeBytes = 0;
            totalBytes = 0;

            // null in live mode (no FAT volume, Kernel.CurrentVolume == "/").
            VfsMount mount = Volumes.MountOf(Kernel.CurrentVolume);
            if (mount == null)
            {
                return false;
            }

            string mountPoint = mount.MountPoint;
            long now = Environment.TickCount64;

            if (_spaceMountPoint != null && _spaceMountPoint == mountPoint && now - _spaceTick < SpaceCacheMs)
            {
                freeBytes = _spaceFree;
                totalBytes = _spaceTotal;
                return true;
            }

            if (!Volumes.TryGetSpace(mountPoint, out freeBytes, out totalBytes))
            {
                _spaceMountPoint = null;
                return false;
            }

            _spaceMountPoint = mountPoint;
            _spaceTick = now;
            _spaceFree = freeBytes;
            _spaceTotal = totalBytes;
            return true;
        }

        public static string ConvertSize(ulong bytes)
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
