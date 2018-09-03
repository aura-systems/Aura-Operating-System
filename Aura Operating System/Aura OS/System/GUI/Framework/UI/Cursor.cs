using Aura_OS.System.GUI.Graphics;
using Cosmos.HAL;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Text;
 namespace Aura_OS.System.GUI.UI
{
    public static class Cursor
    {
        public static bool Enabled { get; set; }
        public static Framework.Graphics.Image Image;

        public static void Init()
        {
            MouseManager.ScreenHeight = (uint)Desktop.Height;
            MouseManager.ScreenWidth = (uint)Desktop.Width;
        }

        public static void Render()
        {
            if (Enabled)
            {
                Desktop.g.DrawImage(Image, (int)MouseManager.X, (int)MouseManager.Y, Colors.Lime);
            }
        }

    }
}