/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Surface class: a layout's Canvas
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// A surface the app's code draws on (a layout's Canvas): what is drawn stays until it is drawn
    /// over or cleared. A new size clears it to its background.
    /// </summary>
    public class Surface : Component
    {
        /// <summary>
        /// The color Clear and a new size fill it with.
        /// </summary>
        public Color Background;

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
