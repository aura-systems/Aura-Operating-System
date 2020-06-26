/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Settings
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;
using Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Settings
{
    class Passwd
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
        public Passwd() { }

        public static void c_Passwd(string username)
        {
            Char separator = ' ';
            string[] cmdargs = username.Split(separator);

            if (cmdargs.Length == 2)
            {
                username = cmdargs[1];
            }

            Text.Display("passwd:newpass");
            string newpassword = Console.ReadLine();
            Text.Display("passwd:retype");
            string retypepass = Console.ReadLine();

            if(newpassword == retypepass)
            {
                System.Users.Users User = new System.Users.Users();

                User.ChangePassword(username, newpassword);

                Text.Display("passwd:updated");
            }
            else
            {
                return;
            }            


        }
    }
}
