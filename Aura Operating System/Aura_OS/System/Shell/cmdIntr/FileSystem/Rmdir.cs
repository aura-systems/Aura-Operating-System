/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rmdir
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandRmdir : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRmdir(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to remove a directory";
        }

        /// <summary>
        /// CommandMkdir
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string dir = arguments[0];
            if (Directory.Exists(Kernel.current_directory + dir))
            {
                Directory.Delete(Kernel.current_directory + dir, true);
                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                L.Text.Display("doesnotexist", dir);
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - rmdir {dir}");
        }
    }
}
