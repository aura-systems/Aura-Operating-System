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
            }

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

            Aura_OS.Kernel.canvas.DrawImageAlpha(Aura_OS.Kernel.cursor, (int)Mouse.X, (int)Mouse.Y);

            //Canv.DrawString(0, 0, WaveShell.Canv.FPS + " FPS", Graphics.Color.White);

            //Aura_OS.Kernel.canvas.Update();

            return -1;
        }


        WaveWindow windowToSetActive;
        WaveWindow windowToSetInActive;
        int freeWindID = 0;

        internal void AddWindow(WaveWindow window)
        {
            window.windID = freeWindID;
            freeWindID++;
            window.State = WindowState.Minimized;
            windows.Add(window);
        }

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
