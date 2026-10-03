/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Layout element of a control
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// A control of a layout file: a component child of the app window, moved (and, when it
    /// stretches, resized) by its container.
    /// </summary>
    public class ControlNode : LayoutNode
    {
        public readonly Component Component;

        // The size the control was created with: what it wants when the file gives no size.
        private readonly int _naturalWidth;
        private readonly int _naturalHeight;

        // What the last placing saw, to notice the app's code showing, hiding or relabelling the control.
        private bool _placedVisible;
        private int _placedWidth;
        private int _placedHeight;

        public ControlNode(Component component)
        {
            Component = component;
            _naturalWidth = component.Width;
            _naturalHeight = component.Height;
        }

        public override bool Visible
        {
            get
            {
                return Component.Visible;
            }
            set
            {
                Component.Visible = value;
            }
        }

        public override void Measure()
        {
            DesiredWidth = ContentWidth() + Margin.Horizontal;
            DesiredHeight = ContentHeight() + Margin.Vertical;
        }

        public override void Arrange(int x, int y, int width, int height)
        {
            x += Margin.Left;
            y += Margin.Top;
            width = Math.Max(0, width - Margin.Horizontal);
            height = Math.Max(0, height - Margin.Vertical);

            int controlWidth = Fills(Width, HorizontalAlignment) ? width : ContentWidth();
            int controlHeight = Fills(Height, VerticalAlignment) ? height : ContentHeight();

            if (controlWidth != Component.Width || controlHeight != Component.Height)
            {
                Component.SetSize(controlWidth, controlHeight);
            }

            int left = Align(x, width, controlWidth, HorizontalAlignment);
            int top = Align(y, height, controlHeight, VerticalAlignment);

            // The setters recompute the absolute position of the control and of its children.
            if (Component.X != left)
            {
                Component.X = left;
            }

            if (Component.Y != top)
            {
                Component.Y = top;
            }
        }

        /// <summary>
        /// Remembers the visibility and width the window's elements were placed with.
        /// </summary>
        internal void MarkPlaced()
        {
            _placedVisible = Component.Visible;
            _placedWidth = ContentWidth();
            _placedHeight = ContentHeight();
        }

        /// <summary>
        /// The control was shown, hidden or wants another size (a label's new text, a picture's new
        /// image) since the window's elements were placed: the others have to move.
        /// </summary>
        internal bool ChangedSincePlaced()
        {
            return Component.Visible != _placedVisible || ContentWidth() != _placedWidth || ContentHeight() != _placedHeight;
        }

        private int ContentWidth()
        {
            if (Width >= 0)
            {
                return Width;
            }

            // A label or a picture fits its text or image.
            int preferred = Component.PreferredWidth;
            return preferred >= 0 ? preferred : _naturalWidth;
        }

        private int ContentHeight()
        {
            if (Height >= 0)
            {
                return Height;
            }

            int preferred = Component.PreferredHeight;
            return preferred >= 0 ? preferred : _naturalHeight;
        }

        private static bool Fills(int size, Alignment alignment)
        {
            return size == Star || (size == Auto && alignment == Alignment.Stretch);
        }

        private static int Align(int start, int space, int size, Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Center:
                    return start + (space - size) / 2;
                case Alignment.End:
                    return start + space - size;
                default:
                    return start;
            }
        }
    }
}
