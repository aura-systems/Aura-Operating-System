/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.System.Graphics;

namespace Aura_OS.System.Processing.Applications
{
    public class PictureApp : Application
    {
        public static string ApplicationName = "Picture";

        private Bitmap _image;

        public PictureApp(string name, Bitmap bitmap, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {
            ApplicationName = name;
            _image = bitmap;
        }

        public override void Draw()
        {
            base.Draw();

            DrawImageAlpha(_image, 0 + (int)(Width / 2 - _image.Width / 2), 0 + (int)(Height / 2 - _image.Height / 2));
        }
    }
}
