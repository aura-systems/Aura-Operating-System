/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command history
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;

namespace Aura_OS.System.Utils
{
    public class CommandsHistory
    {
        public static int CHIndex = 0;
        public static List<string> commands = new List<string>();

        public static void Add(string cmd)
        {
            commands.Add(cmd);
            CommandsHistory.CHIndex = commands.Count - 1;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

    }
}