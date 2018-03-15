using Aura_OS.System.GUI.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.UI
{
    public class WindowsManager
    {

        public static List<Window> Active_Windows { get; protected set; } = new List<Window>();

        public static void AddWindow(int sizex, int sizey, int posx, int posy, string name)
        {
            Window win = new Window();
            Util.Point position = new Util.Point(posx, posy);
            Util.Point size = new Util.Point(sizex, sizey);
            win.pos = position;
            win.size = size;
            win.Name = name;
            AddWindow(win);
        }

        public static void AddWindow(Window window)
        {
            Active_Windows.Add(window);
        }

        public static void ShowWindows()
        {
            foreach (Window wind in Active_Windows)
            {
                wind.Draw();
            }
        }
    }

    public class Window
    {

        public string Name;

        public Util.Point pos;

        public Util.Point size;

        public Util.Area CloseArea;

        public void Draw()
        {
            Desktop.g.FillRectangle(pos.X, pos.Y - 21, size.X, 20, Colors.AliceBlue);
            Desktop.g.DrawString(Name, pos.X + 1, pos.Y - 20, Colors.Black, Fonts.CFF.Pixel13_cff);
            Desktop.g.FillRectangle(pos.X, pos.Y - 1, size.X + 1, size.Y + 1, Colors.White);
            Desktop.g.DrawRectangle(pos.X - 1, pos.Y - 22, size.X + 2, size.Y + 22, Colors.DarkGray);
            Desktop.g.FillRectangle(pos.X + size.X - 19, pos.Y - 21, 20, 20, Colors.Red);
            CloseArea.X = pos.X + size.X - 19;
            CloseArea.Y = pos.Y - 20;
            CloseArea.XMAX = CloseArea.X + 19;
            CloseArea.YMAX = CloseArea.Y + 19;
        }

    }

    public enum WindowState
    {

        fullscreen = 0x01,
        min = 0x02,
        max = 0x03,
        windowed = 0x04,

    }

    public enum BorderStyle
    {

        None = 0x00,
        Fixed3D = 0x10,
        FixedDialog = 0x11,
        FixedSingle = 0x12,
        FixedToolWindow = 0x13,
        Sizable = 0x20,
        SizableToolWindow = 0x21

    }
}
