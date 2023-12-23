/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File interface
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;

namespace Aura_OS.System.Filesystem
{
    public class Entries
    {
        public static bool ForceRemove(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                return true;
            }
            else
            {
                Console.WriteLine(fullPath + " does not exist!");
                return false;
            }
        }
    }
}
