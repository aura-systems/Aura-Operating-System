/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Surface class: a layout's Canvas
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Cosmos.Kernel.System.Mouse;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// A surface the app's code draws on (a layout's Canvas): what is drawn stays until it is drawn
    /// over or cleared. A new size clears it to its background. A click on it calls Click, ClickX and
    /// ClickY saying where.
    /// </summary>
    public class Surface : Component
    {
        /// <summary>
        /// The color Clear and a new size fill it with.
        /// </summary>
        public Color Background;

        /// <summary>
        /// A left click on the surface, null for nothing.
        /// </summary>
        public Action Click;

        /// <summary>
        /// Where the last click was, in pixels from the surface's top left corner.
        /// </summary>
        public int ClickX { get; private set; }
        public int ClickY { get; private set; }

        public Surface(Color background, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Background = background;
            Clear(background);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Clear(Background);
        }

        public override void HandleLeftClick()
        {
            // Closes an open menu, as a click anywhere else does.
            base.HandleLeftClick();

            ClickX = (int)MouseManager.X - AbsoluteX;
            ClickY = (int)MouseManager.Y - AbsoluteY;

            // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
            if (Click != null)
            {
                Click();
            }
        }

        /// <summary>
        /// Two quick clicks are two clicks.
        /// </summary>
        public override void HandleLeftDoubleClick()
        {
            HandleLeftClick();
        }

        /// <summary>
        /// A surface looks the same under the mouse: no hover state, so no redraw when the mouse
        /// crosses it.
        /// </summary>
        public override void Update()
        {
        }

        /// <summary>
        /// The drawing is already in the buffer: nothing to redraw.
        /// </summary>
        public override void Draw()
        {
        }
    }
}
