/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Panel class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Panel : Component
    {
        public Color Color1;
        public Color? Color2;
        public bool Borders = false;
        public bool Background = true;

        public Panel(Color color, int x, int y, int width, int height) : base(0, y, width, height)
        {
            Color1 = color;
        }

        public Panel(Color color1, Color color2, int x, int y, int width, int height) : base(0, y, width, height)
        {
            Color1 = color1;
            Color2 = color2;
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
                DrawLine(Kernel.DarkGray, 0, 0, 0 + Width, 0);
                DrawLine(Kernel.DarkGray, 0, 0, 0, 0 + Height);
                DrawLine(Kernel.WhiteColor, 0, 0 + Height, 0 + Width + 1, 0 + Height);
                DrawLine(Kernel.WhiteColor, 0 + Width, 0, 0 + Width, 0 + Height);
            }
        }
    }
}