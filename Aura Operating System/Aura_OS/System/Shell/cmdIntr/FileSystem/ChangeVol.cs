/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Change Vol
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.FileSystem
{
    class ChangeVol
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public ChangeVol() { }

        /// <summary>
        /// c = command, c_ChangeVol
        /// </summary>
        /// <param name="cmd">The command</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_ChangeVol(string cmd, short startIndex = 0, short count = 0)
        {
            if (cmd.Remove(0, 1) == ":")
            {
                try
                {
                    string volume = cmd.Remove(1, 1);

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
        }
    }
}
