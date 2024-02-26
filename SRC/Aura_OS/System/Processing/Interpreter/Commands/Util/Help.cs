/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
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
            StringBuilder sb = new();
            List<ICommand> commands = CommandManager.GetCommands();

            for (int i = 0; i < commands.Count; i++)
            {
                ICommand command = commands[i];

                sb.Append("- ");
                if (showaliases)
                {
                    for (int j = 0; j < command.CommandValues.Length; j++)
                    {
                        if (j != command.CommandValues.Length - 1)
                        {
                            sb.Append(command.CommandValues[j] + ", ");
                        }
                        else
                        {
                            sb.Append(command.CommandValues[j]);
                        }
                    }
                }
                else
                {
                    sb.Append(command.CommandValues[0]);
                }
                sb.AppendLine(" (" + command.Description + ")");

                /*
                count++;
                if (count == Kernel.console.Rows - 3)
                {
                    //Console.ReadKey(); TODO FIX THIS
                    count = 0;
                }
                */
            }
            Console.WriteLine(sb.ToString());
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
