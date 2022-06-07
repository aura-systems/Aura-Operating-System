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
using System.IO;
using WaveOS.GUI;

namespace Aura_OS.System.Graphics
{
    public class GUI
    {
        public void DrawVerticalGradient(int X, int Y, int Width, int Height, List<Pen> gradient)
        {
            for (int i = 0; i < Height; i++)
            {
                float e = (float)i / (Height);
                e = e * gradient.Count;

                //DrawLine(X + i, Y, X + i, Y + Height, gradient[(int)e]);
                DrawLine(X, Y + i, X + Width, Y + i, gradient[(int)e]);
            }
        }

        public void DrawFilledRectangle(int X, int Y, int Width, int Height, int Radius, Pen pen)
        {
            if (Radius == 0)
            {
                for (int IX = X; IX < X + Width; IX++)
                {
                    for (int IY = Y; IY < Y + Height; IY++)
                    {
                        Aura_OS.Kernel.canvas.DrawPoint(pen, IX, IY);
                    }
                }
            }
            else
            {
                DrawFilledRectangle(X + Radius, Y, Width - Radius * 2, Height, 0, pen);
                DrawFilledRectangle(X, Y + Radius, Width, Height - Radius * 2, 0, pen);

                DrawFilledCircle(X + Radius, Y + Radius, Radius, pen);
                DrawFilledCircle(X + Width - Radius - 1, Y + Radius, Radius, pen);

                DrawFilledCircle(X + Radius, Y + Height - Radius - 1, Radius, pen);
                DrawFilledCircle(X + Width - Radius - 1, Y + Height - Radius - 1, Radius, pen);
            }
        }

        public void WriteByte(char ch, int mX, int mY, Pen pen)
        {
            Kernel.canvas.DrawChar((char)ch, Kernel.font, pen, mX, mY);
        }

        public void SetCursorPos(int mX, int mY)
        {
            /*if (Kernel.console.CursorVisible)
            {
                DrawFilledRectangle((int)Kernel.console.X + mX * Kernel.font.Width,
                    (int)Kernel.console.Y + mY * Kernel.font.Height + Kernel.font.Height, 8, 4, Kernel.console.ForegroundPen);
            }*/
        }

        public void DrawFilledCircle(int X, int Y, int Radius, Pen pen, int StartAngle = 0, int EndAngle = 360)
        {
            if (Radius == 0)
            {
                return;
            }

            for (int I = 0; I < Radius; I++)
            {
                Kernel.canvas.DrawCircle(pen, X, Y, Radius); //TODO Port this
            }
        }

        public Pen pen = new Pen(Color.FromArgb(223, 223, 223));
        public Pen pen2 = new Pen(Color.FromArgb(127, 127, 127));

        public void DrawLine(int X, int Y, int X2, int Y2, Pen pen)
        {
            int dx = Math.Abs(X2 - X), sx = X < X2 ? 1 : -1;
            int dy = Math.Abs(Y2 - Y), sy = Y < Y2 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2;

            while (X != X2 || Y != Y2)
            {
                Kernel.canvas.DrawPoint(pen, X, Y);
                int e2 = err;
                if (e2 > -dx) { err -= dy; X += sx; }
                if (e2 < dy) { err += dx; Y += sy; }
            }
        }

        public void Draw3DBorder(int X, int Y, int Width, int Height)
        {
            //Top and left white lines
            DrawFilledRectangle(X, Y, Width - 1, 1, 0, Kernel.WhitePen);
            DrawFilledRectangle(X, Y, 1, Height - 1, 0, Kernel.WhitePen);

            //Inner shadow Top
            DrawFilledRectangle(X + 1, Y + 1, Width - 1, 1, 0, pen);
            DrawFilledRectangle(X + 1, Y + 1, 1, Height - 1, 0, pen);

            //Inner shadow Bottom
            DrawFilledRectangle(X + 1, (Y + Height) - 2, Width - 1, 1, 0, pen2);
            DrawFilledRectangle((X + Width) - 2, Y + 1, 1, Height - 1, 0, pen2);

            //Bottom and right black lines
            DrawFilledRectangle(X, (Y + Height) - 1, Width, 1, 0, Kernel.BlackPen);
            DrawFilledRectangle((X + Width) - 1, Y, 1, Height, 0, Kernel.BlackPen);
        }
    }
}