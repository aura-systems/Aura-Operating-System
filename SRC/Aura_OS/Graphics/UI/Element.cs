/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
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

namespace Aura_OS.System.Graphics.UI
{
    public class Element
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Element(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public virtual void Update()
        {
            Kernel.canvas.DrawFilledRectangle(Kernel.DarkGrayLight, X + 2, Y + 2, Width - 3, Height - 3);

            Kernel.canvas.DrawLine(Kernel.Gray, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X + Width, Y, X + Width, Y + Height);
        }
    }
}