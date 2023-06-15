/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.Config;
using Aura_OS;
using Cosmos.System.Graphics;
using Cosmos.System;
using Aura_OS.Interpreter;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandChangeRes : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandChangeRes(string[] commandvalues) : base(commandvalues)
        {
            Description = "to change screen resolution";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            Kernel.console.WriteLine("Available modes:");

            foreach (var mode in Kernel.canvas.AvailableModes)
            {
                Kernel.console.WriteLine("- " + mode.ToString());
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count != 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                uint width = uint.Parse(arguments[0]);
                uint height = uint.Parse(arguments[1]);

                bool modeExists = false;

                foreach (var mode in Kernel.canvas.AvailableModes)
                {
                    if (mode.Width == width && mode.Height == height)
                    {
                        modeExists = true;
                    }
                }

                if (modeExists)
                {
                    Kernel.screenWidth = (uint)width;
                    Kernel.screenHeight = (uint)height;

                    MouseManager.ScreenWidth = (uint)width;
                    MouseManager.ScreenHeight = (uint)height;

                    Kernel.canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(width, height, ColorDepth.ColorDepth32));

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Requested graphic mode is not supported.");
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            } 
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - changeres");
            Kernel.console.WriteLine(" - changeres {x} {y}");
        }
    }
}