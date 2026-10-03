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
    public class Checkbox : Component
    {
        public Color TextColor;
        public string Text = "";

        /// <summary>
        /// Called after a click checks or unchecks the box.
        /// </summary>
        public Action Changed;

        private Button _check;
        private bool _checked;

        public Checkbox(string text, Color color, int x, int y, bool isChecked = false) : base(x, y, (text.Length * Kernel.font.Width) + 13 + 6, 13)
        {
            _check = new Button((text.Length * Kernel.font.Width) + 3, 0, 13, 13);
            _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.off.normal"));
            _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.off.highlighted"));
            _check.Click = new Action(() =>
            {
                Checked = !Checked;

                // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
                if (Changed != null)
                {
                    Changed();
                }
            });

            AddChild(_check);

            TextColor = color;
            Text = text;
            Checked = isChecked;
        }

        /// <summary>
        /// Setting it does not call Changed.
        /// </summary>
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
                UpdateCheckbox();
            }
        }

        private void UpdateCheckbox()
        {
            if (_checked)
            {
                _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.on.normal"));
                _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.on.highlighted"));
                _check.MarkDirty();
                MarkDirty();
            }
            else
            {
                _check.SetNormalFrame(Kernel.ThemeManager.GetFrame("check.off.normal"));
                _check.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("check.off.highlighted"));
                _check.MarkDirty();
                MarkDirty();
            }
        }

        public override void Update()
        {
            _check.Update();

            if (_check.IsDirty())
            {
                MarkDirty();
            }
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