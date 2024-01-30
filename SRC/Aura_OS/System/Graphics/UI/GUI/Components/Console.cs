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
    public class Console : Component
    {
        public struct Cell
        {
            public char Char;
            public uint ForegroundColor;
            public uint BackgroundColor;
        }

        private const char LineFeed = '\n';
        private const char CarriageReturn = '\r';
        private const char Tab = '\t';
        private const char Space = ' ';

        public bool DrawBackground = true;
        public bool ScrollMode = false;
        public bool CursorVisible;
        public int mX = 0;
        public int mY = 0;

        public int mCols;
        public int mRows;

        private uint[] _pallete = new uint[16];
        private Cell[] _text;
        private List<Cell[]> _terminalHistory;
        private int _terminalHistoryIndex = 0;
        
        public Color ForegroundColor = Color.White;
        private uint _foreground = (byte)ConsoleColor.White;
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)_foreground; }
            set
            {
                _foreground = (uint)value;

                uint color = _pallete[_foreground];
                byte r = (byte)(color >> 16 & 0xFF); // Extract the red component
                byte g = (byte)(color >> 8 & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                ForegroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public Color BackgroundColor = Color.Black;
        private uint _background = (byte)ConsoleColor.Black;
        public ConsoleColor Background
        {
            get { return (ConsoleColor)_background; }
            set
            {
                _background = (uint)value;

                uint color = _pallete[_background];
                byte r = (byte)(color >> 16 & 0xFF); // Extract the red component
                byte g = (byte)(color >> 8 & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                BackgroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public Console(int x, int y, int width, int height) : base(x, y, width, height)
        {
            _pallete[0] = 0xFF000000; // Black
            _pallete[1] = 0xFF0000AB; // Darkblue
            _pallete[2] = 0xFF008000; // DarkGreen
            _pallete[3] = 0xFF008080; // DarkCyan
            _pallete[4] = 0xFF800000; // DarkRed
            _pallete[5] = 0xFF800080; // DarkMagenta
            _pallete[6] = 0xFF808000; // DarkYellow
            _pallete[7] = 0xFFC0C0C0; // Gray
            _pallete[8] = 0xFF808080; // DarkGray
            _pallete[9] = 0xFF5353FF; // Blue
            _pallete[10] = 0xFF55FF55; // Green
            _pallete[11] = 0xFF00FFFF; // Cyan
            _pallete[12] = 0xFFAA0000; // Red
            _pallete[13] = 0xFFFF00FF; // Magenta
            _pallete[14] = 0xFFFFFF55; // Yellow
            _pallete[15] = 0xFFFFFFFF; //White

            mCols = width / Kernel.font.Width - 1;
            mRows = height / Kernel.font.Height - 2;

            _text = new Cell[mCols * mRows];

            ClearText();

            CursorVisible = true;
            _terminalHistory = new List<Cell[]>();

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
                    if (_text[index].Char == 0 || _text[index].Char == '\n')
                        continue;

                    WriteByte(_text[index].Char, X + j * Kernel.font.Width, Y + i * Kernel.font.Height, _text[index].ForegroundColor);
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
            for (int i = 0; i < _text.Length; i++)
            {
                _text[i].Char = (char)0;
                _text[i].ForegroundColor = (uint)ForegroundColor.ToArgb();
                _text[i].BackgroundColor = (uint)BackgroundColor.ToArgb();
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

        private void Scroll()
        {
            Cell[] lineToHistory = new Cell[mCols];
            Array.Copy(_text, 0, lineToHistory, 0, mCols);
            _terminalHistory.Add(lineToHistory);

            Array.Copy(_text, mCols, _text, 0, (mRows - 1) * mCols);

            int startIndex = (mRows - 1) * mCols;
            for (int i = startIndex; i < startIndex + mCols; i++)
            {
                _text[i].Char = (char)0;
                _text[i].ForegroundColor = (uint)ForegroundColor.ToArgb();
                _text[i].BackgroundColor = (uint)BackgroundColor.ToArgb();
            }

            _terminalHistoryIndex = _terminalHistory.Count;
        }

        public void ScrollUp()
        {
            if (_terminalHistoryIndex > 0)
            {
                ScrollMode = true;

                _terminalHistoryIndex--;

                Array.Copy(_text, 0, _text, mCols, (mRows - 1) * mCols);

                Cell[] lineFromHistory = _terminalHistory[_terminalHistoryIndex];
                Array.Copy(lineFromHistory, 0, _text, 0, mCols);
            }
        }

        public void ScrollDown()
        {
            _terminalHistoryIndex = 0;

            _terminalHistory.Clear();

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
                _text[index] = new Cell() { Char = aChar, ForegroundColor = (uint)ForegroundColor.ToArgb(), BackgroundColor = (uint)BackgroundColor.ToArgb() };

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