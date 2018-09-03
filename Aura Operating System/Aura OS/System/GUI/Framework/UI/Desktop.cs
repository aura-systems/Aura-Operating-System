/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/
 using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.GUI;
using Aura_OS.System.GUI.Graphics;
using Aura_OS.System.GUI.Imaging;
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
        static Util.Point mouse_click;
        static Dictionary<string, Util.Area> clickabled_area = new Dictionary<string, Util.Area>();
        private static bool flag = false;
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

         //public static Image cursor;
        public static Graphics.Graphics g;
        public static bool firsttime = true;
        public static string hour;
        public static void Render()
        {

            g.Clear(Colors.White);

            if (_deltaT != RTC.Second)
            {
                _fps = _frames;
                _frames = 0;
                _deltaT = RTC.Second;
            }
            _frames++;

            g.DrawString("Aura Operating System v" + Kernel.version + "-" + Kernel.revision, 0, 0, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            g.DrawString(Time.TimeString(true, true, true), 0, 20, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            g.DrawString("FPS: " + _fps, 0, 40, Colors.Black, Fonts.CFF._Pixel6_Mini_cff);

            Cursor.Render();

            Canvas.WriteToScreen();
        }

        public static bool click_contained_in_area(Area CloseArea, Util.Point ClickPoint)
        {
            return true;
        }

        public static void Initialize()
        {
            
            Shell.VESAVBE.Graphics.Clear(Colors.Blue.ToHex());

            g = new Graphics.Graphics(Canvas);
            g.Clear(Colors.White);

            Cursor.Init();
            Cursor.Enabled = true;
    
            Cursor.Image = Framework.Graphics.Image.Load(Images.Cursors.CursorCIF);

            Canvas.WriteToScreen();

        }

        public static void Final()
        {
            Cosmos.System.Power.Reboot();
        }
    }
}