/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class CommandLsprocess : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandLsprocess(string[] commandvalues) : base(commandvalues)
        {
            Description = "to list all registered processes";
        }

        /// <summary>
        /// CommandHelp
        /// </summary>
        public override ReturnInfo Execute()
        {
            Kernel.console.WriteLine("ID      TYPE    STATE    NAME");

            for (int i = 0; i < Kernel.ProcessManager.Processes.Count; i++)
            {
                Kernel.console.Write(Kernel.ProcessManager.Processes[i].ID.ToString().PadRight(8, ' '));
                Kernel.console.Write(((int)Kernel.ProcessManager.Processes[i].Type).ToString().PadRight(8, ' '));
                Kernel.console.Write((Kernel.ProcessManager.Processes[i].Running ? 1 : 0).ToString().PadRight(9, ' '));
                Kernel.console.Write(Kernel.ProcessManager.Processes[i].Name.ToString().PadRight(24, ' '));

                Kernel.console.WriteLine();
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Available command:");
            Kernel.console.WriteLine("- help /alias    show command aliases.");
        }
    }
}
