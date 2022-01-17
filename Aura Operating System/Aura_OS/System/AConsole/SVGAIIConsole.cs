/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/


using System;
using System.Drawing;
using System.Text;
using Aura_OS.System.Graphics;
using Cosmos.Debug.Kernel;
using Cosmos.System.Graphics;

namespace Aura_OS.System.AConsole
{
    public class SVGAIIConsole : Console
    {
        public Color BackColor, ForeColor;
        public Font Font;

        private static uint[] Pallete = new uint[16];

        SVGAIIGraphics SVGA;

        public SVGAIIConsole()
        {
            Name = "SVGAII";
            Type = ConsoleType.Graphical;

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

            SVGA = new SVGAIIGraphics();

            mWidth = SVGA.Width;
            mHeight = SVGA.Height;

            Font = Fonts.Serif8x16;

            mCols = SVGA.Width / (Font.Width + Font.SpacingX);
            mRows = SVGA.Height / (Font.Height + Font.SpacingY);
            
            BackColor = Color.Black;
            ForeColor = Color.White;
            Clear();
        }

        protected int mX = 0;
        public override int X
        {
            get { return mX; }
            set
            {
                mX = value;
                UpdateCursor();
            }
        }


        protected int mY = 0;
        public override int Y
        {
            get { return mY; }
            set
            {
                mY = value;
                UpdateCursor();
            }
        }

        public static int mWidth;
        public override int Width
        {
            get { return mWidth; }
        }

        public static int mHeight;
        public override int Height
        {
            get { return mHeight; }
        }

        public static int mCols;
        public override int Cols
        {
            get { return mCols; }
        }

        public static int mRows;
        public override int Rows
        {
            get { return mRows; }
        }

        public static uint foreground = (byte)ConsoleColor.White;
        public override ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set
            {
                foreground = (byte)global::System.Console.ForegroundColor;
                ForeColor = Color.FromArgb((int)Pallete[foreground]);
            }
        }

        public static uint background = (byte)ConsoleColor.Black;

        public override ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set
            {
                background = (byte)global::System.Console.BackgroundColor;
                BackColor = Color.FromArgb((int)Pallete[background]);
            }
        }

        public override int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public static bool cursorvisible = false;
        public override bool CursorVisible { get => cursorvisible; set => cursorvisible = value; }

        public override void Clear()
        {
            Clear(BackColor);
        }

        public override void Clear(uint color)
        {
            Clear(Color.FromArgb((int)color));
        }

        public void Clear(Color bg)
        {
            BackColor = bg;
            SVGA.Clear(bg);
            mX = 0;
            mY = 0;
            UpdateCursor();
        }

        public override void UpdateCursor()
        {
            SVGA.SVGA.Update(0, 0, (uint)SVGA.Width, (uint)SVGA.Height);
        }

        public void Scroll()
        {
            UpdateCursor();
            int h = Font.Height + Font.SpacingY;
            SVGA.Copy(0, h, 0, 0, mCols * (Font.Width + Font.SpacingX), SVGA.Height - h);
            for (int i = 0; i < mCols; i++) { SVGA.DrawChar(i * (Font.Width + Font.SpacingX), SVGA.Height - h, ' ', ForeColor, BackColor, Font); }
            mX = 0;
            mY = mRows - 1;
            UpdateCursor();
        }

        private void DoCarriageReturn()
        {
            mX = 0;
            UpdateCursor();
        }

        public void NewLine()
        {
            UpdateCursor();
            mX = 0;
            mY++;
            if (mY >= mRows)
            {
                Scroll();
            }
            UpdateCursor();
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(char aChar)
        {
            if (aChar == 0)
                return;

            UpdateCursor();
            SVGA.DrawChar(mX * (Font.Width + Font.SpacingX), mY * (Font.Height + Font.SpacingY), aChar, ForeColor, BackColor, Font);
            mX++;
            if (mX >= mCols) { NewLine(); return; }
            UpdateCursor();
        }

        public override void Write(char[] aText)
        {
            for (int i = 0; i < aText.Length; i++)
            {
                switch (aText[i])
                {
                    case LineFeed:
                        NewLine(); //DoLineFeed();
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

        public override void Write(byte[] aText)
        {
            //throw new NotImplementedException();
        }

        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
        }

        public override void DrawImage(ushort X, ushort Y, Bitmap image)
        {
            //graphics.canvas.DrawImage(image, X, Y);
        }
    }
}
