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
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]");
            Console.WriteLine("Created by Alexy DA CRUZ and Valentin CHARBONNIER.");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Website: github.com/aura-systems");
            Console.ForegroundColor = ConsoleColor.White;

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}