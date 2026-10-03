/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Stack layout
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Places its elements one after the other, top to bottom or left to right, Spacing pixels apart.
    /// An element sized "*" on that axis takes the space the others leave (shared between several).
    /// Across the axis, each element gets the stack's whole width (or height) and aligns in it.
    /// </summary>
    public class StackNode : ContainerNode
    {
        public Orientation Orientation = Orientation.Vertical;
        public int Spacing;

        private bool IsVertical => Orientation == Orientation.Vertical;

        public override void Measure()
        {
            int along = 0;
            int across = 0;
            int count = 0;

            foreach (LayoutNode child in Children)
            {
                if (!child.Visible)
                {
                    continue;
                }

                child.Measure();

                along += IsVertical ? child.DesiredHeight : child.DesiredWidth;
                across = Math.Max(across, IsVertical ? child.DesiredWidth : child.DesiredHeight);
                count++;
            }

            if (count > 1)
            {
                along += Spacing * (count - 1);
            }

            int width = IsVertical ? across : along;
            int height = IsVertical ? along : across;

            DesiredWidth = (Width >= 0 ? Width : width + Padding.Horizontal) + Margin.Horizontal;
            DesiredHeight = (Height >= 0 ? Height : height + Padding.Vertical) + Margin.Vertical;
        }

        public override void Arrange(int x, int y, int width, int height)
        {
            x += Margin.Left + Padding.Left;
            y += Margin.Top + Padding.Top;
            width = Math.Max(0, width - Margin.Horizontal - Padding.Horizontal);
            height = Math.Max(0, height - Margin.Vertical - Padding.Vertical);

            // The space of the "*" elements: what the others and the spacing leave.
            int used = 0;
            int stars = 0;
            int count = 0;

            foreach (LayoutNode child in Children)
            {
                if (!child.Visible)
                {
                    continue;
                }

                if (IsStar(child))
                {
                    stars++;
                }
                else
                {
                    used += IsVertical ? child.DesiredHeight : child.DesiredWidth;
                }

                count++;
            }

            if (count > 1)
            {
                used += Spacing * (count - 1);
            }

            int free = Math.Max(0, (IsVertical ? height : width) - used);
            int position = IsVertical ? y : x;
            int star = 0;

            foreach (LayoutNode child in Children)
            {
                if (!child.Visible)
                {
                    continue;
                }

                int size;

                if (IsStar(child))
                {
                    // The last "*" element also takes the pixels the division leaves.
                    star++;
                    size = star == stars ? free - (free / stars) * (stars - 1) : free / stars;
                }
                else
                {
                    size = IsVertical ? child.DesiredHeight : child.DesiredWidth;
                }

                if (IsVertical)
                {
                    child.Arrange(x, position, width, size);
                }
                else
                {
                    child.Arrange(position, y, size, height);
                }

                position += size + Spacing;
            }
        }

        private bool IsStar(LayoutNode child)
        {
            return (IsVertical ? child.Height : child.Width) == Star;
        }
    }
}
