/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Liquid Text Editor
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   TheCool1Kevin        <kevindai02@outlook.com>
*/

using System;
using System.IO;
using Aura_OS.System.Translation;
using Aura_OS.System.Drawable;

namespace Aura_OS.Apps.User
{
    class Editor
    {

        public static string prgm_version = "0.3";
        char[] line = new char[80]; int pointer = 0;
        string[] final;

        Aura_OS.System.Utils.Dictionary<int, string> lines = new Aura_OS.System.Utils.Dictionary<int, string>();
        int linecounter = 0;

        internal void filepath(string currentdirectory)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Text.Display("liquideditor", prgm_version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Text.Display("filename");
            string filename = Console.ReadLine();
            Start(filename, currentdirectory, false);
        }

        internal void Start(string filename, string currentdirectory, bool exists)
        {
            Console.Clear();
            drawTopBar();
            Console.SetCursorPosition(0, 1);

            if (exists)
            {
                string[] file = File.ReadAllLines(currentdirectory + filename);

                foreach (string value in file)
                {
                    line = value.ToCharArray();
                    Console.Write(line);
                    lines.Add(linecounter++, new string(line).TrimEnd()); cleanArray(line); Console.CursorLeft = 0; Console.CursorTop++; pointer = 0;
                }
            }
            
            ConsoleKeyInfo c; cleanArray(line);
            while ((c = Console.ReadKey(true)) != null)
            {
                drawTopBar();
                char ch = c.KeyChar;
                if (c.Key == ConsoleKey.Escape)
                    break;
                else if (c.Key == ConsoleKey.F1)
                {
                    Console.Clear();
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Text.Display("liquideditor", prgm_version);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;

                    lines.Add(linecounter++, new string(line).TrimEnd());

                    final = lines.Values.ToArray();

                    string foo = concatString(final);

                    if (!exists)
                    {
                        File.Create(currentdirectory + filename);
                    }

                    File.WriteAllText(currentdirectory + filename, foo); 
                    Console.ForegroundColor = ConsoleColor.Green;
                    int lastline = 0;
                    foreach (int number in lines.Keys)
                    {
                        lastline = number;
                    }
                    Console.WriteLine("Number of lines: " + lastline);
                    Text.Display("saved", filename, currentdirectory);
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.ReadKey();
                    break;
                }
                else if (c.Key == ConsoleKey.F2)
                {
                    filepath(Kernel.current_directory);
                    break;
                }
                else if (c.Key == ConsoleKey.F3)
                {
                    drawSettings();
                    break;
                }
                switch (c.Key)
                {
                    case ConsoleKey.Home: break;
                    case ConsoleKey.PageUp: break;
                    case ConsoleKey.PageDown: break;
                    case ConsoleKey.End: break;
                    case ConsoleKey.UpArrow:
                        if (Console.CursorTop > 1)
                        {
                            Console.CursorTop = Console.CursorTop - 1;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (Console.CursorTop < 23)
                        {
                            Console.CursorTop = Console.CursorTop + 1;
                        }
                        break;
                    case ConsoleKey.LeftArrow: if (pointer > 0) { pointer--; Console.CursorLeft--; } break;
                    case ConsoleKey.RightArrow: if (pointer < 80) { pointer++; Console.CursorLeft++; if (line[pointer] == 0) line[pointer] = ' '; } break;
                    case ConsoleKey.Backspace: deleteChar(); break;
                    case ConsoleKey.Delete: deleteChar(); break;
                    case ConsoleKey.Enter:
                        lines.Add(linecounter++, new string(line).TrimEnd()); cleanArray(line); Console.CursorLeft = 0; Console.CursorTop++;  pointer = 0;
                        break;
                    default: line[pointer] = ch; pointer++; Console.Write(ch); break;
                }
            }
            Console.Clear();
        }

        static int x = Console.CursorLeft;
        static int y = Console.CursorTop;

        private string drawSettings()
        {
            settingsMenu();
            string[] item = { "C", "C#", "ASM", "None" };
            int language = Menu.GenericMenu(item, Settings);
            if (language == 0)
            {
                return "C";
            }
            else if (language == 1)
            {
                return "C#";
            }
            else if (language == 2)
            {
                return "ASM";
            }
            else
            {
                return "none";
            }
        }

        static void Settings()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
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
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 15);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private void settingsMenu()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(8, 8);
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 9);
            Console.WriteLine("║ Settings                                                     ║");
            Console.SetCursorPosition(x, y);
            Console.SetCursorPosition(8, 10);
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.SetCursorPosition(8, 11);
            Console.WriteLine("║ Programming language:                                        ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 12);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 13);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 14);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 15);
            Console.WriteLine("║                                                              ║");
            Console.SetCursorPosition(x, y);

            Console.SetCursorPosition(8, 16);
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private string concatString(string[] s)
        {
            string t = "";
            if (s.Length >= 1)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(s[i]))
                    {
                        t = string.Concat(t, s[i].TrimEnd(), Environment.NewLine);
                    }
                }
            }
            else
            {
                t = s[0];
            }
            //t = string.Concat(t, '\0');
            return t;
        }

        private void cleanArray(char[] c)
        {
            for (int i = 0; i < c.Length; i++)
                c[i] = ' ';
        }

        private void drawTopBar()
        {
            int x = Console.CursorLeft, y = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.Write("Liquid Editor v" + prgm_version + "    ");
                    break;
                case "en_US":
                    Console.Write("Liquid Editor v" + prgm_version + "                    ");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Text.Display("menuliquideditor");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(x, y);
        }

        private void deleteChar()
        {
            if ((Console.CursorLeft >= 1) && (pointer >= 1))
            {
                pointer--; Console.CursorLeft--;
                Console.Write(" "); Console.CursorLeft--;
                line[pointer] = ' ';
            }
            else if ((Console.CursorLeft <= 1) && (pointer <= 1))
            {
                if ((Console.CursorTop > 1))
                {
                    Console.CursorTop = Console.CursorTop - 1;
                    int cursorleft = lines.Values[linecounter - 1].Length;
                    Console.CursorLeft = Console.CursorLeft + cursorleft;
                    pointer = cursorleft;

                    linecounter--;

                    string previouslines = lines.Values[linecounter];
                    lines.Values.RemoveAt(lines.Keys[linecounter]);
                    lines.Keys.RemoveAt(lines.Keys[linecounter]);

                    cleanArray(line);
                    line = previouslines.ToCharArray();
                }
            }
        }

        private void listCheck()
        {
            //foreach (var s in lines)
                //Text.Display("list", s);
        }

        private string[] arrayCheck(string[] s)
        {
            foreach (var ss in s)
            {
                Text.Display("line", ss);
            }
            return s;
        }
    }
}
