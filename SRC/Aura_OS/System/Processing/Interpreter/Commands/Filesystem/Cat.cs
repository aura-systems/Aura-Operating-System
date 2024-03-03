/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
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
                    StringBuilder sb = new();

                    foreach (string line in File.ReadAllLines(Kernel.CurrentDirectory + file))
                    {
                        sb.AppendLine(line);
                    }

                    Console.WriteLine(sb.ToString());
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
            Console.WriteLine("Usage:");
            Console.WriteLine(" - cat {file_path}");
        }
    }
}