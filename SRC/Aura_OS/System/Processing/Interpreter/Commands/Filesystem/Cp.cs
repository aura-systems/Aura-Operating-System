/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Rm
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Interpreter.Commands.Filesystem
{
    class CommandCopy : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCopy(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to copy a file or directory";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string sourcePath = AuraPath.Resolve(arguments[0]);
            string destPath = AuraPath.Resolve(arguments[1]);

            // GEN3-GAP(fat-names): the FAT driver writes any character into a long name, so reject the
            // reserved ones in the destination (a directory copy creates the whole chain).
            foreach (string part in destPath.Split('/'))
            {
                if (part.Length > 0 && !AuraPath.IsValidName(part))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid destination name.");
                }
            }

            try
            {
                if (Entries.ForceCopy(sourcePath, destPath))
                {
                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Failed to copy.");
                }
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
            Console.WriteLine(" - cp {source_file/directory} {destination_file/directory}");
        }
    }
}
