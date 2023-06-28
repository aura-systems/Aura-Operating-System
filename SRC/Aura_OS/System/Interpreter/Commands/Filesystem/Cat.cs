/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandCat : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCat(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a text file";
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                string file = arguments[0];

                if (File.Exists(Kernel.CurrentDirectory + file))
                {
                    foreach (string line in File.ReadAllLines(Kernel.CurrentDirectory + file))
                    {
                        Kernel.console.WriteLine(line);
                    }
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
                }

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - cat {file_path}");
        }
    }
}