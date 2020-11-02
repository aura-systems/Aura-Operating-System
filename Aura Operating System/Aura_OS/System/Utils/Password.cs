using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils
{
    class Password
    {
        private static Aura_OS.System.AConsole.Console GetConsole()
        {
            return Aura_OS.Kernel.AConsole;
        }

        public static String ReadLine()
        {
            var xConsole = GetConsole();
            if (xConsole == null)
            {
                // for now:
                return null;
            }
            List<char> chars = new List<char>(32);
            Cosmos.System.KeyEvent current;

            int currentCount = 0;

            bool firstdown = false;

            string CMDToComplete = "";

            while ((current = Cosmos.System.KeyboardManager.ReadKey()).Key != Cosmos.System.ConsoleKeyEx.Enter)
            {
                if (current.Key == Cosmos.System.ConsoleKeyEx.NumEnter) break;
                //Check for "special" keys
                if (current.Key == Cosmos.System.ConsoleKeyEx.Backspace) // Backspace
                {
                    CMDToComplete = "";
                    if (currentCount > 0)
                    {
                        int curCharTemp = GetConsole().X;
                        chars.RemoveAt(currentCount - 1);
                        GetConsole().X = GetConsole().X - 1;

                        //Move characters to the left
                        for (int x = currentCount - 1; x < chars.Count; x++)
                        {
                            Console.Write(chars[x]);
                        }

                        Console.Write(' ');

                        GetConsole().X = curCharTemp - 1;

                        currentCount--;
                    }
                    else
                    {
                        Cosmos.System.PCSpeaker.Beep();
                    }
                    continue;
                }
                else if (current.Key == Cosmos.System.ConsoleKeyEx.LeftArrow)
                {
                    if (currentCount > 0)
                    {
                        GetConsole().X = GetConsole().X - 1;
                        currentCount--;
                    }
                    continue;
                }
                else if (current.Key == Cosmos.System.ConsoleKeyEx.RightArrow)
                {
                    if (currentCount < chars.Count)
                    {
                        GetConsole().X = GetConsole().X + 1;
                        currentCount++;
                    }
                    continue;
                }

                if (current.KeyChar == '\0') continue;

                //Console.Write the character to the screen
                if (currentCount == chars.Count)
                {
                    chars.Add(current.KeyChar);
                    Console.Write("*");
                    //Console.Write("●");
                    currentCount++;
                }
                else
                {
                    //Insert the new character in the correct location
                    //For some reason, List.Insert() doesn't work properly
                    //so the character has to be inserted manually
                    List<char> temp = new List<char>();

                    for (int x = 0; x < chars.Count; x++)
                    {
                        if (x == currentCount)
                        {
                            temp.Add(current.KeyChar);
                        }

                        temp.Add(chars[x]);
                    }

                    chars = temp;

                    //Shift the characters to the right
                    for (int x = currentCount; x < chars.Count; x++)
                    {
                        Console.Write(chars[x]);
                    }

                    GetConsole().X -= (chars.Count - currentCount) - 1;
                    currentCount++;
                }
            }
            Console.WriteLine();

            char[] final = chars.ToArray();
            return new string(final);
        }
    }
}
