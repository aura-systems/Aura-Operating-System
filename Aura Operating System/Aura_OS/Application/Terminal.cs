using Aura_OS.System.Shell.cmdIntr;
using Aura_OS.System.Users;
using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Drawing;
using Aura_OS.System.Graphics;

namespace Aura_OS
{
    public class Terminal : App
    {
        Graphics Graphics;

        public string text = string.Empty;
        public string Command = string.Empty;

        internal const char LineFeed = '\n';
        internal const char CarriageReturn = '\r';
        internal const char Tab = '\t';
        internal const char Space = ' ';

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
        public int Rows { get; }

        public static uint foreground = (byte)ConsoleColor.White;
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set
            {
                foreground = (byte)global::System.Console.ForegroundColor;
                Graphics.ChangeForegroundPen(foreground);
            }
        }

        public static uint background = (byte)ConsoleColor.Black;
        public ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set
            {
                background = (byte)global::System.Console.BackgroundColor;
                Graphics.ChangeBackgroundPen(background);
            }
        }

        public int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CursorVisible;

        public Terminal(uint width, uint height, uint x = 0, uint y = 0) : base("Terminal", width, height, x, y)
        {
            Graphics = new Graphics();

            mWidth = (int)width;
            mHeight = (int)height;

            mCols = mWidth / Kernel.font.Width - 1;
            mRows = mHeight / Kernel.font.Height - 2;

            CursorVisible = true;

            mX = 0;
            mY = 0;

            BeforeCommand();
        }

        public override void _Update()
        {
            KeyEvent keyEvent = null;

            if (KeyboardManager.TryReadKey(out keyEvent))
            {
                switch (keyEvent.Key)
                {
                    case ConsoleKeyEx.Enter:
                        text += '\n';

                        Kernel.CommandManager.Execute(Command);
                        Command = string.Empty;

                        BeforeCommand();
                        break;
                    case ConsoleKeyEx.Backspace:
                        if (Command.Length > 0)
                        {
                            text = text.Remove(text.Length - 1);
                            Command = Command.Remove(Command.Length - 1);
                        }
                        break;
                    default:
                        if ((char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || (keyEvent.KeyChar == ' ')))
                        {
                            text += keyEvent.KeyChar;
                            Command += keyEvent.KeyChar;
                        }
                        break;
                }
            }

            Kernel.canvas.DrawFilledRectangle(Kernel.BlackPen, (int)x, (int)y, (int)width, (int)height);

            mX = 0;
            mY = 0;

            WriteDisplay(text);

            DrawCursor();
        }

        public void Clear()
        {
            mX = 0;
            mY = 0;
            text = string.Empty;
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
                text = text.Substring(text.IndexOf('\n') + 1);
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
        public void WriteDisplay(char aChar)
        {
            if (aChar == '\0')
                return;
            Graphics.WriteByte(aChar);
            mX++;
            if (mX == mCols)
            {
                DoLineFeed();
            }
        }

        public void WriteDisplay(string aText)
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
                        WriteDisplay(aText[i]);
                        break;
                }
            }
        }

        private void DoTab()
        {
            WriteDisplay(Space);
            WriteDisplay(Space);
            WriteDisplay(Space);
            WriteDisplay(Space);
        }

        public void DrawImage(ushort X, ushort Y, Bitmap image)
        {
            //graphics.canvas.DrawImage(image, X, Y);
        }

        public void Write(char aChar)
        {
            text += aChar;
        }

        public void Write(uint aInt) => Write(aInt.ToString());

        public void Write(ulong aLong) => Write(aLong.ToString());

        public void WriteLine() => Write(Environment.NewLine);

        public void WriteLine(string aText) => Write(aText + Environment.NewLine);

        public void Write(string aText)
        {
            for (int i = 0; i < aText.Length; i++)
            {
                text += aText[i];
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
            Write("root");

            Foreground = ConsoleColor.DarkGray;
            Write("@");

            Foreground = ConsoleColor.Blue;
            Write("aura-pc");

            Foreground = ConsoleColor.Gray;
            Write("> ");

            Foreground = ConsoleColor.White;
        }

        #endregion
    }
}
