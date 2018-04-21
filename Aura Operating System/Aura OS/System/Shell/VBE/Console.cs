/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE Console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Shell.VBE.CosmosGLGraphics;
using System;

namespace Aura_OS.System.Shell.VBE
{
    class VBEConsole : Console
    {

        Graphics graphics;
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';

        public VBEConsole()
        {
            Name = "VBE";
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
            get { return 87; }
        }

        public override int Height
        {
            get { return 37; }
        }

        public override int Cols
        {
            get { return 87; }
        }

        public override int Rows
        {
            get { return 37; }
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
            graphics.Clear(0x000000);
            mX = 0;
            mY = 0;
        }

        public override void Write(byte[] aText)
        {
            foreach (byte ch in aText)
            {
                switch (ch)
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

                    default:
                        graphics.WriteByte(ch);
                        break;
                }
            }
            graphics.Canvas.WriteToScreen();
        }

        private void DoTab()
        {
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
        }

        private void DoLineFeed()
        {
            mY++;
            mX = 0;
            if (mY == Rows)
            {
                graphics.ScrollUp();
                mY--;
            }
        }

        private void DoCarriageReturn()
        {
            mX = 0;
        }

        public override void DrawImage(ushort X, ushort Y, ushort Length, ushort height, Image image)
        {
            graphics.DrawImage(X, Y, Length, height, image);
        }

        public override void DisableGraphicMode()
        {
            graphics.Disable();
            Kernel.AConsole = new VGA.VGAConsole(null);
            Kernel.AConsole.Clear();
        }

    }
}
