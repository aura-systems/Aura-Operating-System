/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Filesystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandMkdir : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandMkdir(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to create a directory";
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute()
        {
            PrintHelp();

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string dir = arguments[0].TrimEnd('/', '\\');

            // GEN3-GAP(fat-names): the FAT driver writes any character into a long name, reject
            // the reserved ones here. (The gen2 "no dot" rule is gone: gen3 FAT writes LFN entries.)
            if (!IsValidPath(AuraPath.FromLegacy(dir)))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Invalid directory name.");
            }

            try
            {
                string path = AuraPath.Resolve(dir).TrimEnd('/');

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    //if the directory already exists, create "name(2)", "name(3)", ...
                    int num = 1;
                    string newPath;

                    do
                    {
                        num = num + 1;
                        newPath = path + "(" + num + ")";
                    }
                    while (Directory.Exists(newPath));

                    //create directory
                    Directory.CreateDirectory(newPath);

                    //display text to inform the user that the directory has been created with an another name
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("That folder existed already, directory \"" + dir + "(" + num + ")" + "\" has been created.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Every component typed by the user is a valid name, and the last one names a new directory
        /// (not "", "." or "..").
        /// </summary>
        private static bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string last = string.Empty;

            foreach (string part in path.Split('/'))
            {
                if (part.Length == 0)
                {
                    continue;
                }

                if (!AuraPath.IsValidName(part))
                {
                    return false;
                }

                last = part;
            }

            return last.Length > 0 && last != "." && last != "..";
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - mkdir {directory}");
        }
    }
}