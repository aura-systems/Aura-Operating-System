/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File interface
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

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

                var root = Kernel.VirtualFileSystem.GetDirectory(Kernel.CurrentDirectory);

                if (Kernel.CurrentDirectory != Kernel.CurrentVolume)
                {
                    Kernel.CurrentDirectory = root.mParent.mFullPath;
                }
            }
            if (dir == "~")
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
                    Kernel.CurrentDirectory = Kernel.CurrentDirectory + dir + @"\";
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
