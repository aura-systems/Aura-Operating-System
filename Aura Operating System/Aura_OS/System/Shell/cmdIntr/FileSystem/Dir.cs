/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Dir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
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
            DirectoryListing.DispDirectories(Kernel.current_directory);
            DirectoryListing.DispFiles(Kernel.current_directory);
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

                if (Directory.Exists(Kernel.current_directory + directory))
                {
                    DirectoryListing.DispDirectories(Kernel.current_directory + directory);
                    DirectoryListing.DispFiles(Kernel.current_directory + directory);
                }
            }

            else
            {
                if (arguments[0].Equals("-a"))
                {
                    DirectoryListing.DispDirectories(Kernel.current_directory);
                    DirectoryListing.DispHiddenFiles(Kernel.current_directory);

                    if (arguments.Count == 2)
                    {
                        directory = arguments[1];

                        DirectoryListing.DispDirectories(Kernel.current_directory + directory);
                        DirectoryListing.DispHiddenFiles(Kernel.current_directory + directory);
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
    }
}
