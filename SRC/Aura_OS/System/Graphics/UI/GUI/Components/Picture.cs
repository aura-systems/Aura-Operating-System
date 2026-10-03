/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Picture class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Shows a bitmap, blended by its alpha. The picture takes the size of its image.
    /// </summary>
    public class Picture : Component
    {
        private Bitmap _image;

        public Picture(Bitmap image, int x, int y) : base(x, y, image != null ? (int)image.Width : 0, image != null ? (int)image.Height : 0)
        {
            _image = image;
        }

        /// <summary>
        /// The bitmap shown, null for none.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;

                if (PreferredWidth != Width || PreferredHeight != Height)
                {
                    SetSize(PreferredWidth, PreferredHeight);
                }

                MarkDirty();
            }
        }

        public override int PreferredWidth => _image != null ? (int)_image.Width : 0;
        public override int PreferredHeight => _image != null ? (int)_image.Height : 0;

        /// <summary>
        /// A picture looks the same under the mouse: no hover state, so no redraw when the mouse
        /// crosses it.
        /// </summary>
        public override void Update()
        {
        }

        public override void Draw()
        {
            Clear(Color.Transparent);
            DrawImage(_image, 0, 0);
        }
    }
}
