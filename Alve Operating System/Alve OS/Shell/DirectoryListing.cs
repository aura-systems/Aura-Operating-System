/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Directory and files listing system.
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Alve_OS.System.Computer;

namespace Alve_OS.Shell
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
                    Color.DisplayTextColor("6");
                    Console.Write(dir + "\t");
                }
            }
        }

        /// <summary>
        /// Display hidden and normal directories of "directory"
        /// </summary>
        /// <param name="directory"></param>
        public static void DispHiddenDirectories(string directory)
        {
            //foreach (string dir in Directory.GetDirectories(directory))
            //{
            //  Color.DisplayTextColor("6");
            //  Console.Write(dir + "\t");
            //}

            throw new NotImplementedException();
            //Cosmos doesn't support directories with a dot in it. :/ (FAT problem)
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
                    if ((lastext == "set") || (lastext == "nam") || (lastext == "usr"))
                    {
                        Color.DisplayTextColor("4");
                        Console.Write(file + "\t");
                    }
                    else
                    {
                        Color.DisplayTextColor("1");
                        Console.Write(file + "\t");
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

                if ((lastext == "set") || (lastext == "nam") || (lastext == "usr"))
                {
                    Color.DisplayTextColor("4");
                    Console.Write(file + "\t");
                }
                else if (file.StartsWith("."))
                {
                    Color.DisplayTextColor("5");
                    Console.Write(file + "\t");
                }
                else
                {
                    Color.DisplayTextColor("1");
                    Console.Write(file + "\t");
                }
            }
        }

    }
}
