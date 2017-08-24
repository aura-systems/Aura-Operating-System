﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            foreach (string dir in Directory.GetDirectories(directory))
            {
                Color.DisplayTextColor("6");
                Console.Write(dir + "\t");
            }
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
                else
                {
                    Color.DisplayTextColor("1");
                    Console.Write(file + "\t");
                }

            }
        }

    }
}