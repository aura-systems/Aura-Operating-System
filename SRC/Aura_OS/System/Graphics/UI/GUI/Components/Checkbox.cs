/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Label class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics.Fonts;
using System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Checkbox : Component
    {
        public Color TextColor;
        public string Text = "";
        public bool Checked = false;

        private Button _check;

        public Checkbox(string text, Color color, int x, int y, bool isChecked = false) : base(x, y, (text.Length * Kernel.font.Width) + 13 + 6, 13)
        {
            _check = new Button((text.Length * Kernel.font.Width) + 3, 0, 13, 13);
            _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.off.normal"));
            _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.off.highlighted"));
            _check.Click = new Action(() =>
            {
                Checked = !Checked;
                UpdateCheckbox();
            });

            AddChild(_check);

            TextColor = color;
            Text = text;
            Checked = isChecked;

            UpdateCheckbox();
        }

        private void UpdateCheckbox()
        {
            if (Checked)
            {
                _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.on.normal"));
                _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.on.highlighted"));
                _check.Draw(this);
                MarkDirty();
            }
            else
            {
                _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.off.normal"));
                _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.off.highlighted"));
                _check.Draw(this);
                MarkDirty();
            }
        }

        public override void Update()
        {
            _check.Update();
        }

        public override void Draw()
        {
            Clear(Color.Transparent);

            _check.Draw(this);

            if (Text != "")
            {
                DrawString(Text, Kernel.font, TextColor, 0, 0);
            }
        }
    }
}