/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Graphical terminal application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Users;
using Cosmos.System;
using System;
using System.Drawing;
using Aura_OS.System.UI.GUI;
using System.Collections.Generic;

namespace Aura_OS
{
    public struct Cell
    {
        public char Char;
        public Color ForegroundColor;
        public Color BackgroundColor;
    }

    public class Terminal : App
    {
        GUI Graphics;

        internal const char LineFeed = '\n';
        internal const char CarriageReturn = '\r';
        internal const char Tab = '\t';
        internal const char Space = ' ';

        private static uint[] Pallete = new uint[16];

        Cell[][] Text;

        List<string> Commands = new List<string>();
        private int CommandIndex = 0;
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

        public Color ForegroundColor = Color.White;
        public static uint foreground = (byte)ConsoleColor.White;
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set
            {
                foreground = (uint)value;

                uint color = Pallete[foreground];
                byte r = (byte)((color >> 16) & 0xFF); // Extract the red component
                byte g = (byte)((color >> 8) & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                ForegroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public Color BackgroundColor = Color.Black;
        public static uint background = (byte)ConsoleColor.Black;
        public ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set
            {
                background = (uint)value;

                uint color = Pallete[background];
                byte r = (byte)((color >> 16) & 0xFF); // Extract the red component
                byte g = (byte)((color >> 8) & 0xFF); // Extract the green component
                byte b = (byte)(color & 0xFF); // Extract the blue component

                BackgroundColor = Color.FromArgb(0xFF, r, g, b);
            }
        }

        public int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CursorVisible;

        public Terminal(int width, int height, int x = 0, int y = 0) : base("Terminal", width, height, x, y)
        {
            Window.Icon = Kernel.terminalIco;

            Graphics = new GUI();

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

            Text = new Cell[mRows][];
            for (int i = 0; i < mRows; i++)
            {
                Text[i] = new Cell[mCols];

                for (int j = 0; j < mRows; j++)
                {
                    Text[i][j] = new Cell();
                }
            }

            ClearText();

            CursorVisible = true;

            mX = 0;
            mY = 0;

            Command = string.Empty;

            BeforeCommand();
        }

        public override void UpdateApp()
        {
            if (Kernel.WindowManager.Focused.Equals(this))
            {
                KeyEvent keyEvent = null;

                if (KeyboardManager.TryReadKey(out keyEvent))
                {
                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Enter:
                            if (ScrollMode)
                            {
                                break;
                            }
                            if (Command.Length > 0)
                            {
                                mX -= Command.Length;

                                WriteLine(Command);

                                Kernel.CommandManager.Execute(Command);

                                Commands.Add(Command);
                                CommandIndex = Commands.Count - 1;

                                Command = string.Empty;
                            }
                            else
                            {
                                WriteLine();
                                WriteLine();
                            }

                            BeforeCommand();
                            break;
                        case ConsoleKeyEx.Backspace:
                            if (ScrollMode)
                            {
                                break;
                            }
                            if (Command.Length > 0)
                            {
                                Command = Command.Remove(Command.Length - 1);
                                mX--;
                            }
                            break;
                        case ConsoleKeyEx.UpArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                ScrollUp();
                            }
                            else
                            {
                                if (CommandIndex >= 0)
                                {
                                    mX -= Command.Length;
                                    Command = Commands[CommandIndex];
                                    CommandIndex--;
                                    mX += Command.Length;
                                }
                            }
                            break;
                        case ConsoleKeyEx.DownArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                ScrollDown();
                            }
                            else
                            {
                                if (CommandIndex < Commands.Count - 1)
                                {
                                    mX -= Command.Length;
                                    CommandIndex++;
                                    Command = Commands[CommandIndex];
                                    mX += Command.Length;
                                }
                            }
                            break;
                        default:
                            if (ScrollMode)
                            {
                                break;
                            }
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || (keyEvent.KeyChar == ' '))
                            {
                                Command += keyEvent.KeyChar;
                                mX++;
                            }
                            break;
                    }
                }
            }

            Kernel.canvas.DrawFilledRectangle(Kernel.BlackColor, x, y, width, height);

            DrawTerminal();

            if (!ScrollMode)
            {
                DrawCursor();
            }
        }

        void DrawTerminal()
        {
            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    if (Text[i][j].Char == 0 || Text[i][j].Char == '\n')
                        continue;

                    Graphics.WriteByte((char)Text[i][j].Char, Kernel.console.x + j * Kernel.font.Width, Kernel.console.y + i * Kernel.font.Height, Text[i][j].ForegroundColor);
                }
            }

            if (Command.Length > 0)
            {
                int baseX = mX - Command.Length;

                for (int i = 0; i < Command.Length; i++)
                {
                    Graphics.WriteByte(Command[i], Kernel.console.x + ((baseX + i) * Kernel.font.Width), Kernel.console.y + mY * Kernel.font.Height, ForegroundColor);
                }
            }
        }

        private void ClearText()
        {
            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    Text[i][j].Char = (char)0;
                }
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
                Scroll();
                mY--;
            }
        }

        List<Cell[]> TerminalHistory = new List<Cell[]>();
        int TerminalHistoryIndex = 0;
        bool ScrollMode = false;

        private void Scroll()
        {
            TerminalHistory.Add(Text[0]);
            TerminalHistoryIndex++;

            for (int i = 0; i < mRows - 1; i++)
            {
                Text[i] = Text[i + 1];
            }

            Text[mRows - 1] = new Cell[mCols];
        }

        private void ScrollUp()
        {
            if (TerminalHistoryIndex > 0)
            {
                ScrollMode = true;

                for (int i = Rows - 1; i > 0; i--)
                {
                    Text[i] = Text[i - 1];
                }

                TerminalHistoryIndex--;

                Text[0] = TerminalHistory[TerminalHistoryIndex];
            }
        }

        private void ScrollDown()
        {
            TerminalHistoryIndex = 0;

            TerminalHistory.Clear();

            ScrollMode = false;

            ClearText();
            mX = 0;
            mY = 0;
            BeforeCommand();
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
            Text[mY][mX] = new Cell() { Char = aChar, ForegroundColor = ForegroundColor, BackgroundColor = BackgroundColor };
            mX++;
            if (mX == mCols)
            {
                DoLineFeed();
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

            Foreground = ConsoleColor.DarkGray;
            Write(Kernel.CurrentDirectory + "~ ");

            Foreground = ConsoleColor.White;
        }

        public string GetConsoleInfo()
        {
            return Kernel.canvas.Name() + " (" + Kernel.console.Cols + "x" + Kernel.console.Rows + " - " + global::System.Console.OutputEncoding.BodyName + ")";
        }

        #endregion
    }
}
