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

using Cosmos.HAL;

namespace Aura_OS.System.GUI.UI
{
    class Desktop
    {

        public static VbeScreen Screen = new VbeScreen();
        public static Canvas Canvas = new Canvas(800, 600);
        public static SdfFont terminus;

        static int _frames = 0;
        static int _fps = 0;
        static int _deltaT = 0;

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

        static ConsoleKeyInfo c;

        public static int Update()
        {
            c = Console.ReadKey(true);

            if (c.Key == ConsoleKey.Escape)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static void Render()
        {
            if (_deltaT != RTC.Second)
            {
                _fps = _frames;
                _frames = 0;
                _deltaT = RTC.Second;
            }

            _frames++;

            var g = new Graphics.Graphics(Canvas);

            //if (RTC.Second > 30 && !flag)
            //{
                flag = true;
                g.Clear(Colors.White);
                //g.DrawString(10, 10, "FPS: " + _fps, 50f, terminus, Colors.Black);
                //g.DrawString(10, 10 + 17, "Frames: " + _frames, 50f, terminus, Colors.Cyan);
                //g.DrawString(10, 10 + 17 + 17, "DeltaT: " + _deltaT, 50f, terminus, Colors.Orange);
                //g.DrawString(10, 10 + 17 + 17 + 17, "RTC.Second: " + RTC.Second, 50f, terminus, Colors.Purple);
            //}
            
            //var img = Image.FromBytes(MyvarLogoPng.Myvar_LogoPng, "png");
            //var img = Image.FromBytes(Images.MyvarLogoPPM.Myvar_LogoPPM, "ppm");
            //g.DrawImage(10, 10, img);

            Canvas.WriteToScreen();

            terminus = new SdfFont(Fonts.Terminus.Terminus_fnt,
                Image.FromBytes(Fonts.Terminus.Terminus_ppm, "ppm"));


            g.DrawString(10, 10, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 14f, terminus, Colors.Black);
            Canvas.WriteToScreen();
        }

        public static void Initialize()
        {
            Console.Clear();
            Screen.SetMode(VbeScreen.ScreenSize.Size800X600, VbeScreen.Bpp.Bpp32);
            Screen.Clear(Colors.Blue);


            _deltaT = RTC.Second;

            var g = new Graphics.Graphics(Canvas);
            g.Clear(Colors.LightGreen);


            Canvas.WriteToScreen();

            //terminus = new SdfFont(Fonts.Terminus.Terminus_fnt,Image.FromBytes(Fonts.Terminus.Terminus_ppm, "ppm"));
        }

        public static void Final()
        {
            Cosmos.System.Power.Reboot();
        }
    }
}
