/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;
namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandTree : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandTree(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a directory tree";
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute()
        {
            try
            {
                DoTree(Kernel.current_directory, 4);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        private void DoTree(string directory, int depth)
        {
            var directories = Directory.GetDirectories(directory);
            string dir;

            for (int j = 0; j < directories.Length; j++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                for (int i = 0; i < depth; i++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine(directories[j]);
                Console.ForegroundColor = ConsoleColor.Blue;

                if (depth == 4)
                {
                    dir = directory;
                }
                else
                {
                    dir = directory + "/" + directories[j];
                }

                foreach (string file in Directory.GetFiles(dir))
                {
                    for (int i = 0; i < depth + 4; i++)
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine(file);
                }
                Console.ForegroundColor = ConsoleColor.White;
                DoTree(directory + "/" + directories[j], depth + 4);
            }
        }
    }
}
