/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Echo
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;

namespace Aura_OS.System.Shell.cmdIntr.c_Console
{
    class CommandEcho : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandEcho(string[] commandvalues) : base(commandvalues)
        {
            Description = "to echo text";
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
            foreach (var argument in arguments)
            {
                if (argument.StartsWith("$"))
                {
                    try
                    {
                        Console.WriteLine(Kernel.environmentvariables[argument.Remove(0, 1)]);
                    }
                    catch
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Variable does not exist.");
                    }
                }
                else
                {
                    Console.WriteLine(argument);
                }
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
