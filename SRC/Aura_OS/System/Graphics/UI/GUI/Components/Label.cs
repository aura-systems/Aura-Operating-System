/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Label class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Graphics.Fonts;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Label : Component
    {
        public Color TextColor;

        private string _text;

        public Label(string text, Color color, int x, int y) : base(x, y, text.Length * Kernel.font.Width, Kernel.font.Height)
        {
            TextColor = color;
            _text = text;
        }

        /// <summary>
        /// The label resizes to its text, and redraws only when the text changes.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                value = value ?? "";

                if (value == _text)
                {
                    return;
                }

                _text = value;

                if (PreferredWidth != Width)
                {
                    SetSize(PreferredWidth, Height);
                }

                MarkDirty();
            }
        }

        /// <summary>
        /// The text's size in Kernel.font.
        /// </summary>
        public override int PreferredWidth => _text.Length * Kernel.font.Width;
        public override int PreferredHeight => Kernel.font.Height;

        public override void Draw()
        {
            Clear(Color.Transparent);

            if (_text != "")
            {
                DrawString(_text, Kernel.font, TextColor, 0, 0);
            }
        }
    }
}
