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

        public Button(string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Text = text;
        }

        public override void Update()
        {
            base.Update();
            Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4, Y + (Height / 2 - Kernel.font.Height / 2));
        }
    }
}