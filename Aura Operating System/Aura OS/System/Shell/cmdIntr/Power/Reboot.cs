/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Reboot
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Power
{
    class Reboot
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
        public Reboot() { }

        /// <summary>
        /// c = commnad, c_Reboot
        /// </summary>
        public static void c_Reboot()
        {
            if (Kernel.debugger != null)
            {
                if (Kernel.debugger.enabled)
                {
                    Kernel.debugger.Stop();
                }
            }
            Kernel.running = false;
            Console.Clear();
            L.Text.Display("restart");
            Sys.Power.Reboot();
        }
    }
}
