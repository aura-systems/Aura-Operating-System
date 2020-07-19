/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command history
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;

namespace Aura_OS.System.Utils
{
    public class CommandsHistory
    {
        public static int CHIndex = 0;

        public static void Add(string cmd)
        {
            Kernel.AConsole.commands.Add(cmd);
            CommandsHistory.CHIndex = Kernel.AConsole.commands.Count - 1;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

    }
}