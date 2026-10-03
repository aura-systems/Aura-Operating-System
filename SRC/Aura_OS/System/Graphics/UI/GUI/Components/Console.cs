/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Console class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI.Skin;
using Cosmos.Kernel.System.Mouse;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public struct Cell
    {
        public char Char;
        public uint ForegroundColor;
        public uint BackgroundColor;
    }

    /// <summary>
    /// A text console: a grid of colored characters, with a cursor and a line being typed when
    /// CursorVisible (a terminal), without (the boot console). The lines that scroll off the top
    /// are kept, and can be scrolled back to (ScrollUp, or the ScrollBar).
    /// </summary>
    public class Console : Component
    {
        private const char LineFeed = '\n';
        private const char CarriageReturn = '\r';
        private const char Tab = '\t';
        private const char Space = ' ';

        // The scroll bar, the size of the multiline TextBox's.
        private const int ScrollBarSize = 15;
        private const int MinThumbSize = 16;

        /// <summary>
        /// Lines kept above the screen; past it the oldest one goes.
        /// </summary>
        private const int MaxHistoryLines = 1000;

        public bool DrawBackground = true;

        /// <summary>
        /// Hides the line being typed and the cursor (while a terminal runs a command).
        /// </summary>
        public bool InputHidden = false;

        /// <summary>
        /// Draws the cursor under the character at mX, mY (not in scroll mode).
        /// </summary>
        public bool CursorVisible;
        public int mX = 0;
        public int mY = 0;

        public int mCols;
        public int mRows;

        private uint[] _pallete = new uint[16];
        private Cell[] _text;
        private List<Cell[]> _terminalHistory;
        private string _input = "";

        /// <summary>
        /// Lines the view is scrolled back from the screen being written (0: not scrolled).
        /// </summary>
        private int _viewOffset = 0;

        private bool _scrollBar = false;
        private Frame _rail;
        private Frame _thumb;
        private Frame _thumbHighlighted;
        private Frame _thumbPressed;

        // The thumb's state when last drawn: the mouse coming over it or leaving it redraws.
        private Frame _drawnThumb;

        private bool _dragging = false;

        // Where the thumb was taken, from its top.
        private int _scrollGrab;

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

            InitConsole(width, height);
        }

        /// <summary>
        /// Text typed before the cursor and not written yet (a terminal's command line): drawn
        /// from mX - Input.Length. Setting it moves the cursor along, so mX stays after it. Not in
        /// scroll mode.
        /// </summary>
        public string Input
        {
            get
            {
                return _input;
            }
            set
            {
                value = value ?? "";
                mX += value.Length - _input.Length;
                _input = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// A vertical scroll bar on the right, shown once lines scrolled off the top: dragging its
        /// thumb scrolls back through them. Its width is kept for it from the start, so the columns
        /// stay the same when it appears.
        /// </summary>
        public bool ScrollBar
        {
            get
            {
                return _scrollBar;
            }
            set
            {
                if (value == _scrollBar)
                {
                    return;
                }

                // Not in the constructor: the boot console exists before the theme does.
                if (value && _rail == null)
                {
                    _rail = Kernel.ThemeManager.GetFrame("rail.vertical");
                    _thumb = Kernel.ThemeManager.GetFrame("slider.vertical.normal");
                    _thumbHighlighted = Kernel.ThemeManager.GetFrame("slider.vertical.highlighted");
                    _thumbPressed = Kernel.ThemeManager.GetFrame("slider.vertical.depressed");
                }

                _scrollBar = value;
                InitConsole(Width, Height);
            }
        }

        public void InitConsole(int width, int height)
        {
            int textWidth = _scrollBar ? width - ScrollBarSize : width;

            // At least one cell: a negative count would throw out of the allocation.
            mCols = Math.Max(1, textWidth / Kernel.font.Width - 1);
            mRows = Math.Max(1, height / Kernel.font.Height - 2);

            _text = new Cell[mCols * mRows];

            // Kept lines are mCols long: they go with the old size.
            _terminalHistory = new List<Cell[]>();

            ClearText();
        }

        private int GetIndex(int row, int col)
        {
            return row * mCols + col;
        }

        /// <summary>
        /// A new size starts the console over: empty, with as many columns and rows as fit.
        /// </summary>
        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            InitConsole(width, height);
        }

        #region Scroll bar geometry (console coordinates)

        private bool HasScrollBar => _scrollBar && _terminalHistory.Count > 0;

        private int ScrollBarX => Width - ScrollBarSize;

        private int ThumbHeight => Math.Min(Height, Math.Max(MinThumbSize, Height * mRows / (_terminalHistory.Count + mRows)));

        /// <summary>
        /// The kept line at the top of the view.
        /// </summary>
        private int TopLine => _terminalHistory.Count - _viewOffset;

        private int ThumbY => _terminalHistory.Count > 0 ? (Height - ThumbHeight) * TopLine / _terminalHistory.Count : 0;

        #endregion

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;

            // On the rail: drag the thumb, brought under the mouse when the press is beside it.
            if (HasScrollBar && x >= ScrollBarX)
            {
                bool onThumb = y >= ThumbY && y < ThumbY + ThumbHeight;
                _scrollGrab = onThumb ? y - ThumbY : ThumbHeight / 2;
                _dragging = true;
                ScrollToThumb(y - _scrollGrab);
                MarkDirty();
            }
        }

        /// <summary>
        /// Drags the thumb while the button pressed on the rail is down, and highlights the thumb
        /// under the mouse. Nothing else in the console changes with the mouse.
        /// </summary>
        public override void Update()
        {
            if (!HasScrollBar || !Kernel.MouseManager.IsLeftButtonDown)
            {
                _dragging = false;
            }

            if (!HasScrollBar)
            {
                return;
            }

            if (_dragging)
            {
                ScrollToThumb((int)MouseManager.Y - AbsoluteY - _scrollGrab);
            }

            if (GetThumbFrame() != _drawnThumb)
            {
                MarkDirty();
            }
        }

        /// <summary>
        /// Scrolls so the thumb starts at that y (console coordinates).
        /// </summary>
        private void ScrollToThumb(int thumbY)
        {
            int lines = _terminalHistory.Count;
            int range = Height - ThumbHeight;
            int topLine = range > 0 ? (thumbY * lines + range / 2) / range : lines;
            topLine = Math.Max(0, Math.Min(topLine, lines));

            SetViewOffset(lines - topLine);
        }

        private Frame GetThumbFrame()
        {
            if (_dragging)
            {
                return _thumbPressed;
            }

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;
            bool over = x >= ScrollBarX && x < Width && y >= ThumbY && y < ThumbY + ThumbHeight;

            return over ? _thumbHighlighted : _thumb;
        }

        public override void Draw()
        {
            if (DrawBackground)
            {
                Clear(Kernel.BlackColor);
            }

            int kept = _terminalHistory.Count;
            int topLine = TopLine;

            for (int i = 0; i < mRows; i++)
            {
                // Scrolled back, the first rows are kept lines and the screen starts lower.
                int line = topLine + i;
                Cell[] cells = line < kept ? _terminalHistory[line] : _text;
                int start = line < kept ? 0 : GetIndex(line - kept, 0);

                for (int j = 0; j < mCols; j++)
                {
                    Cell cell = cells[start + j];
                    if (cell.Char == 0 || cell.Char == '\n')
                        continue;

                    WriteByte(cell.Char, 0 + j * Kernel.font.Width, 0 + i * Kernel.font.Height, cell.ForegroundColor);
                }
            }

            // The line being typed moved down with the screen, maybe out of the view.
            int inputRow = mY + _viewOffset;

            if (!InputHidden && inputRow < mRows)
            {
                int inputX = mX - _input.Length;

                for (int i = 0; i < _input.Length; i++)
                {
                    WriteByte(_input[i], (inputX + i) * Kernel.font.Width, inputRow * Kernel.font.Height, (uint)ForegroundColor.ToArgb());
                }

                SetCursorPos(mX, inputRow);
            }

            // Last: over a line being typed past the columns.
            if (HasScrollBar)
            {
                _drawnThumb = GetThumbFrame();

                DrawFrame(_rail, ScrollBarX, 0, ScrollBarSize, Height);
                DrawFrame(_drawnThumb, ScrollBarX, ThumbY, ScrollBarSize, ThumbHeight);
            }
        }

        public void WriteByte(char ch, int mX, int mY, uint color)
        {
            DrawChar(ch, Kernel.font, (int)color, mX, mY);
        }

        public void SetCursorPos(int mX, int mY)
        {
            if (CursorVisible)
            {
                DrawFilledRectangle(ForegroundColor, 0 + mX * Kernel.font.Width,
                    0 + mY * Kernel.font.Height + Kernel.font.Height, 8, 4);
            }
        }

        /// <summary>
        /// Empties the screen and the kept lines, and the line being typed, which was on it.
        /// </summary>
        public void ClearText()
        {
            Clear(Color.Black);
            mX = 0;
            mY = 0;
            _input = "";

            _terminalHistory.Clear();
            _viewOffset = 0;
            _dragging = false;

            for (int i = 0; i < _text.Length; i++)
            {
                _text[i].Char = (char)0;
                _text[i].ForegroundColor = (uint)ForegroundColor.ToArgb();
                _text[i].BackgroundColor = (uint)BackgroundColor.ToArgb();
            }

            MarkDirty();
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
            Clear(Color.Black);

            // Full: the oldest line goes, and its cells take the new one.
            Cell[] lineToHistory;
            if (_terminalHistory.Count >= MaxHistoryLines)
            {
                lineToHistory = _terminalHistory[0];
                _terminalHistory.RemoveAt(0);
            }
            else
            {
                lineToHistory = new Cell[mCols];
            }

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

            // Scrolled back, the view stays on the same lines.
            if (_viewOffset > 0)
            {
                _viewOffset = Math.Min(_viewOffset + 1, _terminalHistory.Count);
            }
        }

        /// <summary>
        /// Shows one more kept line at the top.
        /// </summary>
        public void ScrollUp()
        {
            SetViewOffset(_viewOffset + 1);
        }

        /// <summary>
        /// Shows one more line at the bottom, up to the screen being written.
        /// </summary>
        public void ScrollDown()
        {
            SetViewOffset(_viewOffset - 1);
        }

        /// <summary>
        /// Back to the screen being written.
        /// </summary>
        public void ScrollToEnd()
        {
            SetViewOffset(0);
        }

        private void SetViewOffset(int offset)
        {
            offset = Math.Max(0, Math.Min(offset, _terminalHistory.Count));

            if (offset != _viewOffset)
            {
                _viewOffset = offset;
                MarkDirty();
            }
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