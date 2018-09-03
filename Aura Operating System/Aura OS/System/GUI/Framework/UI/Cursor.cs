
using Aura_OS.System.GUI.Graphics;
using Cosmos.System;
 namespace Aura_OS.System.GUI.UI
{
    public static class Cursor
    {
        public static bool Enabled { get; set; }
        public static Framework.Graphics.Image Image;

        public static int CursorSize;

        public static void Init()
        {
            MouseManager.ScreenHeight = (uint)Desktop.Height;
            MouseManager.ScreenWidth = (uint)Desktop.Width;
            MouseManager.X = 1;
            MouseManager.X = 1;
            CursorSize = Image.Height;
        }

        public static void Render()
        {
            if (Enabled)
            {
                Desktop.lastx = (int)MouseManager.X;
                Desktop.lasty = (int)MouseManager.Y;
                Desktop.g.DrawImage(Image, (int)MouseManager.X, (int)MouseManager.Y, Colors.Lime);
            }
        }

    }
}