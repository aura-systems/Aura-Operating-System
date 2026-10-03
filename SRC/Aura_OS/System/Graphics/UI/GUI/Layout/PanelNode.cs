/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Stack layout on a panel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// A stack drawn on a Panel (a colored background, with or without borders) that fills the
    /// stack's space: a toolbar. The panel is drawn before the elements on it.
    /// </summary>
    public class PanelNode : StackNode
    {
        public readonly Panel Background;

        public PanelNode(Panel background)
        {
            Background = background;
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                Background.Visible = value;
                base.Visible = value;
            }
        }

        public override void Arrange(int x, int y, int width, int height)
        {
            int left = x + Margin.Left;
            int top = y + Margin.Top;
            int panelWidth = Math.Max(0, width - Margin.Horizontal);
            int panelHeight = Math.Max(0, height - Margin.Vertical);

            if (Background.Width != panelWidth || Background.Height != panelHeight)
            {
                Background.SetSize(panelWidth, panelHeight);
            }

            if (Background.X != left)
            {
                Background.X = left;
            }

            if (Background.Y != top)
            {
                Background.Y = top;
            }

            base.Arrange(x, y, width, height);
        }
    }
}
