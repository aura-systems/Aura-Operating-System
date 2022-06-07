using Cosmos.HAL;
using WaveOS.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mouse = Cosmos.System.MouseManager;
using IL2CPU.API.Attribs;
using Cosmos.System;
using WaveOS.GUI;
using Aura_OS;

namespace WaveOS.Apps
{
    public class WindowManager : WaveGUIApp
    {
        [ManifestResourceStream(ResourceName = "WaveOS.Assets.Cursor3.bmp")]
        static byte[] pointer;

        //[ManifestResourceStream(ResourceName = "WaveOS.Assets.Wallpaper.bmp")]
        //static byte[] wallpaper;

        //public Formats.Bitmap pointerImg;
        //public static Formats.Bitmap wallpaperImg;
        public Graphics.Color desktopColor = new WaveOS.Graphics.Color((byte)Aura_OS.Kernel.rnd.Next(255), (byte)Aura_OS.Kernel.rnd.Next(255), (byte)Aura_OS.Kernel.rnd.Next(255));

        public bool drawBG = false;

        public List<WaveWindow> windows = new List<WaveWindow>();
        public List<WaveWindow> windowsToClose = new List<WaveWindow>();
        public List<WaveWindow> windowsToOpen = new List<WaveWindow>();

        public Action<WaveWindow> WindowClosed;
        public Action<WaveWindow> WindowOpened;
        public Action<WaveWindow> WindowSetActive;
        public Action<WaveWindow> WindowSetInActive;

        public int restrictedTaskbarSize = 28;

        public override void Initialize()
        {
            Name = "Window Manager";
            //if (pointerImg == null)
            //    pointerImg = new Formats.Bitmap(pointer);
            //wallpaperImg = new Formats.Bitmap(wallpaper);

            OpenWindow(new WaveWindow("Test window", 50, 50, 200, 200, this));
            OpenWindow(new WaveWindow("Second Window", 270, 100, 200, 300, this));
            //OpenWindow(new WaveWindow("Third Window", 400, 300, 300, 100, this));

            WaveWindow window = new WaveWindow("Third Window", 400, 300, 300, 100, this);
            window.children.Add(new WaveButton() { X = 0, Y = 0, parent = window, Text = "aa", Width = 20, Height = 20 });
            OpenWindow(window);

            OpenWindow(new WaveTaskbar(0, Aura_OS.Kernel.canvas.Mode.Rows - 28, Aura_OS.Kernel.canvas.Mode.Columns, 28, this));
        }

        public int Run()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent k))
            {
                if(k.Key == ConsoleKeyEx.Escape)
                {
                    return 0;
                }

                if (k.Key == ConsoleKeyEx.R)
                {
                    Cosmos.System.Power.Reboot();
                }

                if (k.Key == ConsoleKeyEx.P)
                {
                    desktopColor = new WaveOS.Graphics.Color((byte)Aura_OS.Kernel.rnd.Next(255), (byte)Aura_OS.Kernel.rnd.Next(255), (byte)Aura_OS.Kernel.rnd.Next(255));
                }

                if (k.Key == ConsoleKeyEx.O)
                {
                    drawBG = !drawBG;
                }
            }

            Aura_OS.Kernel.canvas.Clear(desktopColor.ARGB);

            //if (drawBG)
                //Canv.DrawBitmap(0, 0, wallpaperImg);


            for (int i = windows.Count - 1; i >= 0; i--)
            {
                windows[i].Update();
            }

            foreach (var window in windows)
            {
                window.Draw();
            }

            foreach (var window in windowsToClose)
            {
                windows.Remove(window);
                WindowClosed?.Invoke(window);
            }

            windowsToClose.Clear();

            foreach (var window in windowsToOpen)
            {
                windows.Add(window);
                WindowOpened?.Invoke(window);
            }

            windowsToOpen.Clear();

            if (windowToSetActive != null)
            {
                foreach (var item in windows)
                {
                    item.Active = false;
                }

                windows.Remove(windowToSetActive);
                windowToSetActive.Active = true;
                if (windowToSetActive.StayOnTop || !windows[windows.Count - 1].StayOnTop) //If the window is meant to stay on top, or there is no other window like that
                    windows.Add(windowToSetActive);
                else //Other window was meant to be on top? Find closest index where we can put the window instead
                {
                    windows.Insert(FindLastNOWindowIndex() + 1, windowToSetActive);
                }

                WindowSetActive?.Invoke(windowToSetActive);

                windowToSetActive = null;
            }

            if(windowToSetInActive != null)
            {
                windowToSetInActive.Active = false;
                WindowSetInActive?.Invoke(windowToSetInActive);
                windowToSetInActive = null;
            }

            //Aura_OS.Kernel.canvas.DrawImage((int)Mouse.X, (int)Mouse.Y, pointerImg);

            //Canv.DrawString(0, 0, WaveShell.Canv.FPS + " FPS", Graphics.Color.White);

            //Aura_OS.Kernel.canvas.Update();

            return -1;
        }


        WaveWindow windowToSetActive;
        WaveWindow windowToSetInActive;
        int freeWindID = 0;

        internal void OpenWindow(WaveWindow window)
        {
            window.windID = freeWindID;
            freeWindID++;
            windowsToOpen.Add(window);
        }
        internal void CloseWindow(WaveWindow waveWindow)
        {
            windowsToClose.Add(waveWindow);
        }

        internal void SetActiveWindow(WaveWindow waveWindow)
        {
            windowToSetActive = waveWindow;
        }

        internal void SetInActiveWindow(WaveWindow waveWindow)
        {
            windowToSetInActive = waveWindow;
        }

        public int FindLastNOWindowIndex()
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (!windows[i].StayOnTop)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
