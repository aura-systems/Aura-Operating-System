/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Display bitmap
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Filesystem;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandPicture : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPicture(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to display a bitmap in a new window";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                string path = AuraPath.Resolve(arguments[0]);

                if (!File.Exists(path))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
                }

                Package picture = Kernel.PackageManager.Find("Picture");

                if (picture == null || !picture.IsApp)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "The Picture package is missing.");
                }

                // Throws for a file it cannot show, reported by the catch below.
                Kernel.ApplicationManager.StartPackage(picture, new List<string> { path });

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new ReturnInfo(this, ReturnCode.ERROR);
            }
        }


        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - pic {source_file}");
        }
    }
}
