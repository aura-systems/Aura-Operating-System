/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Surface class: a layout's Canvas
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Aura_OS.System.Input;
using CosmosMouse = Cosmos.Kernel.System.Input.MouseManager;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// A surface the app's code draws on (a layout's Canvas): what is drawn stays until it is drawn
    /// over or cleared. A new size clears it to its background. A press of the left button on it calls
    /// Click, ClickX and ClickY saying where; then, while the button is down, each move of the mouse
    /// (even off the surface) calls Move, and the button going up calls Release. A move over it with the
    /// button up calls Move too, so the app can change its Cursor there.
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
        /// The mouse moved over the surface, or anywhere while the button pressed on it is down; null for
        /// nothing.
        /// </summary>
        public Action Move;

        /// <summary>
        /// The button pressed on the surface went up, null for nothing.
        /// </summary>
        public Action Release;

        /// <summary>
        /// The cursor shown while the mouse is over the surface, and while the button pressed on it is down.
        /// </summary>
        public CursorState Cursor = CursorState.Normal;

        /// <summary>
        /// Where the last click was, in pixels from the surface's top left corner.
        /// </summary>
        public int ClickX { get; private set; }
        public int ClickY { get; private set; }

        /// <summary>
        /// The left button went down on the surface and is still down.
        /// </summary>
        public bool Pressed { get; private set; }

        /// <summary>
        /// Where the mouse is, in pixels from the surface's top left corner: below 0 or past its size when
        /// it is off the surface.
        /// </summary>
        public int MouseX => (int)CosmosMouse.X - AbsoluteX;
        public int MouseY => (int)CosmosMouse.Y - AbsoluteY;

        // Where the mouse was at the last update: Move is for a change.
        private int _lastX = int.MinValue;
        private int _lastY = int.MinValue;

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

            ClickX = MouseX;
            ClickY = MouseY;
            _lastX = ClickX;
            _lastY = ClickY;
            Pressed = true;

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
        /// Calls Move and Release as the mouse goes, and shows the Cursor. A surface looks the same under
        /// the mouse: no hover state, so no redraw when the mouse crosses it.
        /// </summary>
        public override void Update()
        {
            int x = MouseX;
            int y = MouseY;
            bool moved = x != _lastX || y != _lastY;
            _lastX = x;
            _lastY = y;

            if (Pressed)
            {
                if (!Kernel.MouseManager.IsLeftButtonDown)
                {
                    Pressed = false;

                    if (Release != null)
                    {
                        Release();
                    }
                }
                else
                {
                    if (moved && Move != null)
                    {
                        Move();
                    }

                    Input.MouseManager.CursorState = Cursor;
                }

                return;
            }

            // Under another window, the mouse is not over it.
            if (x < 0 || y < 0 || x >= Width || y >= Height || !Kernel.MouseManager.IsOnTop(this))
            {
                return;
            }

            if (moved && Move != null)
            {
                Move();
            }

            Input.MouseManager.CursorState = Cursor;
        }

        /// <summary>
        /// The drawing is already in the buffer: nothing to redraw.
        /// </summary>
        public override void Draw()
        {
        }
    }
}
