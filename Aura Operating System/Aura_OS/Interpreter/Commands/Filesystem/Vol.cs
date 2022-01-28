/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Vol
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;

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
            var vols = Kernel.VirtualFileSystem.GetVolumes();

            Kernel.console.WriteLine();
            Kernel.console.WriteLine("  Volume ###\tFormat\tSize");
            Kernel.console.WriteLine("  ----------\t------\t--------");

            foreach (var vol in vols)
            {
                if (vol.mName == Kernel.CurrentVolume && vols.Count > 1)
                {
                    Kernel.console.WriteLine(" >" + vol.mName + "\t   \t" + Kernel.VirtualFileSystem.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
                else
                {
                    Kernel.console.WriteLine("  " + vol.mName + "\t   \t" + Kernel.VirtualFileSystem.GetFileSystemType(vol.mName) + " \t" + vol.mSize + " MB\t" + vol.mParent);
                }
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}