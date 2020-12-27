/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Cat
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using L = Aura_OS.System.Translation;
namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandHex : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandHex(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to print an a file in hexadecimal";
        }

        /// <summary>
        /// CommandHex
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                string file = arguments[0];
                if (File.Exists(Kernel.current_directory + file))
                {
                    Console.WriteLine("Offset(h)  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F");
                    Console.WriteLine();
                    Console.WriteLine(Utils.Conversion.HexDump(File.ReadAllBytes(Kernel.current_directory + file)));
                }
                else
                {
                    L.Text.Display("doesnotexit");
                }
                return new ReturnInfo(this, ReturnCode.OK);
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
            Console.WriteLine("Usage:");
            Console.WriteLine(" - hex {file}");
        }
    }
}
