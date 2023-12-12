/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rm
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
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

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                Kernel.console.WriteLine(path + " does not exist!");
            }

            return new ReturnInfo(this, ReturnCode.ERROR);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - rm {file/directory}");
        }
    }
}
