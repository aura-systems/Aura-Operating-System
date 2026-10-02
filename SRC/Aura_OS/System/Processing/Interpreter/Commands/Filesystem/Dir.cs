/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Dir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Filesystem;
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
            try
            {
                DirectoryListing.DispDirectories(Kernel.CurrentDirectory);
                DirectoryListing.DispFiles(Kernel.CurrentDirectory);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
            Console.WriteLine();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return Execute();
            }

            string directory;

            try
            {
                if (!arguments[0].StartsWith("-"))
                {
                    directory = AuraPath.Resolve(arguments[0]);

                    if (!Directory.Exists(directory))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "This directory doesn't exist!");
                    }

                    DirectoryListing.DispDirectories(directory);
                    DirectoryListing.DispFiles(directory);
                }

                else
                {
                    if (arguments[0].Equals("-a"))
                    {
                        directory = arguments.Count >= 2 ? AuraPath.Resolve(arguments[1]) : Kernel.CurrentDirectory;

                        if (!Directory.Exists(directory))
                        {
                            return new ReturnInfo(this, ReturnCode.ERROR, "This directory doesn't exist!");
                        }

                        DirectoryListing.DispDirectories(directory);
                        DirectoryListing.DispHiddenFiles(directory);
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
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
                // gen3 returns full paths ("/0/Users"); listing "/" gives the volumes ("0", "1").
                foreach (string entry in Directory.GetDirectories(directory))
                {
                    string dir = Path.GetFileName(entry);

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
                foreach (string entry in Directory.GetFiles(directory))
                {
                    string file = Path.GetFileName(entry);
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
                foreach (string entry in Directory.GetFiles(directory))
                {
                    string file = Path.GetFileName(entry);
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