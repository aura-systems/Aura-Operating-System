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
                DrawGradient(Color1, Color2.Value, X, Y, Width, Height);
            }
        }

        private void DrawGradient(Color color1, Color color2, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                // Calculate the ratio of the current position relative to the total width
                float ratio = (float)i / width;

                // Interpolate the RGB values based on the ratio
                byte r = (byte)((color2.R - color1.R) * ratio + color1.R);
                byte g = (byte)((color2.G - color1.G) * ratio + color1.G);
                byte b = (byte)((color2.B - color1.B) * ratio + color1.B);

                Color interpolatedColor = Color.FromArgb(0xff, r, g, b);

                for (int j = 0; j < height; j++)
                {
                    Kernel.canvas.DrawPoint(interpolatedColor, x + i, y + j);
                }
            }
        }
    }
}