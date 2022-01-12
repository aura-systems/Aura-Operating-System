/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - MIV command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*                   bartashevich <bartashevich@ua.pt>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.Config;
using Aura_OS.Apps.User;

namespace Aura_OS.System.Shell.cmdIntr.Tools
{
    class CommandMIV : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandMIV(string[] commandvalues) : base(commandvalues, CommandType.Utils)
        {
            Description = "to start MIV Editor";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            PrintHelp();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string file = arguments[0];
            
            MIV.StartMIV(file);

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - miv {filepath}");
        }
    }
}