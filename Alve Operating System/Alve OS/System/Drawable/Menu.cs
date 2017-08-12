using System;
using System.Collections.Generic;
using System.Text;

namespace Alve_OS.System.Drawable
{
    public class Menu
    {
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
            string password = Console.ReadLine();
            Console.SetCursorPosition(x, y);

            string text = name + "//////" + password;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            return text;
        }

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

        public static string DispSetupForm(string title)
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
            string password = Console.ReadLine();
            Console.SetCursorPosition(x, y);

            string text = name + "//////" + password;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            return text;
        }

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
