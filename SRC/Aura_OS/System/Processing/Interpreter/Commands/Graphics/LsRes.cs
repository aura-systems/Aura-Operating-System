/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Text;
using Cosmos.Kernel.System.Graphics;

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

            // Null before BeforeRun acquired the screen (e.g. lsres in boot.bat): a null dereference halts gen3 (C6).
            if (Kernel.Canvas != null)
            {
                // GEN3-GAP(display-mode): firmware/virtio-gpu displays list only the current mode
                // (resolution fixed by limine.conf); only VMware SVGA lists switchable modes.
                foreach (var mode in Kernel.Canvas.AvailableModes)
                {
                    sb.AppendLine("- " + mode.ToString());
                }
            }

            sb.AppendLine();
            sb.AppendLine("Displays:");

            for (int i = 0; i < DisplayManager.Count; i++)
            {
                DisplayDevice display;

                if (DisplayManager.TryGet(i, out display) && display != null)
                {
                    sb.AppendLine("- " + display.DriverName + " " + display.Name + " " + display.Width.ToString() + "x" + display.Height.ToString() + "x" + display.BitsPerPixel.ToString());
                }
            }

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}