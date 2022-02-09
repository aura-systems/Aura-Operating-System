/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Interpreter;
using System;
using System.Collections.Generic;

namespace Aura_OS.Interpreter.Commands.Filesystem
{
    class CommandChangeVol : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandChangeVol(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to change current volume";
        }

        /// <summary>
        /// CommandChangeVol
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0].Remove(0, 1) == ":")
            {
                try
                {
                    string volume = arguments[0].Remove(1, 1);

                    bool exist = false;

                    foreach (var vol in Kernel.VirtualFileSystem.GetVolumes())
                    {
                        if (vol.mName == volume + ":\\")
                        {
                            exist = true;
                            Kernel.CurrentVolume = vol.mName;
                            Kernel.CurrentDirectory = Kernel.CurrentVolume;
                        }
                    }
                    if (!exist)
                    {
                        Kernel.console.WriteLine("The specified drive is not found.");
                    }
                }
                catch
                {
                    Kernel.console.WriteLine("The specified drive is not found.");
                }
            }
            else
            {
                Kernel.console.WriteLine("The specified drive is not found.");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Usage:");
            Kernel.console.WriteLine(" - chgvol {vol_name}");
        }
    }
}