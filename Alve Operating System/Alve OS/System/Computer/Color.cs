using Alve_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alve_OS.System.Computer
{
    class Color
    {

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

        public static void GetTextColor(string defaultcolor = "7")
        {
            if (File.Exists(@"0:\System\color.set"))
            {
                string color = File.ReadAllText(@"0:\System\color.set");

                File.WriteAllText(@"0:\System\color.set", color);

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
            }
            else
            {
                File.Create(@"0:\System\color.set");
                File.WriteAllText(@"0:\System\color.set", defaultcolor);
            }
        }

        public static void SetTextColor(string color = "7")
        {
            if (File.Exists(@"0:\System\color.set"))
            {
                File.WriteAllText(@"0:\System\color.set", color);
                GetTextColor();
            }
            else
            {
                File.Create(@"0:\System\color.set");
                File.WriteAllText(@"0:\System\color.set", color);
            }

        }

        public static void GetBackgroundColor(string defaultcolor = "0")
        {
            if (File.Exists(@"0:\System\backcolor.set"))
            {
                string color = File.ReadAllText(@"0:\System\backcolor.set");

                File.WriteAllText(@"0:\System\backcolor.set", color);

                if (color.Equals("0"))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else if (color.Equals("1"))
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }
                else if (color.Equals("2"))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                }
                else if (color.Equals("3"))
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                }
                else if (color.Equals("4"))
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                else if (color.Equals("5"))
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                }
                else if (color.Equals("6"))
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                }
                else if (color.Equals("7"))
                {
                    Console.BackgroundColor = ConsoleColor.White;
                }
            }
            else
            {
                File.Create(@"0:\System\backcolor.set");
                File.WriteAllText(@"0:\System\backcolor.set", defaultcolor);
            }
        }

        public static void SetBackgroundColor(string color = "0")
        {
            if (File.Exists(@"0:\System\backcolor.set"))
            {
                File.WriteAllText(@"0:\System\backcolor.set", color);
                GetBackgroundColor();
            }
            else
            {
                File.Create(@"0:\System\backcolor.set");
                File.WriteAllText(@"0:\System\backcolor.set", color);
            }

        }
    }
}
