/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Graphics
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.AConsole;
using Aura_OS.System.AConsole.VESAVBE;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Aura_OS.System.Graphics
{
    public class Graphics
    {
        public Canvas canvas;
        public Font font = PCScreenFont.Default;
        private static uint[] Pallete = new uint[16];

        public Graphics()
        {
            canvas = FullScreenCanvas.GetFullScreenCanvas();
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
        }

        public Pen frontpen = new Pen(Color.FromArgb((int)Pallete[VESAVBEConsole.foreground]));
        public Pen backpen = new Pen(Color.FromArgb((int)Pallete[VESAVBEConsole.background]));

        public void WriteByte(byte ch)
        {
            char c = (char)ch;
            if (c != 0 && c != '\n' && c != '\r')
            {
                canvas.DrawFilledRectangle(backpen, Kernel.AConsole.X * font.Width, Kernel.AConsole.Y * font.Height, font.Width, font.Height);
                canvas.DrawChar((char)c, font, frontpen, Kernel.AConsole.X * font.Width, Kernel.AConsole.Y * font.Height);
            }
            else if (c == '\n')
            {
                Kernel.AConsole.Y++;
                Kernel.AConsole.X = -1;

                if (Kernel.AConsole.Y == Kernel.AConsole.Cols)
                {
                    ScrollUp();
                    Kernel.AConsole.Y--;
                }
            }

            if (Kernel.AConsole.X == Kernel.AConsole.Rows - 1)
            {
                if (Kernel.AConsole.Y == Kernel.AConsole.Cols - 1)
                {
                    ScrollUp();
                }
                else
                {
                    Kernel.AConsole.Y++;
                }

                Kernel.AConsole.X = 0;
            }
            else
            {
                Kernel.AConsole.X++;
            }
        }

        public void ChangeForegroundPen(uint foreground)
        {
            frontpen = new Pen(Color.FromArgb((int)Pallete[foreground]));
        }

        public void ChangeBackgroundPen(uint background)
        {
            backpen = new Pen(Color.FromArgb((int)Pallete[background]));
        }

        public void ScrollUp()
        {
            canvas.ScrollUp();
        }
    }
}