/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Consoles
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Shell
{
    public abstract class Console
    {

        public Console()
        {
            
        }

        public string Name;

        public abstract  int X { get; set; }
        public abstract  int Y { get; set; }

        public abstract  int Cols { get; }
        public abstract  int Rows { get; }

        public abstract void Clear();

        public abstract void Write(byte[] aText);

        public abstract ConsoleColor Foreground { get; set; }

        public abstract ConsoleColor Background { get; set; }

        public abstract int CursorSize { get; set; }

        public abstract bool CursorVisible { get; set; }
    }
}
