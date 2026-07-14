/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File interface
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Vfs;
using System.IO;

namespace Aura_OS.System.Filesystem
{
    public class CurrentPath
    {
        public static bool Set(string dir, out string error)
        {
            if (dir == "..")
            {
                Directory.SetCurrentDirectory(Kernel.CurrentDirectory);

                if (Kernel.CurrentDirectory != Kernel.CurrentVolume)
                {
                    // gen2 walked DirectoryEntry.mParent.mFullPath; gen3 has no DirectoryEntry,
                    // so compute the parent from the path itself, clamped at the mount point.
                    string parent = Path.GetDirectoryName(Kernel.CurrentDirectory.TrimEnd('/'));

                    if (parent == null || parent.Length < Kernel.CurrentVolume.TrimEnd('/').Length)
                    {
                        parent = Kernel.CurrentVolume.TrimEnd('/');
                    }

                    Kernel.CurrentDirectory = parent.TrimEnd('/') + "/";
                }
            }
            else if (dir.StartsWith("/"))
            {
                // Absolute path: may land on any mount point (gen3 has several,
                // e.g. /mnt, /mnt1), so track which volume now contains us.
                string normalized = dir.TrimEnd('/');

                VfsManager.VfsMount covering = null;
                foreach (VfsManager.VfsMount mount in VfsManager.Mounts)
                {
                    if (normalized == mount.MountPoint || normalized.StartsWith(mount.MountPoint + "/"))
                    {
                        if (covering == null || mount.MountPoint.Length > covering.MountPoint.Length)
                        {
                            covering = mount;
                        }
                    }
                }

                if (covering == null)
                {
                    error = "No mounted volume contains " + dir + " (see: mount).";
                    return false;
                }

                if (normalized != covering.MountPoint && !Directory.Exists(normalized))
                {
                    error = "This directory doesn't exist!";
                    return false;
                }

                Kernel.CurrentVolume = covering.MountPoint + "/";
                Kernel.CurrentDirectory = normalized + "/";
            }
            else if (dir == "~")
            {
                if (Directory.Exists(Kernel.UserDirectory))
                {
                    Directory.SetCurrentDirectory(Kernel.CurrentDirectory);
                    Kernel.CurrentDirectory = Kernel.UserDirectory;
                }
                else
                {
                    error = "No user directory found.";
                    return false;
                }
            }
            else if (dir == Kernel.CurrentVolume)
            {
                Kernel.CurrentDirectory = Kernel.CurrentVolume;
            }
            else
            {
                if (Directory.Exists(Kernel.CurrentDirectory + dir))
                {
                    Directory.SetCurrentDirectory(Kernel.CurrentDirectory);
                    Kernel.CurrentDirectory = Kernel.CurrentDirectory + dir + "/";
                }
                else if (File.Exists(Kernel.CurrentDirectory + dir))
                {
                    error = "This is a file.";
                    return false;
                }
                else
                {
                    error = "This directory doesn't exist!";
                    return false;
                }
            }

            error = "none";
            return true;
        }
    }
}
