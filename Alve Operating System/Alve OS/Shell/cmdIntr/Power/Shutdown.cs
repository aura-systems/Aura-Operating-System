/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Shutdown
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.Power
{
    class Shutdown
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
        public Shutdown() { }

        /// <summary>
        /// c = commnad, c_Shutdown
        /// </summary>
        public static void c_Shutdown()
        {
            Kernel.running = false;
            Console.Clear();
            L.Text.Display("shutdown");
            Sys.Power.Shutdown();
        }
    }
}
