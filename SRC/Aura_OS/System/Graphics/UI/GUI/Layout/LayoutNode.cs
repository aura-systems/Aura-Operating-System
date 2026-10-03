/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Layout element base classes
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// Where an element sits in the space its container gives it. Stretch resizes it to fill that space.
    /// </summary>
    public enum Alignment
    {
        Start,
        Center,
        End,
        Stretch
    }

    /// <summary>
    /// Space around an element (margin) or inside a container (padding), in pixels.
    /// </summary>
    public struct Thickness
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Thickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Horizontal => Left + Right;

        public int Vertical => Top + Bottom;
    }

    /// <summary>
    /// An element of a layout file: a control (ControlNode) or a container placing its children
    /// (StackNode, GridNode). Placing runs in two passes, as in WPF: Measure computes the size each
    /// element wants, then Arrange gives it its place, in window coordinates.
    /// </summary>
    public abstract class LayoutNode
    {
        /// <summary>
        /// Width or Height given by the content.
        /// </summary>
        public const int Auto = -1;

        /// <summary>
        /// Width or Height taking the space left in a stack.
        /// </summary>
        public const int Star = -2;

        public string Id;
        public Thickness Margin;
        public int Width = Auto;
        public int Height = Auto;
        public Alignment HorizontalAlignment = Alignment.Start;
        public Alignment VerticalAlignment = Alignment.Center;
        public int ColumnSpan = 1;

        /// <summary>
        /// Position in the window's content area (x, y attributes). A positioned element is out of
        /// the flow of the window's elements; a missing coordinate centers it on that axis.
        /// </summary>
        public int? X;
        public int? Y;

        /// <summary>
        /// Size wanted by the last Measure, margin included.
        /// </summary>
        public int DesiredWidth { get; protected set; }
        public int DesiredHeight { get; protected set; }

        public bool IsPositioned => X.HasValue || Y.HasValue;

        /// <summary>
        /// A hidden element takes no space: the elements after it move up.
        /// </summary>
        public abstract bool Visible { get; set; }

        public abstract void Measure();

        /// <summary>
        /// Places the element in a slot (window coordinates, margin included). Measure runs first.
        /// </summary>
        public abstract void Arrange(int x, int y, int width, int height);
    }

    /// <summary>
    /// An element holding other elements.
    /// </summary>
    public abstract class ContainerNode : LayoutNode
    {
        public List<LayoutNode> Children = new List<LayoutNode>();
        public Thickness Padding;

        private bool _visible = true;

        /// <summary>
        /// Hiding a container hides all its elements, showing it shows them all again.
        /// </summary>
        public override bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;

                foreach (LayoutNode child in Children)
                {
                    child.Visible = value;
                }
            }
        }
    }
}
