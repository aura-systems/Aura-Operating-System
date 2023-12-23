/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rm
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandRm : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRm(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to remove a file or directory";
        }

        /// <summary>
        /// CommandRm
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string path = arguments[0];
            string fullPath = Kernel.CurrentDirectory + path;

            if (System.Filesystem.Entries.ForceRemove(fullPath))
            {
                return new ReturnInfo(this, ReturnCode.OK);
            }

            return new ReturnInfo(this, ReturnCode.ERROR);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - rm {file/directory}");
        }
    }
}
