/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
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

                    foreach (var vol in Kernel.vFS.GetVolumes())
                    {
                        if (vol.mName == volume + ":\\")
                        {
                            exist = true;
                            Kernel.current_volume = vol.mName;
                            Kernel.current_directory = Kernel.current_volume;
                        }
                    }
                    if (!exist)
                    {
                        L.Text.Display("volumeinvalid");
                    }
                }
                catch
                {
                    L.Text.Display("volumeinvalid");
                }
            }
            else
            {
                L.Text.Display("volumeinvalid");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - chgvol {vol_name}");
        }
    }
}
