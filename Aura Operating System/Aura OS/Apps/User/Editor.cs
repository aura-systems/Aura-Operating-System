/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Liquid Text Editor
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   TheCool1Kevin        <kevindai02@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Translation;
using System.Linq;

namespace Aura_OS.Apps.User
{
    class Editor
    {
        public static string prgm_version = "0.2";
        char[] line = new char[80]; int pointer = 0;
        //List<string> lines = new List<string>();
        string[] final;

        int x = Console.CursorLeft;
        int y = Console.CursorTop;

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
            Start(filename, currentdirectory);
        }

        internal void Start(string filename, string currentdirectory)
        {
            Console.Clear();
            drawTopBar();
            Console.SetCursorPosition(0, 1);
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
                    //lines.Add();

                    final = lines.Values.ToArray();

                    string foo = concatString(final);
                    File.Create(currentdirectory + filename);
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
                        //Console.SetCursorPosition(72, 24);
                        //Console.WriteLine(linecounter + " lines");
                        //Console.SetCursorPosition(x, y);
                        break;
                    default: line[pointer] = ch; pointer++; Console.Write(ch); break;
                }
            }
            Console.Clear();
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
                    Console.Write("Liquid Editor v" + prgm_version + "                    ");
                    break;
                case "en_US":
                    Console.Write("Liquid Editor v" + prgm_version + "                                  ");
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
                    //lines.Remove(linecounter);
                    int cursorleft = lines.Values[linecounter - 1].Length;
                    Console.CursorLeft = Console.CursorLeft + cursorleft;
                    pointer = cursorleft;
                    linecounter--;
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
