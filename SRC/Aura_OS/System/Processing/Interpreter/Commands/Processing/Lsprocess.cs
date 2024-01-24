/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lsprocess command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
{
    class CommandLsprocess : ICommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommandLsprocess(string[] commandvalues) : base(commandvalues)
        {
            Description = "to list all registered processes";
        }

        /// <summary>
        /// CommandLsprocess
        /// </summary>
        public override ReturnInfo Execute()
        {
            Console.WriteLine("ID      TYPE    STATE    NAME");

            for (int i = 0; i < Kernel.ProcessManager.Processes.Count; i++)
            {
                Console.Write(Kernel.ProcessManager.Processes[i].ID.ToString().PadRight(8, ' '));
                Console.Write(((int)Kernel.ProcessManager.Processes[i].Type).ToString().PadRight(8, ' '));
                Console.Write((Kernel.ProcessManager.Processes[i].Running ? 1 : 0).ToString().PadRight(9, ' '));
                Console.Write(Kernel.ProcessManager.Processes[i].Name.ToString().PadRight(24, ' '));

                Console.WriteLine();
            }

            for (int i = 0; i < Kernel.WindowManager.apps.Count; i++)
            {
                Console.Write(Kernel.WindowManager.apps[i].ID.ToString().PadRight(8, ' '));
                Console.Write(((int)Kernel.WindowManager.apps[i].Type).ToString().PadRight(8, ' '));
                Console.Write((Kernel.WindowManager.apps[i].Running ? 1 : 0).ToString().PadRight(9, ' '));
                Console.Write(Kernel.WindowManager.apps[i].Name.ToString().PadRight(24, ' '));
                Console.Write(Kernel.WindowManager.apps[i].Focused.ToString().PadRight(32, ' '));

                Console.WriteLine();
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
