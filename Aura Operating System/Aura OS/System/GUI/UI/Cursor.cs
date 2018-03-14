using Aura_OS.System.GUI.Graphics;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.UI
{
    public static class Cursor
    {
        public static bool Enabled { get; set; }

        public static Mouse Mouse = new Mouse();

        public static int sizex = 7;
        public static int sizey = 7;

        public static void Init()
        {
            Mouse.Initialize((uint)Desktop.Width, (uint)Desktop.Height - (uint)sizey);
        }

        public static void Render()
        {
            if (Enabled)
            {
                //Desktop.g.DrawImage(Mouse.X, Mouse.Y, Desktop.cursor);
                Desktop.g.FillRectangle(Mouse.X, Mouse.Y, sizex, sizey, Colors.Black);
            }
        }
    }
}
