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
        public static void DispInstallationDialog()
        {
            Console.Clear();

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.Blue;

            switch(Kernel.langSelected)
            {
                case "en_US":
                    Console.SetCursorPosition(8, 10);
                    Console.WriteLine("╔══════════════════ Installation in progress ══════════════════╗");
                    Console.SetCursorPosition(x, y);
                    break;
                case "fr_FR":
                    Console.SetCursorPosition(8, 10);
                    Console.WriteLine("╔═══════════════════ Installation en cours ════════════════════╗");
                    Console.SetCursorPosition(x, y);
                    break;
            }


            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 14);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Display the login form
        /// </summary>
        public static string DispLoginForm(string title)
        {
            Console.Clear();
            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.Blue;

            Console.SetCursorPosition(8, 8);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 9);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 9);
            Console.WriteLine("║ " + title);
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 10);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║ Login:                                                       ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║ Password:                                                    ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 15);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 16);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.Write("║ Login: ");
            string name = Console.ReadLine();
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 14);
            Console.Write("║ Password: ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            string password = Console.ReadLine();
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

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.DarkRed;

            Console.SetCursorPosition(8, 11);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║ " + error);
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey();

            Console.Clear();
        }

        /// <summary>
        /// Display language dialog
        /// </summary>
        public static string DispLanguageDialog()
        {
            Console.Clear();

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.Blue;

            Console.SetCursorPosition(8, 10);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║ Please choose a language: (available: en-US fr-FR)           ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 15);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.Write("║ Language: ");
            string language = Console.ReadLine();
            Console.SetCursorPosition(x, y);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            return language;
        }

        /// <summary>
        /// Display the computer name dialog
        /// </summary>
        public static string DispCompuernameDialog(string title, string input)
        {
            Console.Clear();

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.BackgroundColor = ConsoleColor.Blue;

            Console.SetCursorPosition(8, 10);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine(title);
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 15);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.Write(input);
            string name = Console.ReadLine();
            Console.SetCursorPosition(x, y);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            return name;
        }

    }
}
