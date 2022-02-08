/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Version
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Interpreter;
using System;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class CommandVersion : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVersion(string[] commandvalues) : base(commandvalues)
        {
            Description = "to display system version";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            Kernel.console.Foreground = ConsoleColor.White;
            Kernel.console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]");
            Kernel.console.WriteLine("Created by Alexy DA CRUZ and Valentin CHARBONNIER.");

            Kernel.console.WriteLine();

            Kernel.console.Foreground = ConsoleColor.Green;
            Kernel.console.WriteLine("Website: github.com/aura-systems");
            Kernel.console.Foreground = ConsoleColor.White;

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}