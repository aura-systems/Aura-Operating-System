﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Shutdown
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using Aura_OS;
using Aura_OS.System.Processing.Interpreter;

namespace Aura_OS.System.Processing.Interpreter.Commands.Power
{
    class CommandShutdown : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandShutdown(string[] commandvalues) : base(commandvalues)
        {
            Description = "to do an ACPI shutdown";
        }

        /// <summary>
        /// ShutdownCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            Console.WriteLine("Shutting Down...");
            Sys.Power.Shutdown();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
