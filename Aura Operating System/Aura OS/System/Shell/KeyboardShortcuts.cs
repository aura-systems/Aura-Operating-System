using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = System;

namespace Aura_OS.System.Shell
{
    class KeyboardShortcuts
    {

        public static bool Close()
        {
            ConsoleKeyInfo key;

            key = Sys.Console.ReadKey(true);

            if (key.Key == ConsoleKey.C && KeyboardManager.ControlPressed)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
