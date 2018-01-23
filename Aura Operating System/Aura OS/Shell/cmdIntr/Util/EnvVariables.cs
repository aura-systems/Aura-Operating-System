/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Environment variables
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.Shell.cmdIntr.Util
{
    class EnvVar
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
        public EnvVar() { }

        /// <summary>
        /// c = command, c_Export
        /// </summary>
        /// <param name="arg">The command</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Export(string arg, short startIndex = 0, short count = 7)
        {
			string str = arg.Remove(startIndex, count);
			string[] exportcmd = str.Split('=');
			string var = exportcmd[0];
			string value = exportcmd[1];
            Kernel.environmentvariables.Add(var, value);
		}
    }
}
