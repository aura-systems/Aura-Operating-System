using Aura_OS.System.Shell.cmdIntr;
using Aura_OS.System.Users;
using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Drawing;
using Aura_OS.System.Graphics;

namespace Aura_OS
{
    public struct Cell
    {
        public char ?Char;
        public uint ?Colour;
    }

    public class Terminal : App
    {
        Graphics Graphics;

        internal const char LineFeed = '\n';
        internal const char CarriageReturn = '\r';
        internal const char Tab = '\t';
        internal const char Space = ' ';

        private static uint[] Pallete = new uint[16];

        Cell[] Text;
        public string Command = string.Empty;

        protected int mX = 0;
        public int X
        {
            get { return mX; }
            set
            {
                mX = value;
            }
        }


        protected int mY = 0;
        public int Y
        {
            get { return mY; }
            set
            {
                mY = value;
            }
        }

        public static int mWidth;
        public int Width
        {
            get { return mWidth; }
        }

        public static int mHeight;
        public int Height
        {
            get { return mHeight; }
        }

        public static int mCols;
        public int Cols
        {
            get { return mCols; }
        }

        public static int mRows;
        public int Rows
        {
            get { return mRows; }
        }

        public Pen ForegroundPen = new Pen(Color.White);
        public static uint foreground = (byte)ConsoleColor.White;
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set
            {
                foreground = (uint)value;
                ForegroundPen = new Pen(Color.FromArgb((int)Pallete[foreground]));
            }
        }

        public Pen BackgroundPen = new Pen(Color.Black);
        public static uint background = (byte)ConsoleColor.Black;
        public ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set
            {
                background = (uint)value;
                BackgroundPen = new Pen(Color.FromArgb((int)Pallete[background]));
            }
        }

        public int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CursorVisible;

        public Terminal(uint width, uint height, uint x = 0, uint y = 0) : base("Terminal", width, height, x, y)
        {
            Graphics = new Graphics();

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

            mWidth = (int)width;
            mHeight = (int)height;

            mCols = mWidth / Kernel.font.Width - 1;
            mRows = mHeight / Kernel.font.Height - 2;

            Text = new Cell[mCols * mRows];

            CursorVisible = true;

            mX = 0;
            mY = 0;

            visible = true;

            BeforeCommand();
        }

        public override void UpdateApp()
        {
            KeyEvent keyEvent = null;

            if (KeyboardManager.TryReadKey(out keyEvent))
            {
                switch (keyEvent.Key)
                {
                    case ConsoleKeyEx.Enter:
                        Kernel.CommandManager.Execute(Command);

                        Command = string.Empty;

                        BeforeCommand();
                        break;
                    case ConsoleKeyEx.Backspace:
                        if (Command.Length > 0)
                        {
                            Command = Command.Remove(Command.Length - 1);
                        }
                        break;
                    default:
                        if ((char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || (keyEvent.KeyChar == ' ')))
                        {
                            Command += keyEvent.KeyChar;
                        }
                        break;
                }
            }

            Kernel.canvas.DrawFilledRectangle(Kernel.BlackPen, (int)x, (int)y, (int)width, (int)height);

            DrawTerminal();

            DrawCursor();
        }

        void DrawTerminal()
        {
            int DrawX = 0;
            int DrawY = 0;

            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i].Char == null)
                    continue;

                Graphics.WriteByte((char)Text[i].Char, (int)Kernel.console.x + DrawX * Kernel.font.Width, (int)Kernel.console.y + DrawY * Kernel.font.Height);

                DrawX++;
                if (DrawX == mCols)
                {
                    DrawY++;
                    DrawX = 0;
                    if (DrawY == mRows)
                    {
                        DrawY--;
                    }
                }
            }
        }

        public void Clear()
        {
            Text = new Cell[mRows * mCols];
            mX = 0;
            mY = 0;
        }

        public void DrawCursor()
        {
            Graphics.SetCursorPos(mX, mY);
        }

        /// <summary>
        /// Scroll the console up and move crusor to the start of the line.
        /// </summary>
        private void DoLineFeed()
        {
            mY++;
            mX = 0;
            if (mY == mRows)
            {
                mY--;
            }
        }

        private void DoCarriageReturn()
        {
            mX = 0;
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(char aChar)
        {
            if (aChar == '\0')
                return;
            Text[X * mCols + Y] = new Cell() { Char = aChar, Colour = Pallete[foreground] };
            mX++;
            if (mX == mCols)
            {
                DoLineFeed();
            }
        }

        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
        }

        public void DrawImage(ushort X, ushort Y, Bitmap image)
        {
            //graphics.canvas.DrawImage(image, X, Y);
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

        #region BeforeCommand

        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        public void BeforeCommand()
        {
            Foreground = ConsoleColor.Blue;
            Write(UserLevel.TypeUser);

            Foreground = ConsoleColor.Yellow;
            Write(Kernel.userLogged);

            Foreground = ConsoleColor.DarkGray;
            Write("@");

            Foreground = ConsoleColor.Blue;
            Write(Kernel.ComputerName);

            Foreground = ConsoleColor.Gray;
            Write("> ");

            Foreground = ConsoleColor.White;
        }

        #endregion
    }
}
