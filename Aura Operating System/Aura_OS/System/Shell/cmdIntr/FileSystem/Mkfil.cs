/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Mkfil
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandMkfil : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandMkfil(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to create a file";
        }

        /// <summary>
        /// CommandMkfil
        /// </summary>
        public override ReturnInfo Execute()
        {
            L.Text.Display("mkfil");
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandMkfil
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string file = arguments[0];

            if (!File.Exists(Kernel.current_directory + file))
            {
                /*Apps.User.Editor application = new Apps.User.Editor();
                application.Start(file, Kernel.current_directory);*/
                File.Create(Kernel.current_directory + file);
            }
            else
            {
                L.Text.Display("alreadyexist", file);
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - mkfir {file}");
        }
    }
}
