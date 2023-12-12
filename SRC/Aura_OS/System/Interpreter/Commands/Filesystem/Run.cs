/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Run Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandRun : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRun(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to run a program (only .bat command script)";
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute()
        {
            PrintHelp();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandTree
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (File.Exists(Kernel.CurrentDirectory + arguments[0]))
            {
                Batch.Execute(Kernel.CurrentDirectory + arguments[0]);
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - run {file}");
        }
    }
}