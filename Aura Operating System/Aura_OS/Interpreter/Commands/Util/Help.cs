/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using Aura_OS.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class CommandHelp : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandHelp(string[] commandvalues) : base(commandvalues)
        {
            Description = "to show all available commands and their description";
        }

        /// <summary>
        /// CommandHelp
        /// </summary>
        public override ReturnInfo Execute()
        {
            return ExecuteHelp(false);
        }

        /// <summary>
        /// CommandHelp
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0] == "/alias")
            {
                return ExecuteHelp(true);
            }
            else
            {
                return ExecuteHelp(false);
            }
        }

        private ReturnInfo ExecuteHelp(bool showaliases)
        {
            int count = 0;
            foreach (var command in Kernel.CommandManager.CMDs)
            {
                Kernel.console.Write("- ");
                if (showaliases)
                {
                    for (int i = 0; i < command.CommandValues.Length; i++)
                    {
                        if (i != command.CommandValues.Length - 1)
                        {
                            Kernel.console.Write(command.CommandValues[i] + ", ");
                        }
                        else
                        {
                            Kernel.console.Write(command.CommandValues[i]);
                        }
                    }
                }
                else
                {
                    Kernel.console.Write(command.CommandValues[0]);
                }
                Kernel.console.WriteLine(" (" + command.Description + ")");

                count++;
                if (count == Kernel.console.Rows - 3)
                {
                    //Console.ReadKey(); TODO FIX THIS
                    count = 0;
                }
            }
            Kernel.console.WriteLine();
            Kernel.console.WriteLine("You can see more information about a specific command by typing: {command} /help");
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
