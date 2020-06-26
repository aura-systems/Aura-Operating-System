/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Directory and files listing system.
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Computer;

namespace Aura_OS.System.Shell
{
    class DirectoryListing
    {

        /// <summary>
        /// Display directories of "directory"
        /// </summary>
        /// <param name="directory"></param>
        public static void DispDirectories(string directory)
        {
            foreach (string dir in Directory.GetDirectories(directory))
            {
                if (!dir.StartsWith("."))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(dir + "\t");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        /// <summary>
        /// Display hidden and normal directories of "directory"
        /// </summary>
        /// <param name="directory"></param>
        public static void DispHiddenDirectories(string directory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Display files of "directory"
        /// </summary>
        /// <param name="directory"></param>
        public static void DispFiles(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                Char formatDot = '.';
                string[] ext = file.Split(formatDot);
                string lastext = ext[ext.Length - 1];

                //display file that doesn't have a dot before the name.
                if (!file.StartsWith("."))
                {
                    if (lastext == "conf")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(file + "\t");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (file.StartsWith("passwd"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(file + "\t");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(file + "\t");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        /// <summary>
        /// Display hidden and normal files of "directory"
        /// </summary>
        /// <param name="directory"></param>
        public static void DispHiddenFiles(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                Char formatDot = '.';
                string[] ext = file.Split(formatDot);
                string lastext = ext[ext.Length - 1];

                if (lastext == "conf")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(file + "\t");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (file.StartsWith("passwd"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(file + "\t");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (file.StartsWith("."))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(file + "\t");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write(file + "\t");
                }
            }
        }

    }
}
