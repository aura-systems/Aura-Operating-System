/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Shutdown
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Power
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
            Kernel.running = false;
            Console.Clear();
            L.Text.Display("shutdown");
            Sys.Power.Shutdown();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
