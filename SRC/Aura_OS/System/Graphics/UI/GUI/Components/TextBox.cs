/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class TextBox : Component
    {
        public string Text;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            Text = text;
        }

        public override void Update()
        {
            Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, X + 2, Y + 2, Width - 3, Height - 3);

            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);

            Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4, Y + (Height / 2 - Kernel.font.Height / 2));
        }
    }
}