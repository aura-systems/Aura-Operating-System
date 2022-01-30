using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Aura_OS.System.Graphics;
using Aura_OS.System.Shell.cmdIntr;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS.Processing;
using System.Drawing;
using Aura_OS.System;

namespace Aura_OS
{
    public class Kernel
    {
        public static string ComputerName = "aura-pc";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static string Version = "0.5.3";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        //FILES
        public static Bitmap programIco;
        public static Bitmap terminalIco;

        public static Bitmap programlogo;
        public static Bitmap bootBitmap;
        public static Bitmap cursor;
        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

        //GRAPHICS
        public static uint screenWidth = 1024;
        public static uint screenHeight = 768;

        public static Canvas canvas;
        public static Pen WhitePen = new Pen(Color.White);
        public static Pen BlackPen = new Pen(Color.Black);
        public static Pen avgColPen = new Pen(Color.DimGray);
        public static Dock dock;

        //PROCESSES
        public static List<App> apps;
        public static ProcessManager ProcessManager;
        public static Terminal console;
        public static SystemInfo systeminfo;
        public static CommandManager CommandManager;

        public static bool Pressed;
        public static int FreeCount = 0;

        private static int _frames = 0;
        public static int _fps = 0;
        private static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem = new CosmosVFS();
        public static Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();

        public static void BeforeRun()
        {
            //Start Filesystem
            //VFSManager.RegisterVFS(VirtualFileSystem);

            //Load Localization
            System.CustomConsole.WriteLineInfo("Initializing localization...");

            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
            KeyboardManager.SetKeyLayout(new Sys.ScanMaps.FR_Standard());

            System.CustomConsole.WriteLineInfo("Loading files...");

            //LOAD FILES

            programIco = new Bitmap(Convert.FromBase64String(Files.b64NoIcon));

            System.CustomConsole.WriteLineOK("Program icon.");

            terminalIco = new Bitmap(Convert.FromBase64String(Files.b64TerminalIcon));

            System.CustomConsole.WriteLineOK("Terminal icon.");

            bootBitmap = new Bitmap(Convert.FromBase64String(Files.b64AuraLogo));

            System.CustomConsole.WriteLineOK("Aura logo.");

            programlogo = new Bitmap(Convert.FromBase64String(Files.b64ProgramIcon));

            System.CustomConsole.WriteLineOK("Program icon 2.");

            cursor = new Bitmap(Convert.FromBase64String(Files.b64cursorIcon));

            System.CustomConsole.WriteLineOK("Cursor.");

            font = PCScreenFont.LoadFont(Convert.FromBase64String(Files.b64font));

            System.CustomConsole.WriteLineOK("Font.");

            System.CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode((int)screenWidth, (int)screenHeight, ColorDepth.ColorDepth32));
            dock = new Dock();

            //START PROCESSES
            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            CommandManager = new CommandManager();
            CommandManager.Initialize();

            console = new Terminal(700, 600, 40, 40);
            console.Initialize();

            systeminfo = new SystemInfo(400, 300, 40, 40);
            systeminfo.Initialize();

            apps = new List<App>();
            apps.Add(console);
            apps.Add(systeminfo);

            //START MOUSE
            MouseManager.ScreenWidth = screenWidth;
            MouseManager.ScreenHeight = screenHeight;

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            Running = true;
        }

        public static void Run()
        {
            try
            {
                if (_deltaT != RTC.Second)
                {
                    _fps = _frames;
                    _frames = 0;
                    _deltaT = RTC.Second;
                }

                _frames++;

                FreeCount = Heap.Collect();

                switch (MouseManager.MouseState)
                {
                    case MouseState.Left:
                        Pressed = true;
                        break;
                    case MouseState.None:
                        Pressed = false;
                        break;
                }

                canvas.Clear(0x000000);

                canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

                canvas.DrawString("fps=" + _fps, font, WhitePen, 2, (int)screenHeight - (font.Height * 2));
                canvas.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhitePen, 2, (int)screenHeight - font.Height);

                //Global.mDebugger.Send("");

                foreach (App app in apps)
                    app.Update();

                dock.Update();

                DrawCursor(MouseManager.X, MouseManager.Y);

                canvas.Display();
            }
            catch (Exception ex)
            {
                System.Crash.WriteException(ex);
            }
        }

        public static void DrawCursor(uint x, uint y)
        {
            canvas.DrawImageAlpha(cursor, (int)x, (int)y);
        }
    }
}
