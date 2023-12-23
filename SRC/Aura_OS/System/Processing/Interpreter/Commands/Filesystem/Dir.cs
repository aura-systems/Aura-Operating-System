/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Dir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandDir : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandDir(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to list directories and files";
        }

        /// <summary>
        /// CommandClear
        /// </summary>
        public override ReturnInfo Execute()
        {
            DirectoryListing.DispDirectories(Kernel.CurrentDirectory);
            DirectoryListing.DispFiles(Kernel.CurrentDirectory);
            Console.WriteLine();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string directory;

            if (!arguments[0].StartsWith("-"))
            {
                directory = arguments[0];

                if (Directory.Exists(Kernel.CurrentDirectory + directory))
                {
                    DirectoryListing.DispDirectories(Kernel.CurrentDirectory + directory);
                    DirectoryListing.DispFiles(Kernel.CurrentDirectory + directory);
                }
            }

            else
            {
                if (arguments[0].Equals("-a"))
                {
                    DirectoryListing.DispDirectories(Kernel.CurrentDirectory);
                    DirectoryListing.DispHiddenFiles(Kernel.CurrentDirectory);

                    if (arguments.Count == 2)
                    {
                        directory = arguments[1];

                        DirectoryListing.DispDirectories(Kernel.CurrentDirectory + directory);
                        DirectoryListing.DispHiddenFiles(Kernel.CurrentDirectory + directory);
                    }
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
            }

            Console.WriteLine();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - dir {directory}");
        }

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
}