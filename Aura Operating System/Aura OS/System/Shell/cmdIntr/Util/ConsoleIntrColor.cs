/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - ConsoleIntrColor
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class ConsoleIntrColor
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
        public ConsoleIntrColor() { }

        /// <summary>
        /// ConsoleIntrColor
        /// c = command, c_cic
        /// </summary>
        /// <param name="set">The start color, example DarkRed.</param>
        /// <param name="txt">The text you wish to chage.</param>
        /// <param name="reset">The color you wish to finish with, example WHITE.</param>
        /// <param name="lg_file">Is it a Language file (french or english)</param>
        /// <param name="nl">Should is be Console.Writeline (new line) or a Console.Write (not new line) (nl = new line)</param>
        public static void c_cic(ConsoleColor set = ConsoleColor.Black, string txt = "", ConsoleColor reset = ConsoleColor.White, bool lg_file = false, bool nl = true)
        {
            Console.ForegroundColor = set;
            if (lg_file) L.Text.Display(txt);
            else if (nl) Console.WriteLine(txt);
            else Console.Write(txt);
            Console.ForegroundColor = reset;
        }

    }
}
/*
 * 
 *   Console.ForegroundColor = ConsoleColor.DarkRed;
            L.Text.Display("UnknownCommand");
            Console.ForegroundColor = ConsoleColor.White;
 */
