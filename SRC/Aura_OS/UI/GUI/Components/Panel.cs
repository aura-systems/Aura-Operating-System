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

namespace Aura_OS.System.UI.GUI.Components
{
    public class Panel : Component
    {
        public Color Color1;
        public Color? Color2;

        public Panel(Color color, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Color1 = color;
        }

        public Panel(Color color1, Color color2, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Color1 = color1;
            Color2 = color2;
        }

        public override void Update()
        {
            if (Color2 == null)
            {
                Kernel.canvas.DrawFilledRectangle(Color1, X, Y, Width, Height);
            }
            else
            {
                Utils.DrawGradient(Color1, Color2.Value, X, Y, Width, Height);
            }
        }
    }
}