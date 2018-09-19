/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Crash
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;

namespace Aura_OS.System.Shell.cmdIntr.Tests
{
    class Crash
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
        public Crash() { }

        /// <summary>
        /// c = command, c_Crash
        /// </summary>
        public static void c_Crash()
        {
            throw new Exception("Crash test");
        }
    }
}
