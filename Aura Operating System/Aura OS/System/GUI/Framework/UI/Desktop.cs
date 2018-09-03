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

        public static Canvas Canvas = new Canvas(1366, 768);
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
            if (_deltaT != RTC.Second)
            {
                _fps = _frames;
                _frames = 0;
                _deltaT = RTC.Second;
                
                if (_deltaTM != RTC.Minute)
                {
                    hour = Time.TimeString(true, true, false);
                    g.FillRectangle(1295, 738, 86, 30, Colors.LightBlue);
                    Desktop.g.DrawString(hour, 1295, 744, Colors.Black, Fonts.CFF._Pixel7_Mini_cff);
                    _deltaTM = RTC.Minute;
                }
            }
             //if (mouse_click != null)
            //{
            //    foreach (KeyValuePair<string, Util.Area> entry in clickabled_area)
            //    {
            //        foreach (Window win in WindowsManager.Active_Windows)
            //        {
            //            if (click_contained_in_area(win.CloseArea, mouse_click))
            //            {
            //                WindowsManager.Active_Windows.Remove(win);
            //            }
            //        }
            //    }
            //    mouse_click = null;
            //}
             _frames++;
             //g = new Graphics.Graphics(Canvas);
             //if (RTC.Second > 30 && !flag)
            //{
            //flag = true;
             //g.Clear(Colors.White);
             //g.DrawString(10, 10, "FPS: " + _fps, 50f, terminus, Colors.Black);
            //g.DrawString(10, 10 + 17, "Frames: " + _frames, 50f, terminus, Colors.Cyan);
            //g.DrawString(10, 10 + 17 + 17, "DeltaT: " + _deltaT, 50f, terminus, Colors.Orange);
            //g.DrawString(10, 10 + 17 + 17 + 17, "RTC.Second: " + RTC.Second, 50f, terminus, Colors.Purple);
            //}
             Desktop.g.FillRectangle(10, 500, 100, 20, Colors.White);
            Desktop.g.DrawString("FPS: " + _fps, 10, 500, Colors.Black, Fonts.CFF._Pixel7_Mini_cff);
             
            //g.DrawString(10, 500, "FPS: " + _fps, 14f, terminus, Colors.Black);
             Cursor.Render();
             Canvas.WriteToScreen();
             firsttime = false;
            //terminus = new SdfFont(Fonts.Terminus.Terminus_fnt,
            //    Image.FromBytes(Fonts.Terminus.Terminus_ppm, "ppm"));
             //g.DrawString(10, 10, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 14f, terminus, Colors.Black);
            //Canvas.WriteToScreen();
        }
         public static bool click_contained_in_area(Area CloseArea, Util.Point ClickPoint)
        {
            return true;
        }
         public static void Initialize()
        {
            Console.Clear();

            Shell.VESAVBE.Graphics.Clear(Colors.Blue.ToHex());
             //_deltaT = RTC.Second;
             g = new Graphics.Graphics(Canvas);
            g.Clear(Colors.White);
             
             Cursor.Init();
            Cursor.Enabled = true;
             g.FillRectangle(0, 738, 1366, 30, Colors.LightBlue);
             WindowsManager.AddWindow(300, 300, 100, 100, "Test Window.");
            WindowsManager.ShowWindows();
             //if (windows)
            //{
            //    g.FillRectangle(0, 288, 250, 450, Colors.LightBlue);
            //}
             //var img = Image.FromBytes(Images.CosmosLogoPPM.Cosmos_LogoPPM, "ppm");
            //g.DrawImage(10, 10, img);
             //Cursor.Image = Framework.Graphics.Image.Load(Images.Cursors.Normal_CIF);
             //terminus = new SdfFont(Fonts.Terminus.Terminus_fnt,
            //    Image.FromBytes(Fonts.Terminus.Terminus_ppm, "ppm"));
             //g.DrawString(10, 10, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 14f, terminus, Colors.Black);
             Canvas.WriteToScreen();
             //cursor = Image.FromBytes(Images.Cursors.Cif, "cif");
        }
         public static void Final()
        {
            Cosmos.System.Power.Reboot();
        }
    }
}