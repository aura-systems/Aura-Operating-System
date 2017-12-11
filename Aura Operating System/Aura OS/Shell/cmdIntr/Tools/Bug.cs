/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Bug
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/

using Aura_OS.Apps.User;
using System;
using System.IO;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.Tools
{
    class Bug
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
        public Bug() { }

        /// <summary>
        /// c = command, c_Bug
        /// </summary>
        public static void c_Bug()
        {
            System.Utils.Bug bug = new System.Utils.Bug();
            bug.ScanIssues();
        }
    }
}
