/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Label class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Graphics.Fonts;
using System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Label : Component
    {
        public Color TextColor;

        private string _text;

        // The text cut at its '\n's, and its longest line.
        private string[] _lines;
        private int _columns;

        public Label(string text, Color color, int x, int y) : base(x, y, Columns(text) * Kernel.font.Width, LineCount(text) * Kernel.font.Height)
        {
            TextColor = color;
            SetText(text);
        }

        /// <summary>
        /// The label resizes to its text, and redraws only when the text changes. A '\n' starts a
        /// new line.
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

                SetText(value);

                if (PreferredWidth != Width || PreferredHeight != Height)
                {
                    SetSize(PreferredWidth, PreferredHeight);
                }

                MarkDirty();
            }
        }

        /// <summary>
        /// The text's size in Kernel.font: its longest line by its number of lines.
        /// </summary>
        public override int PreferredWidth => _columns * Kernel.font.Width;
        public override int PreferredHeight => _lines.Length * Kernel.font.Height;

        public override void Draw()
        {
            Clear(Color.Transparent);

            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] != "")
                {
                    DrawString(_lines[i], Kernel.font, TextColor, 0, i * Kernel.font.Height);
                }
            }
        }

        private void SetText(string text)
        {
            _text = text ?? "";
            _lines = _text.Split('\n');
            _columns = Columns(_text);
        }

        private static int LineCount(string text)
        {
            return string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
        }

        private static int Columns(string text)
        {
            int columns = 0;

            if (text != null)
            {
                foreach (string line in text.Split('\n'))
                {
                    columns = Math.Max(columns, line.Length);
                }
            }

            return columns;
        }
    }
}
