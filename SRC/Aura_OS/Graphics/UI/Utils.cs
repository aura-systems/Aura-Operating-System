using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Graphics.UI
{
    public class Utils
    {
        public static void DrawGradient(Color color1, Color color2, int x, int y, int width, int height)
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
