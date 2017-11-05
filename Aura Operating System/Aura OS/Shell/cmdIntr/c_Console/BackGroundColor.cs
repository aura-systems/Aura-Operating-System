/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - BackGroundColor
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;

namespace Aura_OS.Shell.cmdIntr.c_Console
{
    class BackGroundColor
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
        public BackGroundColor() {  }

        /// <summary>
        /// c = commnad, c_BackGroundColor
        /// </summary>
        public static void c_BackGroundColor()
        {
            L.Color.Display();
        }

        /// <summary>
        /// c = commnad, c_BackGroundColor
        /// <para>bg_color 0 = Black</para>
        /// <para>bg_color 1 = Blue</para>
        /// <para>bg_color 2 = Green</para>
        /// <para>bg_color 3 = DarkBlue</para>
        /// <para>bg_color 4 = Red</para>
        /// <para>bg_color 5 = Magenta</para>
        /// <para>bg_color 6 = Yellow</para>
        /// <para>bg_color 7 = White</para>
        /// </summary>
        /// <param name="bg_colour">The number of the color you wish to pass in.</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static bool c_BackGroundColor(string color, short startIndex = 0, short count = 16)
        {
            //string color = bg_colour.Remove(startIndex, count);
            if (color.Equals("0"))
            {
                Color.SetBackgroundColor("0");
                return true;
            }
            else if (color.Equals("1"))
            {
                Color.SetBackgroundColor("1");
                return true;
            }
            else if (color.Equals("2"))
            {
                Color.SetBackgroundColor("2");
                return true;
            }
            else if (color.Equals("3"))
            {
                Color.SetBackgroundColor("3");
                return true;
            }
            else if (color.Equals("4"))
            {
                Color.SetBackgroundColor("4");
                return true;
            }
            else if (color.Equals("5"))
            {
                Color.SetBackgroundColor("5");
                return true;
            }
            else if (color.Equals("6"))
            {
                Color.SetBackgroundColor("6");
                return true;
            }
            else if (color.Equals("7"))
            {
                Color.SetBackgroundColor("7");
                return true;
            }
            else
            {
                L.Text.Display("unknowncolor");
                c_BackGroundColor();
                return false;
            }
        }
    }
}
