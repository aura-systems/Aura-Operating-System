/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Volumes: FAT and ext2 driver registration, /N mounts, hot-plug, free space
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

// File-scope usings resolve from the global namespace. Inside `namespace Aura_OS.System.*` the bare
// identifier `System` binds to `Aura_OS.System`, so never write `System.IO.X` in the body.
using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.FileSystem.Ext2;
using Cosmos.Kernel.System.FileSystem.Fat;
using Cosmos.Kernel.System.Storage;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// Mounts every FAT partition, and every ext2 one Cosmos's driver writes right (Ext2Volume.CanMount),
    /// at /0, /1, ... (gen2 "0:\", "1:\"). Nothing is mounted at "/": the root lists the mount points.
    /// </summary>
    public static class Volumes
    {
        public const string FatDriver = "fat";
        public const string Ext2Driver = "ext2";

        // static readonly, NOT const: a const of an enum type from another assembly makes
        // Mono.Cecil resolve that assembly while the patcher rewrites this one.
        private static readonly MountFlags s_flags = MountFlags.None;

        // Last StorageManager.Partitions snapshot seen by MountAll (replaced whole on every change).
        private static IReadOnlyList<Partition> s_lastPartitions;

        /// <summary>
        /// Registers the FAT and ext2 drivers and mounts every volume. Picks AuraPaths.SystemVolume.
        /// </summary>
        public static void Initialize()
        {
            if (!KernelFeatures.Fat)
            {
                Log.WriteString("[Aura] FAT support compiled out\n");
            }
            else if (!VfsManager.RegisterFileSystem(FatDriver, new FatFileSystemType()))
            {
                Log.WriteString("[Aura] FAT driver already registered or invalid\n");
            }

            if (!VfsManager.RegisterFileSystem(Ext2Driver, new Ext2FileSystemType()))
            {
                Log.WriteString("[Aura] ext2 driver already registered or invalid\n");
            }

            MountAll();
        }

        /// <summary>
        /// Mounts every FAT or ext2 partition not mounted yet at the next free /N (gen2 numbering).
        /// Returns the number of new mounts. Recomputes AuraPaths.SystemVolume.
        /// </summary>
        public static int MountAll()
        {
            int mounted = 0;

            IReadOnlyList<Partition> partitions = StorageManager.Partitions; // snapshot; USB can change it
            s_lastPartitions = partitions;

            for (int i = 0; i < partitions.Count; i++)
            {
                Partition partition = partitions[i];
                if (partition == null || IsMounted(partition))
                {
                    continue;
                }

                // TryMount returns false when the partition holds no such volume, but the probe's ReadBlock
                // can THROW on a device error (e.g. an NVMe timeout), so guard it.
                try
                {
                    if (TryMount(partition, NextFreeMountPoint()))
                    {
                        mounted++;
                    }
                }
                catch (Exception)
                {
                    Log.WriteString("[Aura] cannot mount partition " + partition.Name + "\n");
                }
            }

            RefreshSystemVolume();

            return mounted;
        }

        /// <summary>
        /// Mounts one FAT or ext2 partition at the next free /N: its mount point ("/1/"), null when it holds
        /// no volume Aura mounts. Its mount point when it is mounted already.
        /// </summary>
        public static string Mount(Partition partition)
        {
            VfsMount mount = MountOfPartition(partition);

            if (mount != null)
            {
                return AuraPath.AsDirectory(mount.MountPoint);
            }

            string mountPoint = NextFreeMountPoint();

            if (!TryMount(partition, mountPoint))
            {
                return null;
            }

            RefreshSystemVolume();
            return AuraPath.AsDirectory(mountPoint);
        }

        /// <summary>
        /// Mounts the partition's FAT volume at that mount point, else its ext2 one when the driver writes
        /// it right. False for neither.
        /// </summary>
        private static bool TryMount(Partition partition, string mountPoint)
        {
            string driver = null;
            string reason;

            if (KernelFeatures.Fat && VfsManager.TryMount(FatDriver, partition, s_flags, mountPoint, out _))
            {
                driver = "FAT";
            }
            else if (Ext2Volume.CanMount(partition, out reason) && VfsManager.TryMount(Ext2Driver, partition, s_flags, mountPoint, out _))
            {
                driver = "ext2";
            }

            if (driver != null)
            {
                Log.WriteString("[Aura] " + driver + " volume " + partition.Name + " mounted at " + mountPoint + "\n");
            }

            return driver != null;
        }

        /// <summary>
        /// The mount of a partition, null when it is not mounted. By location, as IsMounted.
        /// </summary>
        public static VfsMount MountOfPartition(Partition partition)
        {
            if (partition == null)
            {
                return null;
            }

            IReadOnlyList<VfsMount> mounts = VfsManager.Mounts;
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
        /// By location (Host + StartSector), never by reference: RescanPartitions creates new Partition objects.
        /// </summary>
        public static bool IsMounted(Partition partition)
        {
            return MountOfPartition(partition) != null;
        }

        /// <summary>
        /// Longest-prefix mount covering an absolute path ("/0/Users/" -> the /0 mount), null if none
        /// (live mode, or "/").
        /// </summary>
        public static VfsMount MountOf(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            VfsMount best = null;
            IReadOnlyList<VfsMount> mounts = VfsManager.Mounts;
            for (int i = 0; i < mounts.Count; i++)
            {
                VfsMount m = mounts[i];
                string mp = m.MountPoint;
                bool covers = mp == "/" || (fullPath.StartsWith(mp, StringComparison.Ordinal)
                    && (fullPath.Length == mp.Length || fullPath[mp.Length] == '/'));
                if (covers && (best == null || mp.Length > best.MountPoint.Length))
                {
                    best = m;
                }
            }

            return best;
        }

        /// <summary>
        /// Free and total bytes of the volume mounted at mountPoint ("/0" or "/0/").
        /// GEN3-GAP(driveinfo): TryStatFs sweeps the whole FAT on every call (ext2 keeps its count), callers
        /// must cache the result.
        /// </summary>
        public static bool TryGetSpace(string mountPoint, out ulong freeBytes, out ulong totalBytes)
        {
            freeBytes = 0;
            totalBytes = 0;

            if (string.IsNullOrEmpty(mountPoint))
            {
                return false;
            }

            try
            {
                VfsStatFs stats;
                if (!VfsManager.TryStatFs(mountPoint, out stats))
                {
                    return false;
                }

                freeBytes = stats.AvailableBlocks * stats.BlockSize;
                totalBytes = stats.Blocks * stats.BlockSize;
                return true;
            }
            catch (Exception)
            {
                freeBytes = 0;
                totalBytes = 0;
                return false;
            }
        }

        /// <summary>
        /// Flushes and detaches every mount (AuraPower calls it before reboot/shutdown).
        /// </summary>
        public static void UnmountAll()
        {
            IReadOnlyList<VfsMount> mounts = VfsManager.Mounts; // snapshot, safe to mutate
            for (int i = 0; i < mounts.Count; i++)
            {
                try
                {
                    VfsManager.TryUnmount(mounts[i].MountPoint);
                }
                catch (Exception)
                {
                    // The flush failed; the mount is already removed from the table.
                }
            }

            RefreshSystemVolume();
        }

        /// <summary>
        /// Call from the UI loop (about once per second). GEN3-GAP(mounts): there is no hot-plug event.
        /// When the StorageManager.Partitions snapshot changed, mounts the new partitions (MountAll).
        /// Resets Kernel.CurrentVolume/CurrentDirectory when their mount vanished (a pulled USB stick is
        /// detached by the VFS). Returns true when something changed. Never throws.
        /// </summary>
        public static bool PollHotplug()
        {
            bool changed = false;

            try
            {
                if (!ReferenceEquals(StorageManager.Partitions, s_lastPartitions))
                {
                    MountAll();
                    changed = true;
                }

                if (LeaveVanishedVolume())
                {
                    changed = true;
                }
            }
            catch (Exception)
            {
                // Never let the UI loop crash on a storage change.
            }

            return changed;
        }

        /// <summary>
        /// Moves Kernel.CurrentVolume and Kernel.CurrentDirectory to the system volume (or "/") when their
        /// mount is gone: a pulled USB stick, an unmounted or formatted volume. True when they moved.
        /// </summary>
        public static bool LeaveVanishedVolume()
        {
            string volume = Kernel.CurrentVolume;
            if (volume != null && volume != "/" && MountOf(volume) == null)
            {
                RefreshSystemVolume();
                Kernel.CurrentVolume = AuraPaths.SystemVolume ?? "/";
                Kernel.CurrentDirectory = Kernel.CurrentVolume;
                return true;
            }

            string directory = Kernel.CurrentDirectory;
            if (directory != null && directory != "/" && MountOf(directory) == null)
            {
                Kernel.CurrentDirectory = Kernel.CurrentVolume ?? "/";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Recomputes AuraPaths.SystemVolume: the lowest /N holding System/settings.ini, else "/0/" if
        /// mounted, else null (live mode).
        /// </summary>
        public static void RefreshSystemVolume()
        {
            string system = null;
            int systemNumber = int.MaxValue;
            bool zeroMounted = false;

            IReadOnlyList<VfsMount> mounts = VfsManager.Mounts;
            for (int i = 0; i < mounts.Count; i++)
            {
                string volume = AuraPath.AsDirectory(mounts[i].MountPoint);
                int number = MountNumber(volume);

                if (volume == "/0/")
                {
                    zeroMounted = true;
                }

                if (number >= systemNumber)
                {
                    continue;
                }

                try
                {
                    if (File.Exists(volume + "System/settings.ini"))
                    {
                        system = volume;
                        systemNumber = number;
                    }
                }
                catch (Exception)
                {
                    // Unreadable volume: not the system volume.
                }
            }

            if (system == null && zeroMounted)
            {
                system = "/0/";
            }

            AuraPaths.SystemVolume = system;
        }

        private static string NextFreeMountPoint()
        {
            for (int n = 0; ; n++)
            {
                string mountPoint = "/" + n.ToString();
                if (!VfsManager.TryGetMount(mountPoint, out _)) // TryMount itself does not check
                {
                    return mountPoint;
                }
            }
        }

        /// <summary>
        /// N of "/N/", int.MaxValue - 1 for any other mount point.
        /// </summary>
        private static int MountNumber(string volume)
        {
            int number = 0;
            int digits = 0;

            for (int i = 1; i < volume.Length - 1; i++)
            {
                char c = volume[i];
                if (c < '0' || c > '9' || digits >= 9)
                {
                    return int.MaxValue - 1;
                }

                number = number * 10 + (c - '0');
                digits++;
            }

            return digits == 0 ? int.MaxValue - 1 : number;
        }
    }
}
