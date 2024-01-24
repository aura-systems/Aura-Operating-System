/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public struct Cell
    {
        public char Char;
        public uint ForegroundColor;
        public uint BackgroundColor;
    }

    public class Console : Component
    {
        internal const char LineFeed = '\n';
        internal const char CarriageReturn = '\r';
        internal const char Tab = '\t';
        internal const char Space = ' ';

        private static uint[] Pallete = new uint[16];

        private Cell[] Text;

        public int mX = 0;
        public int mY = 0;

        public int mCols;
        public int mRows;

        public Color ForegroundColor = Color.White;
        public uint foreground = (byte)ConsoleColor.White;
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set
            {
                foreground = (uint)value;

                uint color = Pallete[foreground];
                byte r = (byte)(color >> 16 & 0xFF); // Extract the red component
                byte g = (byte)(color >> 8 & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                ForegroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public Color BackgroundColor = Color.Black;
        public uint background = (byte)ConsoleColor.Black;
        public ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set
            {
                background = (uint)value;

                uint color = Pallete[background];
                byte r = (byte)(color >> 16 & 0xFF); // Extract the red component
                byte g = (byte)(color >> 8 & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                BackgroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CursorVisible;

        public bool DrawBackground = true;

        public Console(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Pallete[0] = 0xFF000000; // Black
            Pallete[1] = 0xFF0000AB; // Darkblue
            Pallete[2] = 0xFF008000; // DarkGreen
            Pallete[3] = 0xFF008080; // DarkCyan
            Pallete[4] = 0xFF800000; // DarkRed
            Pallete[5] = 0xFF800080; // DarkMagenta
            Pallete[6] = 0xFF808000; // DarkYellow
            Pallete[7] = 0xFFC0C0C0; // Gray
            Pallete[8] = 0xFF808080; // DarkGray
            Pallete[9] = 0xFF5353FF; // Blue
            Pallete[10] = 0xFF55FF55; // Green
            Pallete[11] = 0xFF00FFFF; // Cyan
            Pallete[12] = 0xFFAA0000; // Red
            Pallete[13] = 0xFFFF00FF; // Magenta
            Pallete[14] = 0xFFFFFF55; // Yellow
            Pallete[15] = 0xFFFFFFFF; //White

            mCols = width / Kernel.font.Width - 1;
            mRows = height / Kernel.font.Height - 2;

            Text = new Cell[mCols * mRows];

            ClearText();

            CursorVisible = true;

            mX = 0;
            mY = 0;
        }

        private int GetIndex(int row, int col)
        {
            return row * mCols + col;
        }

        public override void Draw()
        {
            if (DrawBackground)
            {
                Kernel.canvas.DrawFilledRectangle(Kernel.BlackColor, X, Y, Width, Height);
            }

            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    int index = GetIndex(i, j);
                    if (Text[index].Char == 0 || Text[index].Char == '\n')
                        continue;

                    WriteByte(Text[index].Char, X + j * Kernel.font.Width, Y + i * Kernel.font.Height, Text[index].ForegroundColor);
                }
            }
        }

        public void WriteByte(char ch, int mX, int mY, uint color)
        {
            Kernel.canvas.DrawChar(ch, Kernel.font, Color.FromArgb((int)color), mX, mY);
        }

        public void SetCursorPos(int mX, int mY)
        {
            if (CursorVisible)
            {
                Kernel.canvas.DrawFilledRectangle(ForegroundColor, X + mX * Kernel.font.Width,
                    Y + mY * Kernel.font.Height + Kernel.font.Height, 8, 4);
            }
        }

        private void ClearText()
        {
            for (int i = 0; i < Text.Length; i++)
            {
                Text[i].Char = (char)0;
                Text[i].ForegroundColor = (uint)ForegroundColor.ToArgb();
                Text[i].BackgroundColor = (uint)BackgroundColor.ToArgb();
            }
        }

        public void Clear()
        {
            ClearText();
            mX = 0;
            mY = -1;
        }

        public void DrawCursor()
        {
            SetCursorPos(mX, mY);
        }

        /// <summary>
        /// Scroll the console up and move crusor to the start of the line.
        /// </summary>
        private void DoLineFeed()
        {
            if (Kernel.Redirect)
            {
                Kernel.CommandOutput += "\n";
            }
            else
            {
                mY++;
                mX = 0;
                if (mY == mRows)
                {
                    Scroll();
                    mY--;
                }
            }
        }

        List<Cell[]> TerminalHistory = new List<Cell[]>();
        int TerminalHistoryIndex = 0;
        public bool ScrollMode = false;

        private void Scroll()
        {
            Cell[] lineToHistory = new Cell[mCols];
            Array.Copy(Text, 0, lineToHistory, 0, mCols);
            TerminalHistory.Add(lineToHistory);

            Array.Copy(Text, mCols, Text, 0, (mRows - 1) * mCols);

            int startIndex = (mRows - 1) * mCols;
            for (int i = startIndex; i < startIndex + mCols; i++)
            {
                Text[i].Char = (char)0;
                Text[i].ForegroundColor = (uint)ForegroundColor.ToArgb();
                Text[i].BackgroundColor = (uint)BackgroundColor.ToArgb();
            }

            TerminalHistoryIndex = TerminalHistory.Count;
        }

        public void ScrollUp()
        {
            if (TerminalHistoryIndex > 0)
            {
                ScrollMode = true;

                TerminalHistoryIndex--;

                Array.Copy(Text, 0, Text, mCols, (mRows - 1) * mCols);

                Cell[] lineFromHistory = TerminalHistory[TerminalHistoryIndex];
                Array.Copy(lineFromHistory, 0, Text, 0, mCols);
            }
        }

        public void ScrollDown()
        {
            TerminalHistoryIndex = 0;

            TerminalHistory.Clear();

            ScrollMode = false;

            ClearText();
            mX = 0;
            mY = 0;
        }

        private void DoCarriageReturn()
        {
            mX = 0;
        }

        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(char aChar)
        {
            if (Kernel.Redirect)
            {
                Kernel.CommandOutput += aChar;
            }
            else
            {
                int index = GetIndex(mY, mX);
                Text[index] = new Cell() { Char = aChar, ForegroundColor = (uint)ForegroundColor.ToArgb(), BackgroundColor = (uint)BackgroundColor.ToArgb() };

                mX++;
                if (mX == mCols)
                {
                    DoLineFeed();
                }
            }
        }

        public void Write(uint aInt) => Write(aInt.ToString());

        public void Write(ulong aLong) => Write(aLong.ToString());

        public void WriteLine() => Write(Environment.NewLine);

        public void WriteLine(string aText) => Write(aText + Environment.NewLine);

        public void Write(string aText)
        {
            for (int i = 0; i < aText.Length; i++)
            {
                switch (aText[i])
                {
                    case LineFeed:
                        DoLineFeed();
                        break;

                    case CarriageReturn:
                        DoCarriageReturn();
                        break;

                    case Tab:
                        DoTab();
                        break;

                    /* Normal characters, simply write them */
                    default:
                        Write(aText[i]);
                        break;
                }
            }
        }
    }
}