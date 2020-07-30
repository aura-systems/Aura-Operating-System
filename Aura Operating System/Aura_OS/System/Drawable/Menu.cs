/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Menus
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>   
*/

using System;

namespace Aura_OS.System.Drawable
{
    public class Menu
    {

        /// <summary>
        /// Display a progress bar
        /// </summary>
        public static void DispInstallationDialog(int percent)
        {
            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (10 / 2);

            if (percent == 0)
            {
                Console.Clear();

                Console.BackgroundColor = ConsoleColor.DarkBlue;

                switch (Kernel.langSelected)
                {
                    case "en_US":
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                        Console.SetCursorPosition(x, y);
                        Console.SetCursorPosition(x, y+1);
                        Console.WriteLine("║ Installation in Progress...                                  ║");
                        Console.SetCursorPosition(x, y);
                        break;

                    case "fr_FR":
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                        Console.SetCursorPosition(x, y);
                        Console.SetCursorPosition(x, y+1);
                        Console.WriteLine("║ Installation en cours...                                     ║");
                        Console.SetCursorPosition(x, y);
                        break;

                    case "nl_NL":
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                        Console.SetCursorPosition(x, y);
                        Console.SetCursorPosition(x, y+1);
                        Console.WriteLine("║ Installatie wordt uitgevoerd...                              ║");
                        Console.SetCursorPosition(x, y);
                        break;

                    case "it_IT":
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                        Console.SetCursorPosition(x, y);
                        Console.SetCursorPosition(x, y+1);
                        Console.WriteLine("║ Installazione in corso...                                    ║");
                        Console.SetCursorPosition(x, y);
                        break;
                        
                    case "pl_PL":
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                        Console.SetCursorPosition(x, y);
                        Console.SetCursorPosition(x, y+1);
                        Console.WriteLine("║ Instalacja trwa...                                           ║");
                        Console.SetCursorPosition(x, y);
                        break;
                }


                Console.SetCursorPosition(x, y+2);
                Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

                Console.SetCursorPosition(x, y+3);
                Console.WriteLine("║                                                              ║");
                Console.SetCursorPosition(x, y);

                Console.SetCursorPosition(x, y+4);
                Console.WriteLine("║                                                              ║");
                Console.SetCursorPosition(x, y);

                Console.SetCursorPosition(x, y+5);
                Console.WriteLine("║                                                              ║");
                Console.SetCursorPosition(x, y);

                Console.SetCursorPosition(x, y+6);
                Console.WriteLine("║                                                              ║");
                Console.SetCursorPosition(x, y);

                Console.SetCursorPosition(x, y+7);
                Console.WriteLine("║                                                              ║");
                Console.SetCursorPosition(x, y);

                Console.SetCursorPosition(x, y+8);
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 5)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+5);
                Console.WriteLine("###");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+60, y+1);
                Console.WriteLine("5%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 10)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("######");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("10%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 15)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#########");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("15%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 20)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("############");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("20%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 25)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("###############");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("25%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 30)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("##################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("30%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 35)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#####################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("35%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 40)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("########################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("40%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 45)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("###########################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("45%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 50)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("##############################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("50%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 55)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("55%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 60)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("####################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("60%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 65)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#######################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("65%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 70)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("##########################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("70%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 75)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#############################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("75%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 80)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("################################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("80%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 85)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("###################################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("85%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 90)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("######################################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("90%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 95)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("#########################################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("95%");
                Console.SetCursorPosition(x, y);
            }
            else if (percent == 100)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("############################################################");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x+59, y+1);
                Console.WriteLine("100%");
                Console.SetCursorPosition(x, y);
            }
            else
            {
                Console.SetCursorPosition(x+2, y+ 5);
                Console.WriteLine("                                                            ");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// Display the login form
        /// </summary>
        public static string DispLoginForm(string title)
        {
            Console.Clear();

            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (10 / 2);

            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║ " + title);
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+2);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+3);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+4);
            Console.WriteLine("║ Login:                                                       ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+5);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+6);
            Console.WriteLine("║ Password:                                                    ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+7);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+8);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+4);
            Console.Write("║ Login: ");
            string name = Console.ReadLine();
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+5);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+6);
            Console.Write("║ Password: ");
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.Black;
            string password = Utils.Password.ReadLine();
            Console.SetCursorPosition(x, y);

            string text = name + "//////" + password;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            return text;
        }

        /// <summary>
        /// Display the error dialog
        /// </summary>
        public static void DispErrorDialog(string error)
        {
            Console.Clear();

            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (4 / 2);

            Console.BackgroundColor = ConsoleColor.DarkRed;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║ " + error);
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+2);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey();

            Console.Clear();
        }

        /// <summary>
        /// Display the computer name dialog
        /// </summary>
        public static string DispDialogOneArg(string title, string input)
        {
            Console.Clear();

            title = "║ " + title;
            input = "║ " + input;

            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (10 / 2);

            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+1);
            Console.WriteLine(title);
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(x, y+2);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.SetCursorPosition(x, y+3);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+4);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+5);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+6);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+7);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+8);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(x, y+5);
            Console.Write(input);
            string name = Console.ReadLine();
            Console.SetCursorPosition(x, y);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            return name;
        }

        /// <summary>
        /// Display language dialog
        /// </summary>
        public static string DispLanguageDialog()
        {
            int x = (Kernel.AConsole.Width / 2) - (64 / 2);
            int y = (Kernel.AConsole.Height / 2) - (10 / 2);
            x_ = x;
            y_ = y;
            LanguageMenu(x, y);
            string[] item = { "English: en_US", "Français: fr_FR", "Dutch: nl_NL", "Italiano: it_IT", "Polski: pl_PL" };
            int language = GenericMenu(item, Langues, x, y);
            if (language == 0)
            {
                return "en_US";
            }
            else if (language == 1)
            {
                return "fr_FR";
            }
            else if (language == 2)
            {
                return "nl_NL";
            }
            else if (language == 3)
            {
                return "it_IT";
            }
            else if (language == 4)
            {
                return "pl_PL";
            }
            else
            {
                return "en_US";
            }
        }

        public static int x_;
        public static int y_;

        static void Langues()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x_ + 2, y_ + 3);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_+4);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_+5);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_+6);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x_ + 2, y_+7);
            Console.WriteLine(" ");
            Console.SetCursorPosition(x_lang, y_lang);
        }

        static int x_lang = Console.CursorLeft;
        static int y_lang = Console.CursorTop;

        static void LanguageMenu(int x, int y)
        {
            Console.Clear();

            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x_lang, y_lang);
            Console.SetCursorPosition(x, y+1);
            Console.WriteLine("║ Please choose a language:                                    ║");
            Console.SetCursorPosition(x_lang, y_lang);
            Console.SetCursorPosition(x, y+2);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.SetCursorPosition(x, y+3);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+4);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+5);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+6);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+7);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+8);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.SetCursorPosition(x, y+9);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x_lang, y_lang);

            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static int GenericMenu(string[] items, Action method, int x, int y)
        {
            int currentitem = 0, c;
            ConsoleKeyInfo key;

            method();

            int counter = y + 3;
            for (c = 0; c < items.Length; c++)
            {
                counter = counter + 1;
                if (currentitem == c)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(x + 2, counter);
                    Console.Write("* ");
                    Console.WriteLine(items[c]);
                    Console.SetCursorPosition(x_lang, y_lang);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(x + 2, counter);
                    Console.WriteLine("  " + items[c]);
                    Console.SetCursorPosition(x_lang, y_lang);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }

            while (key.Key != ConsoleKey.Enter)
            {
                
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.DownArrow)
                {
                    currentitem++;
                    if (currentitem > items.Length - 1) currentitem = 0;

                    method();

                    counter = y + 3;
                    for (c = 0; c < items.Length; c++)
                    {
                        counter = counter + 1;
                        if (currentitem == c)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.SetCursorPosition(x + 2, counter);
                            Console.WriteLine("*");
                            Console.SetCursorPosition(x_lang, y_lang);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.SetCursorPosition(x + 2, counter);
                            Console.WriteLine(" ");
                            Console.SetCursorPosition(x_lang, y_lang);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    currentitem--;
                    if (currentitem < 0) currentitem = items.Length - 1;

                    method();

                    counter = y + 3;
                    for (c = 0; c < items.Length; c++)
                    {
                        counter = counter + 1;
                        if (currentitem == c)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.SetCursorPosition(x + 2, counter);
                            Console.Write("* ");
                            Console.WriteLine(items[c]);
                            Console.SetCursorPosition(x_lang, y_lang);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.SetCursorPosition(x + 2, counter);
                            Console.WriteLine("  " + items[c]);
                            Console.SetCursorPosition(x_lang, y_lang);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                    }
                }
            }

            return currentitem;
        }

    }
}
