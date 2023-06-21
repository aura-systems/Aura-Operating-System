/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
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
    public class Button : Element
    {
        public string Text;
        public Bitmap Image;

        public Button(string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Text = text;
        }

        public Button(Bitmap image, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Image = image;
        }

        public Button(Bitmap image, int x, int y) : base(x, y, (int)image.Width, (int)image.Height)
        {
            Image = image;
        }

        public override void Update()
        {
            Kernel.canvas.DrawFilledRectangle(Kernel.DarkGrayLight, X + 2, Y + 2, Width - 3, Height - 3);

            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X + Width, Y, X + Width, Y + Height);

            if (Text != null)
            {
                Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4, Y + (Height / 2 - Kernel.font.Height / 2));
            }
            else if (Image != null)
            {
                Kernel.canvas.DrawImage(Image, X, Y);
            }
        }
    }
}