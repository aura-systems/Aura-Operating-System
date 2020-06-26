/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Logout
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;

namespace Aura_OS.System.Shell.cmdIntr.Settings
{
    class Logout
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
        public Logout() { }

        /// <summary>
        /// c = command, c_Logout
        /// </summary>
        public static void c_Logout()
        {
            Kernel.Logged = false;
            Kernel.userLevelLogged = "";
            Kernel.userLogged = "";
            Directory.SetCurrentDirectory(Kernel.current_directory);
            Kernel.current_directory = Kernel.current_volume;
            Console.Clear();
        }
    }
}
