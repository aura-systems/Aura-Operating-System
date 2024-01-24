/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;

namespace Aura_OS.System.Processing.Application
{
    public class PictureApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Picture";

        public Bitmap Image;

        public PictureApp(string name, Bitmap bitmap, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {
            ApplicationName = name;
            Image = bitmap;
        }

        public override void UpdateApp()
        {
            Kernel.canvas.DrawImageAlpha(Image, x + (int)(width / 2 - Image.Width / 2), y + (int)(height / 2 - Image.Height / 2));
        }
    }
}
