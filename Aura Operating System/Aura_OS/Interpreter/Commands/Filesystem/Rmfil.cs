/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rmfil
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandRmfil : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRmfil(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
        }

        /// <summary>
        /// CommandMkfil
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string file = arguments[0];

            if (File.Exists(Kernel.CurrentDirectory + file))
            {
                File.Delete(Kernel.CurrentDirectory + file);
            }
            else
            {
                Kernel.console.WriteLine(file + " does not exist!");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - rmfil {file}");
        }
    }
}