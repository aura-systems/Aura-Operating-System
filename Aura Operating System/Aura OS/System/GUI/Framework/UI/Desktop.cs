/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

 using System;
using System.Collections.Generic;
using Aura_OS.System.GUI.Graphics;
using Aura_OS.System.GUI.UI.Util;
using Cosmos.HAL;
using static Cosmos.System.MouseManager;

namespace Aura_OS.System.GUI.UI
{
    class Desktop
    {
            
        public static Canvas Canvas = new Canvas(Shell.VESAVBE.Graphics.ModeInfo.width, Shell.VESAVBE.Graphics.ModeInfo.height);
        public static SdfFont terminus;

        public static int Width = Shell.VESAVBE.Graphics.ModeInfo.width;
        public static int Height = Shell.VESAVBE.Graphics.ModeInfo.height;

        static int _frames = 0;
        static int _fps = 0;
        static int _deltaT = 0;
        static int _deltaTM = 0;

        public static int Main()
        {
             Initialize();
             while(true)
             {
                int ret = Update();
                if (ret == 1)
                {
                    break;
                }
                Render();
             }
             Final();
             return 0;
        }

        static bool windows = false;
        public static int Update()
        {
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.LeftWindows:
                        if (windows)
                        {
                            windows = false;
                        }
                        else
                        {
                            windows = true;
                        }
                        break;
                    case ConsoleKey.F1:
                        Cosmos.System.Power.Reboot();
                        break;
                    case ConsoleKey.F2:
                        Cosmos.System.Power.Shutdown();
                        break;
                    default:
                        break;
                }
            }
            return 0;
        }

        public static Graphics.Graphics g;

        public static void Render()
        {

            Refresh();

            if (_deltaT != RTC.Second)
            {
                _fps = _frames;
                _frames = 0;
                _deltaT = RTC.Second;
            }
            _frames++;

            g.DrawString(Time.TimeString(true, true, true), 0, 20, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            g.DrawString("FPS: " + _fps, 0, 40, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            switch (MouseState)
            {
                case Cosmos.System.MouseState.Right:
                    g.DrawString("Mouse: Right", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
                case Cosmos.System.MouseState.Left:
                    g.DrawString("Mouse: Left", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    MouseLeftEvent();
                    break;
                case Cosmos.System.MouseState.Middle:
                    g.DrawString("Mouse: Middle", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
                case Cosmos.System.MouseState.None:
                    g.DrawString("Mouse: None", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
                case Cosmos.System.MouseState.FourthButton:
                    g.DrawString("Mouse: FourthButton", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
                case Cosmos.System.MouseState.FifthButton:
                    g.DrawString("Mouse: FifthButton", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
                default:
                    g.DrawString("Mouse: Unknown!?", 0, 60, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);
                    break;
            }

            Cursor.Render();

            Canvas.WriteToScreen();
            //g.DrawArea(lastx, lasty, 12, 18, MouseBuffer);
        }

        public static List<Area> ToRefresh = new List<Area>();

        public static uint MouseBuffer;
        public static int lastx;
        public static int lasty;

        public static void Refresh()
        {
            //MouseBuffer = g.GetArea((int)X, (int)Y, 12, 18);
            g.FillRectangle(lastx, lasty, 12 * 4, 18, Colors.White);
            foreach (Area area in ToRefresh)
            {
                g.FillRectangle(area.X, area.Y, area.XMAX * 4, area.YMAX, Colors.White);
            }
        }

        public static void AddAreatoRefresh(int x, int y, int sizex, int sizey)
        {
            ToRefresh.Add(new Area(x, y, sizex, sizey));
        }

        public static void MouseLeftEvent()
        {
            foreach (Window wind in WindowsManager.Active_Windows)
            {
                if (wind.IsXCloseArea((int)X) && wind.IsYCloseArea((int)Y))
                {
                    WindowsManager.Active_Windows.Remove(wind);
                }
            }
        }

        public static void Initialize()
        {
            
            Shell.VESAVBE.Graphics.Clear(Colors.Blue.ToHex());

            g = new Graphics.Graphics(Canvas);
            g.Clear(Colors.White);

            Cursor.Init();
            Cursor.Enabled = true;
    
            Cursor.Image = Framework.Graphics.Image.Load(Images.Cursors.CursorCIF);

            WindowsManager.AddWindow(600, 400, 300, 200, "Installation.");
            WindowsManager.ShowWindows();

            g.DrawString("Aura Operating System v" + Kernel.version + "-" + Kernel.revision, 0, 0, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            AddAreatoRefresh(0, 20, 69, 14); //DATETIME
            AddAreatoRefresh(0, 40, 69, 14); //FPS
            AddAreatoRefresh(0, 60, 140, 14); //MOUSEVENT

            Canvas.WriteToScreen();

        }

        public static void Final()
        {
            Cosmos.System.Power.Reboot();
        }
    }
}