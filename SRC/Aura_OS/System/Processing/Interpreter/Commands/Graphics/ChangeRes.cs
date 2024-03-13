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
using Cosmos.System.Graphics;
using Aura_OS.System.Processing.Interpreter;

namespace Aura_OS.System.Processing.Interpreter.Commands.Graphics
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
            Console.WriteLine("Available modes:");

            foreach (var mode in Kernel.Canvas.AvailableModes)
            {
                Console.WriteLine("- " + mode.ToString());
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - changeres");
        }
    }
}