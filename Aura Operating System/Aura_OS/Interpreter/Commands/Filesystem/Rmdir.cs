/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rmdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandRmdir : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRmdir(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to remove a directory";
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string dir = arguments[0];
            if (Directory.Exists(Kernel.CurrentDirectory + dir))
            {
                Directory.Delete(Kernel.CurrentDirectory + dir, true);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                Kernel.console.WriteLine(dir + " does not exist!");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - rmdir {dir}");
        }
    }
}