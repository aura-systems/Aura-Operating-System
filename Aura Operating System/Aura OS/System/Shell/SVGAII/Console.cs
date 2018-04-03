/*
* PROJECT:          Aura Operating System Development
* CONTENT:          SVGAII Console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Shell.SVGAII
{
    class VMWareSVGAConsole : Console
    {

        Graphics graphics;

        public VMWareSVGAConsole()
        {
            Name = "VMWare SVGAII";
            graphics = new Graphics();
        }

        protected int mX = 0;
        public override int X
        {
            get { return mX; }
            set
            {
                mX = value;
            }
        }


        protected int mY = 0;
        public override int Y
        {
            get { return mY; }
            set
            {
                mY = value;
            }
        }

        public override int Width
        {
            get { return 80; }
        }

        public override int Height
        {
            get { return 25; }
        }

        public override int Cols
        {
            get { return 80; }
        }

        public override int Rows
        {
            get { return 25; }
        }

        public static uint foreground = (byte)ConsoleColor.White;

        public override ConsoleColor Foreground
        {
            get { return (ConsoleColor)foreground; }
            set { foreground = (byte)global::System.Console.ForegroundColor; }
        }

        public static uint background = (byte)ConsoleColor.Black;

        public override ConsoleColor Background
        {
            get { return (ConsoleColor)background; }
            set { background = (byte)global::System.Console.BackgroundColor; }
        }

        public override int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool CursorVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Clear()
        {
            graphics.Clear();
            mX = 0;
            mY = 0;
        }

        public override void Write(byte[] aText)
        {
            foreach (byte ch in aText)
            {
                graphics.WriteByte(ch);
            }
            graphics.Update(0, 0, 800, 600);
        }

        public void DrawImage(ushort X, ushort Y, ushort Length, ushort height, uint[] data)
        {
            graphics.DrawImage(X, Y, Length, height, data);
        }

    }
}
