/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Graphics
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Aura_OS.System.Graphics
{
    public class Graphics
    {
        public PCScreenFont font;

        private static uint[] Pallete = new uint[16];

        public Graphics()
        {           
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

            frontpen = new Pen(Color.FromArgb((int)Pallete[Terminal.foreground]));
            backpen = new Pen(Color.FromArgb((int)Pallete[Terminal.background]));

            font = Kernel.font;

            lastpen = backpen;
            lastx = 0;
            lasty = font.Height;
        }

        public Pen frontpen;
        public Pen backpen;

        public void DrawFilledRectangle(Pen pen, int x_start, int y_start, int width, int height)
        {
            if (height == -1)
            {
                height = width;
            }

            for (int y = y_start; y < y_start + height; y++)
            {
                Kernel.canvas.DrawLine(pen, x_start, y, x_start + width + 1, y);
            }
        }

        public void WriteByte(char ch)
        {
            if (ch > 0x7E) //max font
            {
                for (int i = 0; i < font._UnicodeMappings.Count; i++)
                {
                    for (int j = 0; j < font._UnicodeMappings[i].UnicodeCharacters.Count; i++)
                    {
                        if (font._UnicodeMappings[i].UnicodeCharacters[j] == ch)
                        {
                            Kernel.canvas.DrawChar((char)font._UnicodeMappings[i].FontPosition, font, frontpen, (int)Kernel.console.x + Kernel.console.X * font.Width, (int)Kernel.console.y + Kernel.console.Y * font.Height);
                        }
                    }
                }
            }
            else
            {
                Kernel.canvas.DrawChar((char)ch, font, frontpen, (int)Kernel.console.x + Kernel.console.X * font.Width, (int)Kernel.console.y + Kernel.console.Y * font.Height);
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

        public Pen lastpen;
        public int lastx;
        public int lasty;

        public void SetCursorPos(int mX, int mY)
        {
            if (Kernel.console.CursorVisible)
            {
                DrawFilledRectangle(frontpen, (int)Kernel.console.x + mX * font.Width, (int)Kernel.console.y + mY * font.Height + font.Height, 8, 4);

                lastx = mX * font.Width;
                lasty = mY * font.Height + font.Height;
                lastpen = backpen;
            }
        }
    }
}