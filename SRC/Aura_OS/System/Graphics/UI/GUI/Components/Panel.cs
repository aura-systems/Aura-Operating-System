/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Panel class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Graphics.Fonts;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Panel : Component
    {
        public Color Color1;
        public Color? Color2;
        public bool Borders = false;
        public bool Background = true;
        public string Text = "";

        public Panel(Color color, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Color1 = color;
        }

        public Panel(Color color1, Color color2, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Color1 = color1;
            Color2 = color2;
        }

        /// <summary>
        /// A panel looks the same under the mouse: no hover state, so no redraw over the controls on it.
        /// </summary>
        public override void Update()
        {
        }

        public override void Draw()
        {
            if (Background)
            {
                if (Color2 == null)
                {
                    Clear(Color1);
                }
                else
                {
                    DrawGradient(Color1, Color2.Value, 0, 0, Width, Height);
                }
            }

            if (Borders)
            {
                // Lines include both ends and are clipped: the far edges are column Width - 1 and row Height - 1.
                DrawLine(Kernel.DarkGray, 0, 0, Width - 1, 0);
                DrawLine(Kernel.DarkGray, 0, 0, 0, Height - 1);
                DrawLine(Kernel.WhiteColor, 0, Height - 1, Width - 1, Height - 1);
                DrawLine(Kernel.WhiteColor, Width - 1, 0, Width - 1, Height - 1);
            }

            if (Text != "")
            {
                // GEN3-GAP(psf): PCScreenFont.Default is internal and DefaultFont is now 16x32; use Aura's zap font.
                DrawString(Text, Kernel.font, Kernel.WhiteColor, 5, 3);
            }
        }
    }
}