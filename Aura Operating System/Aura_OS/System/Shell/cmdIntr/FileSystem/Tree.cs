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
                DoTree(Kernel.current_directory, 0);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                DoTree(Kernel.current_directory + "/" + arguments[0], 0);
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

            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (string file in Directory.GetFiles(directory))
            {
                for (int i = 0; i < depth; i++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine(file);
            }
            Console.ForegroundColor = ConsoleColor.White;

            for (int j = 0; j < directories.Length; j++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                for (int i = 0; i < depth; i++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine(directories[j]);
                DoTree(directory + "/" + directories[j], depth + 4);
            }
        }
    }
}
