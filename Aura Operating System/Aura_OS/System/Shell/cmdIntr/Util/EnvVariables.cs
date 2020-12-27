/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Environment variables
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class CommandEnv : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandEnv(string[] commandvalues) : base(commandvalues)
        {
            Description = "to set environment variables";
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
			string[] exportcmd = arguments[0].Split('=');
            if (exportcmd.Length != 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR);
            }
			string var = exportcmd[0];
			string value = exportcmd[1];
            Kernel.environmentvariables.Add(var, value);
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - export {var_name} {var_value}");
        }
    }
}
