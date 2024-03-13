/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Graphics
{
    class CommandLsRes : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandLsRes(string[] commandvalues) : base(commandvalues)
        {
            Description = "to list available screen resolutions";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Available modes:");

            foreach (var mode in Kernel.Canvas.AvailableModes)
            {
                sb.AppendLine("- " + mode.ToString());
            }

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}