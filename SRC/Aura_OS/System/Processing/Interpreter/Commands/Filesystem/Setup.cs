/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Setup AuraOS
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandSetup : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandSetup(string[] commandvalues) : base(commandvalues)
        {
            Description = "to install AuraOS on a disk.";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 3)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string username = arguments[0];
            string password = arguments[1];
            string hostname = arguments[2];

            Setup setup = new();
            setup.InitSetup(username, password, hostname, "en-US");

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - setup {username} {password} {computername}");
        }
    }
}