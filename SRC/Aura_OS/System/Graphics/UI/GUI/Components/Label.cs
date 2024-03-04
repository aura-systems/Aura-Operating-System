/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Label class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics.Fonts;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Label : Component
    {
        public Color TextColor;
        public string Text = "";

        public Label(string text, Color color, int x, int y) : base(x, y, text.Length * Kernel.font.Width, Kernel.font.Height)
        {
            TextColor = color;
            Text = text;
        }

        public override void Draw()
        {
            Clear(Color.Transparent);

            if (Text != "")
            {
                DrawString(Text, Kernel.font, TextColor, 0, 0);
            }
        }
    }
}