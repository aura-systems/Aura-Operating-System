/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File interface
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter.Commands;
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

        public static bool ForceCopy(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, overwrite: true);
                return true;
            }
            else if (Directory.Exists(sourcePath))
            {
                Entries.CopyDirectory(sourcePath, destPath);
                return true;
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
                return false;
            }
        }

        public static void SaveFile(string sourcePath, byte[] file)
        {
            File.WriteAllBytes(sourcePath, file);
        }

        public static void CopyFile(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, overwrite: true);
            }
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, dest);
            }
        }
    }
}
