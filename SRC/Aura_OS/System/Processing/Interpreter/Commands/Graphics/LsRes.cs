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

            sb.AppendLine("Current mode: " + Kernel.Canvas.Mode.ToString());
            sb.AppendLine();
            sb.AppendLine("Available modes:");

            // GEN3-GAP(video-mode): gen3 has a single Limine-negotiated framebuffer fixed at
            // boot; AvailableModes only reports the current mode and modes cannot be switched
            // at runtime.
            foreach (var mode in Kernel.Canvas.AvailableModes)
            {
                sb.AppendLine("- " + mode.ToString());
            }

            sb.AppendLine();
            sb.AppendLine("Note: the video mode is fixed at boot on Cosmos gen3 and cannot be changed at runtime.");

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}