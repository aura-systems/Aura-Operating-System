/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Consoles
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.System.Graphics;

namespace Aura_OS.System.Graphics.UI
{
    public abstract class Console
    {
        internal const char LineFeed = '\n';
        internal const char CarriageReturn = '\r';
        internal const char Tab = '\t';
        internal const char Space = ' ';

        public Console()
        {

        }

        public bool writecommand = false;
        public int commandindex = -1;
        public string cmd;

        public string Name;

        public ConsoleType Type;

        public abstract int X { get; set; }
        public abstract int Y { get; set; }

        public abstract int Width { get; }
        public abstract int Height { get; }

        public abstract int Cols { get; }
        public abstract int Rows { get; }

        public abstract void Clear();

        public abstract void Clear(uint color);

        public abstract void Write(char[] aText);

        public abstract void Write(byte[] aText);

        public abstract void UpdateCursor();

        public abstract ConsoleColor Foreground { get; set; }

        public abstract ConsoleColor Background { get; set; }

        public abstract int CursorSize { get; set; }

        public abstract bool CursorVisible { get; set; }

        public abstract void DrawImage(ushort X, ushort Y, Bitmap image);
    }

    public enum ConsoleType
    {
        Text,
        Graphical
    }
}