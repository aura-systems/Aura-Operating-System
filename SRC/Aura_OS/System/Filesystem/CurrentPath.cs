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
        /// <summary>
        /// Changes Kernel.CurrentDirectory (and Kernel.CurrentVolume to the volume holding it).
        /// Accepts "~", relative paths, "..", absolute paths ("/1/Docs") and gen2 input ("1:\Docs").
        /// ".." at a volume root goes to "/", the volume list.
        /// </summary>
        public static bool Set(string dir, out string error)
        {
            string target;

            if (dir == "~")
            {
                if (string.IsNullOrEmpty(Kernel.UserDirectory) || !Directory.Exists(Kernel.UserDirectory))
                {
                    error = "No user directory found.";
                    return false;
                }

                target = Kernel.UserDirectory;
            }
            else
            {
                target = AuraPath.Resolve(dir);
            }

            if (Directory.Exists(target))
            {
                // The Kernel.CurrentDirectory setter also syncs Directory.SetCurrentDirectory.
                Kernel.CurrentDirectory = AuraPath.AsDirectory(target);

                VfsManager.VfsMount mount = Volumes.MountOf(target);
                Kernel.CurrentVolume = AuraPath.AsDirectory(mount != null ? mount.MountPoint : "/");
            }
            else if (File.Exists(target))
            {
                error = "This is a file.";
                return false;
            }
            else
            {
                error = "This directory doesn't exist!";
                return false;
            }

            error = "none";
            return true;
        }
    }
}
