/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Translation;
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
            foreach (var command in CommandManager.CMDs)
            {
                Console.Write("- ");
                if (showaliases)
                {
                    for (int i = 0; i < command.CommandValues.Length; i++)
                    {
                        if (i != command.CommandValues.Length - 1)
                        {
                            Console.Write(command.CommandValues[i] + ", ");
                        }
                        else
                        {
                            Console.Write(command.CommandValues[i]);
                        }
                    }
                }
                else
                {
                    Console.Write(command.CommandValues[0]);
                }
                Console.WriteLine(" (" + command.Description + ")");

                count++;
                if (count == Console.WindowHeight)
                {
                    Console.ReadKey();
                    count = 0;
                }
            }
            Console.WriteLine();
            Console.WriteLine("You can see more information about a specific command by typing: {command} /help");
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available command:");
            Console.WriteLine("- help /alias    show command aliases.");
        }
    }
}
