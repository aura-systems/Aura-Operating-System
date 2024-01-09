/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rm
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandCopy : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCopy(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to copy a file or directory";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string sourcePath = ResolvePath(arguments[0]);
            string destPath = ResolvePath(arguments[1]);

            try
            {
                if (Entries.ForceCopy(sourcePath, destPath))
                {
                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Failed to copy.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new ReturnInfo(this, ReturnCode.ERROR);
            }
        }

        private string ResolvePath(string path)
        {
            if (path.StartsWith("./"))
            {
                path = path.Substring(2);
                path = Kernel.CurrentDirectory + path;
            }

            if (path == ".")
            {
                return Kernel.CurrentDirectory;
            }
            else
            {
                return Path.Combine(Kernel.CurrentDirectory, path);
            }
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - cp {source_file/directory} {destination_file/directory}");
        }
    }
}
