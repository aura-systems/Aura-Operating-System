/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command history
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;

namespace Aura_OS.System.Utils
{
    class CommandsHistory
    {
        public static int CHIndex = 0;

        public static void Add(string cmd)
        {
            Cosmos.System.Console.commands.Add(cmd);
            CommandsHistory.CHIndex = Cosmos.System.Console.commands.Count - 1;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            if (Kernel.Consolemode != "VGATextmode")
            {
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
        }

    }
}