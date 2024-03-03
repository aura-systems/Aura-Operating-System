/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CD
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandCD : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCD(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to change current directory";
        }

        /// <summary>
        /// CommandCd
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string dir = arguments[0];

            try
            {
                string error;
                if (System.Filesystem.CurrentPath.Set(dir, out error))
                {
                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, error);
                }
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
            Console.WriteLine("Available commands:");
            Console.WriteLine("- cd {directory}    change current directory");
            Console.WriteLine("- cd ..             go to last directory");
        }
    }
}