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
        /// CommandLspci
        /// </summary>
        public override ReturnInfo Execute()
        {
            int count = 0;
            foreach (var command in CommandManager.CMDs)
            {
                Console.WriteLine("- " + command.CommandValues[0] + " (" + command.Description +  ")");
                count++;
                if (count == Console.WindowHeight)
                {
                    Console.ReadKey();
                    count = 0;
                }
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
