/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Clear
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using System;
namespace Aura_OS.System.Shell.cmdIntr.c_Console
{
    class Clear
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
        public Clear() { }

        /// <summary>
        /// c = commnad, c_Clear
        /// </summary>
        public static void c_Clear()
        {
            Console.Clear();
        }
    }
}
