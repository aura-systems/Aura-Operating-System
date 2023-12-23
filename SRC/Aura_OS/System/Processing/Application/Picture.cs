/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Aura_OS.System.Processing.Application
{
    public class Picture : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Picture";

        public Bitmap Image;

        public Picture(string name, Bitmap bitmap, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
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
