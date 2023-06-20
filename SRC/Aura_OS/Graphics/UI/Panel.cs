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
using Cosmos.System;

namespace Aura_OS.System.Graphics.UI
{
    public class Panel : Element
    {
        public Color Color;

        public Panel(Color color, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Color = color;
        }

        public override void Update()
        {
            Kernel.canvas.DrawFilledRectangle(Color, X, Y, Width, Height);
        }
    }
}