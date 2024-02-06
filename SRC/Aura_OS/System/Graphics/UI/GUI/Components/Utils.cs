/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Graphics utils class.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
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
                    Kernel.Canvas.DrawPoint(interpolatedColor, x + i, y + j);
                }
            }
        }
    }
}
