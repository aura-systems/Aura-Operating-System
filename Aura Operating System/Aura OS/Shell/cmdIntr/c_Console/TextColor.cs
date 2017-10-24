/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - TextColor
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;

namespace Aura_OS.Shell.cmdIntr.c_Console
{
    class TextColor
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
        public TextColor() { }

        /// <summary>
        /// c = commnad, c_TextColor
        /// </summary>
        public static void c_TextColor()
        {
            L.Color.Display();
        }

        /// <summary>
        /// c = commnad, c_TextColor
        /// <para>bg_color 0 = Black</para>
        /// <para>bg_color 1 = Blue</para>
        /// <para>bg_color 2 = Green</para>
        /// <para>bg_color 3 = DarkBlue</para>
        /// <para>bg_color 4 = Red</para>
        /// <para>bg_color 5 = Magenta</para>
        /// <para>bg_color 6 = Yellow</para>
        /// <para>bg_color 7 = White</para>
        /// </summary>
        /// <param name="fg_colour">The number of the color you wish to pass in</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_TextColor(string color, short startIndex = 0, short count = 10)
        {
            //string color = fg_colour.Remove(startIndex, count);
            if (color.Equals("0"))
            {
                Color.SetTextColor("0");
                Kernel.color = 0;
            }
            else if (color.Equals("1"))
            {
                Color.SetTextColor("1");
                Kernel.color = 1;
            }
            else if (color.Equals("2"))
            {
                Color.SetTextColor("2");
                Kernel.color = 2;
            }
            else if (color.Equals("3"))
            {
                Color.SetTextColor("3");
                Kernel.color = 3;
            }
            else if (color.Equals("4"))
            {
                Color.SetTextColor("4");
                Kernel.color = 4;
            }
            else if (color.Equals("5"))
            {
                Color.SetTextColor("5");
                Kernel.color = 5;
            }
            else if (color.Equals("6"))
            {
                Color.SetTextColor("6");
                Kernel.color = 6;
            }
            else if (color.Equals("7"))
            {
                Color.SetTextColor("7");
                Kernel.color = 7;
            }
            else
            {
                L.Text.Display("unknowncolor");
                c_TextColor();
                Kernel.color = -1;
            }
        }
    }
}
