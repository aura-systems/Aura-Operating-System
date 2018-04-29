/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/


using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.Shell.VBE.CosmosGLGraphics;

namespace Aura_OS.System.Shell.VESAVBE
{
    class VESAVBEConsole : Console
    {

        Graphics graphics;
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';

        public VESAVBEConsole()
        {
            Name = "VESA";
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
            get { return 110; } //141
        }

        public override int Height
        {
            get { return 48; } //48
        }

        public override int Cols
        {
            get { return 48; } //48
        }

        public override int Rows
        {
            get { return 110; } //141
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

                    case Tab:
                        DoTab();
                        break;

                    default:
                        graphics.WriteByte(ch);
                        break;
                }
            }
            Driver.VESACanvas.WriteToScreen();
        }

        private void DoTab()
        {
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
            graphics.WriteByte(Space);
        }

        public override void DrawImage(ushort X, ushort Y, ushort Length, ushort height, Image image)
        {
            graphics.DrawImage(X, Y, Length, height, image);
        }

        public override void DisableGraphicMode()
        {
            //graphics.Disable();
            //Kernel.AConsole = new VGA.VGAConsole(null);
            //Kernel.AConsole.Clear();
        }

    }
}
