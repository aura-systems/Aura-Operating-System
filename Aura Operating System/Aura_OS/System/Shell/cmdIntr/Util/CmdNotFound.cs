/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CmdNotFound
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class CmdNotFound
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
        public CmdNotFound() { }

        /// <summary>
        /// c = command, c_CmdNotFound
        /// </summary>
        public static void c_CmdNotFound()
        {//TODO: More advanced color Parsing? xD
            Console.ForegroundColor = ConsoleColor.DarkRed;
            L.Text.Display("UnknownCommand");
            Console.ForegroundColor = ConsoleColor.White;
            //c_cic(ConsoleColor.DarkRed, "UnknownCommand", ConsoleColor.White, true, true);
        }
    }
}
