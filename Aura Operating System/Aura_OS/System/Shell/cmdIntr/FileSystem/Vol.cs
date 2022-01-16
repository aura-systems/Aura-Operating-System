/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Vol
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class CommandVol : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVol(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to list volumes";
        }

        /// <summary>
        /// CommandVol
        /// </summary>
        public override ReturnInfo Execute()
        {
            var vols = Global.vFS.GetVolumes();

            L.Text.Display("volCommand");

            foreach (var vol in vols)
            {
                if (vol.mName == Global.current_volume && vols.Count > 1)
                {
                    Console.WriteLine(" >" + vol.mName + "\t   \t" + Global.vFS.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
                else
                {
                    Console.WriteLine("  " + vol.mName + "\t   \t" + Global.vFS.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
