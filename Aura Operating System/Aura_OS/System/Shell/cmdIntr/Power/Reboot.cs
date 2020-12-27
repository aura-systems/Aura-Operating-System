/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Reboot
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Power
{
    class CommandReboot : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandReboot(string[] commandvalues) : base(commandvalues)
        {
            Description = "to do a CPU reboot";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            Kernel.running = false;
            Console.Clear();
            L.Text.Display("restart");
            Sys.Power.Reboot();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
