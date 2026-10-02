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
    class CommandRm : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandRm(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to remove a file or directory";
        }

        /// <summary>
        /// CommandRm
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            string path = arguments[0];
            string fullPath = AuraPath.Resolve(path);

            try
            {
                if (System.Filesystem.Entries.ForceRemove(fullPath))
                {
                    // The current directory (or one of its parents) is gone: move to the parent of the
                    // removed entry, or relative paths and "cd .." would point at a deleted folder.
                    if (AuraPath.AsDirectory(Kernel.CurrentDirectory).StartsWith(AuraPath.AsDirectory(fullPath), StringComparison.OrdinalIgnoreCase))
                    {
                        Kernel.CurrentDirectory = System.Filesystem.Utils.GetParentPath(fullPath);
                    }

                    return new ReturnInfo(this, ReturnCode.OK);
                }
            }
            catch (IOException ex)
            {
                // Device or FAT error (a mount point is refused by ForceRemove, the plug says EBUSY too).
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
            }

            return new ReturnInfo(this, ReturnCode.ERROR);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - rm {file/directory}");
        }
    }
}
