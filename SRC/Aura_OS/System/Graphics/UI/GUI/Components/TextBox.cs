/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class TextBox : Component
    {
        public struct Cell
        {
            public char Char;
            public uint ForegroundColor;
        }

        private string __text;
        public string Text
        {
            get { return __text; }
            set
            {
                __text = value;
                InitializeText(__text);
            }
        }

        public Action Enter;
        public bool Multiline = false;

        private bool _isSelected = false;
        private bool _cursorVisible = true;
        private DateTime _lastCursorBlink = DateTime.Now;
        private const int _cursorBlinkInterval = 400;

        private Cell[] _text;
        public int mCols;
        public int mRows;

        private int _cursorPositionX = 0;
        private int _cursorPositionY = 0;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            Text = text;
            mCols = width / Kernel.font.Width - 1;
            mRows = height / Kernel.font.Height - 2;

            _text = new Cell[mCols * mRows];
            for (int i = 0; i < _text.Length; i++)
            {
                _text[i] = new Cell { Char = ' ', ForegroundColor = (uint)Kernel.BlackColor.ToArgb() };
            }

            InitializeText(text);
        }

        public override void HandleLeftClick()
        {
            if (IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                _isSelected = true;
                Kernel.MouseManager.FocusedComponent = this;
            }
            else
            {
                _isSelected = false;
            }
        }

        public override void Update()
        {
            if (_isSelected)
            {
                KeyEvent keyEvent = null;

                while (KeyboardManager.TryReadKey(out keyEvent))
                {
                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Backspace:
                            RemoveCharacter();
                            break;
                        case ConsoleKeyEx.Enter:
                            InsertCharacter('\n');
                            break;
                        default:
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
                            {
                                InsertCharacter(keyEvent.KeyChar);
                            }
                            break;
                    }
                }
            }
        }

        public override void Draw()
        {
            // Dessin du cadre de la TextBox
            Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, X + 2, Y + 2, Width - 3, Height - 3);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);

            for (int y = 0; y < mRows; y++)
            {
                for (int x = 0; x < mCols; x++)
                {
                    Cell cell = _text[y * mCols + x];
                    Kernel.canvas.DrawString(cell.Char.ToString(), Kernel.font, Color.FromArgb((int)cell.ForegroundColor), X + 4 + x * Kernel.font.Width, Y + 4 + y * Kernel.font.Height);
                }
            }

            if (_isSelected && _cursorVisible)
            {
                int cursorX = X + 4 + _cursorPositionX * Kernel.font.Width;
                int cursorY = Y + 4 + _cursorPositionY * Kernel.font.Height;
                Kernel.canvas.DrawFilledRectangle(Kernel.BlackColor, cursorX, cursorY, 2, Kernel.font.Height);
            }
        }

        private void InitializeText(string text)
        {
            string[] lines = text.Split('\n');
            int currentRow = 0;

            foreach (var line in lines)
            {
                int currentCol = 0;
                foreach (var ch in line)
                {
                    if (currentRow < mRows && currentCol < mCols)
                    {
                        _text[currentRow * mCols + currentCol] = new Cell { Char = ch, ForegroundColor = (uint)Kernel.BlackColor.ToArgb() };
                    }
                    currentCol++;
                    if (currentCol >= mCols) break;
                }
                currentRow++;
                if (currentRow >= mRows) break;
            }
        }

        private void InsertCharacter(char character)
        {
            int cursorIndex = _cursorPositionY * mCols + _cursorPositionX;

            for (int i = _text.Length - 2; i >= cursorIndex; i--)
            {
                _text[i + 1] = _text[i];
            }

            _text[cursorIndex] = new Cell { Char = character, ForegroundColor = (uint)Kernel.BlackColor.ToArgb() };

            _cursorPositionX++;
            if (_cursorPositionX >= mCols)
            {
                _cursorPositionX = 0;
                _cursorPositionY++;
            }
        }

        private void RemoveCharacter()
        {
            int cursorIndex = _cursorPositionY * mCols + _cursorPositionX;

            if (cursorIndex == 0) return;

            for (int i = cursorIndex; i < _text.Length - 1; i++)
            {
                _text[i - 1] = _text[i];
            }

            _text[_text.Length - 1] = new Cell { Char = ' ', ForegroundColor = (uint)Kernel.BlackColor.ToArgb() };

            _cursorPositionX--;
            if (_cursorPositionX < 0)
            {
                _cursorPositionX = mCols - 1;
                _cursorPositionY--;
            }
        }
    }
}