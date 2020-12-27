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
                if (File.Exists(Kernel.current_directory + file))
                {
                    foreach (string line in File.ReadAllLines(Kernel.current_directory + file))
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    L.Text.Display("doesnotexit");
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
