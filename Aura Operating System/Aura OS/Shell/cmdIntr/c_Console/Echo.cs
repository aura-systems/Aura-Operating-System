/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Echo
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using System;
namespace Aura_OS.Shell.cmdIntr.c_Console
{
    class Echo
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
        public Echo() { }

        /// <summary>
        /// c = Command, c_Echo
        /// </summary>
        /// <param name="txt">The string you wish to pass in. (to be echoed)</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Echo(string txt, short startIndex = 0, int count = 5)
        {
            txt = txt.Remove(startIndex, count);
			if (txt.StartsWith("$"))
			{
                try
                {
                    txt = txt.Remove(0, 1);
                    Console.WriteLine(Kernel.environmentvariables[txt]);
                }
                catch { }
            }
			else 
			{
				Console.WriteLine(txt);
			}
        }
    }
}
