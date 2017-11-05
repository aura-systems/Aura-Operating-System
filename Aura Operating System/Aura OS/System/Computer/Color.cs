/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Console Color
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
using System;
using System.IO;

namespace Aura_OS.System.Computer
{
    class Color
    {

        /// <summary>
        /// Change text color
        /// </summary>
        /// <param name="color"></param>
        public static void DisplayTextColor(string color = "7")
        {
            if (color.Equals("0"))
            {
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (color.Equals("1"))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (color.Equals("2"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (color.Equals("3"))
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            }
            else if (color.Equals("4"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (color.Equals("5"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            else if (color.Equals("6"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (color.Equals("7"))
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Text.Display("unknowncolor");
            }
        }

        /// <summary>
        /// Get the actual text color set in color file
        /// </summary>
        /// <param name="defaultcolor"></param>
        /// <returns></returns>
        public static int GetTextColor(string defaultcolor = "7")
        {
            if (Kernel.SystemExists)
            {
                string color = Settings.GetValue("foregroundcolor");

                if (color.Equals("0"))
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Clear();
                    return 0;
                }
                else if (color.Equals("1"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Clear();
                    return 1;
                }
                else if (color.Equals("2"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Clear();
                    return 2;
                }
                else if (color.Equals("3"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.Clear();
                    return 3;
                }
                else if (color.Equals("4"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Clear();
                    return 4;
                }
                else if (color.Equals("5"))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Clear();
                    return 5;
                }
                else if (color.Equals("6"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Clear();
                    return 6;
                }
                else if (color.Equals("7"))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();
                    return 7;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return int.Parse(defaultcolor);
            }
        }

        /// <summary>
        /// Save actual text color in color file
        /// </summary>
        /// <param name="color"></param>
        public static void SetTextColor(string color = "7")
        {
            GetTextColor();
        }

        /// <summary>
        /// Get background color in color file
        /// </summary>
        /// <param name="defaultcolor"></param>
        public static void GetBackgroundColor(string defaultcolor = "0")
        {
            if (Kernel.SystemExists)
            {
                string color = Settings.GetValue("backgroundcolor");

                if (color.Equals("0"))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Clear();
                }
                else if (color.Equals("1"))
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.Clear();
                }
                else if (color.Equals("2"))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Clear();
                }
                else if (color.Equals("3"))
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Clear();
                }
                else if (color.Equals("4"))
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Clear();
                }
                else if (color.Equals("5"))
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                    Console.Clear();
                }
                else if (color.Equals("6"))
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Clear();
                }
                else if (color.Equals("7"))
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Clear();
                }
            }
        }

        /// <summary>
        /// Save background color in color file
        /// </summary>
        /// <param name="color"></param>
        public static void SetBackgroundColor(string color = "0")
        {
            GetBackgroundColor();
        }

    }
}
