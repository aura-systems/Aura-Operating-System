using Aura_OS.System.GUI.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
 namespace Aura_OS.System.GUI.UI
{
    public class WindowsManager
    {

        public static Framework.Graphics.Image Close = Framework.Graphics.Image.Load(Images.ControlBox.Close);

        public static List<Window> Active_Windows = new List<Window>();
        public static void AddWindow(int sizex, int sizey, int posx, int posy, string name)
        {
            Window win = new Window();
            Util.Point position = new Util.Point((uint)posx, (uint)posy);
            Util.Point size = new Util.Point((uint)sizex, (uint)sizey);
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
            Desktop.g.FillRectangle((int)pos.X, (int)pos.Y - 21, (int)size.X * 4, 20, Colors.AliceBlue);
            Desktop.g.DrawString(Name, (int)pos.X + 1, (int)pos.Y - 20, Colors.Black, Fonts.CFF._Pixel7_Mini_cff);
            Desktop.g.DrawRectangle((int)pos.X - 1, (int)pos.Y - 22, (int)size.X + 2, (int)size.Y + 22, Colors.DarkGray);
            Desktop.g.DrawImage(WindowsManager.Close, (int)(pos.X + size.X) - 16, (int)pos.Y - 19, Colors.Lime);
            CloseArea.X = (int)(pos.X + size.X) - 16;
            CloseArea.Y = (int)pos.Y - 19;
            CloseArea.XMAX = CloseArea.X + 14;
            CloseArea.YMAX = CloseArea.Y + 14;
        }

        public bool IsXCloseArea(int X)
        {
            if ((X > CloseArea.X) && (X < CloseArea.XMAX))
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool IsYCloseArea(int Y)
        {
            if ((Y > CloseArea.Y) && (Y < CloseArea.YMAX))
            {
                return true;
            }
            else
            {
                return false;
            }
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