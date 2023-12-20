/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel.cs, the main init class + main loop class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
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
using Aura_OS.Interpreter;
using Aura_OS.System.UI.GUI;
using Aura_OS.System.Application.Emulators.GameBoyEmu;
using System.IO;
using Aura_OS.System.Processing;

namespace Aura_OS
{
    public class Kernel
    {
        public static string ComputerName = "aura-pc";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static string Version = "0.6.0";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        public static bool NetworkConnected = false;
        public static bool NetworkTransmitting = false;

        //FILES
        public static Bitmap programIco;
        public static Bitmap terminalIco;
        public static Bitmap powerIco;
        public static Bitmap connectedIco;
        public static Bitmap networkTransmitIco;
        public static Bitmap networkIdleIco;
        public static Bitmap networkOfflineIco;
        public static Bitmap directoryIco;
        public static Bitmap fileIco;
        public static Bitmap rebootIco;
        public static Bitmap shutdownIco;
        public static Bitmap terminalIco2;
        public static Bitmap programIco2;

        public static Bitmap programLogo;
        public static Bitmap errorLogo;

        public static Bitmap AuraLogo2;
        public static Bitmap AuraLogo;
        public static Bitmap CosmosLogo;

        public static Bitmap wallpaper;

        public static Bitmap cursor;
        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

        // UI
        public static Bitmap CloseNormal;
        public static Bitmap Start;

        //GRAPHICS
        public static uint screenWidth = 1920;
        public static uint screenHeight = 1080;

        public static Canvas canvas;
        public static Color WhiteColor = Color.White;
        public static Color BlackColor = Color.Black;
        public static Color avgColPen = Color.PowderBlue;

        //WIN95 Colors
        public static Color Gray = Color.FromArgb(0xff, 0xdf, 0xdf, 0xdf);
        public static Color DarkGrayLight = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
        public static Color DarkGray = Color.FromArgb(0xff, 0x80, 0x80, 0x80);
        public static Color DarkBlue = Color.FromArgb(0xff, 0x00, 0x00, 0x80);
        public static Color Pink = Color.FromArgb(0xff, 0xe7, 0x98, 0xde);

        public static Dock dock;

        //PROCESSES
        public static ProcessManager ProcessManager;
        public static WindowManager WindowManager;
        public static System.UI.CUI.Console aConsole;

        public static Terminal console;
        public static MemoryInfo memoryInfo;
        public static SystemInfo systemInfo;
        public static GameBoyEmu gameBoyEmu;
        public static Cube cube;

        public static CommandManager CommandManager;

        public static PackageManager PackageManager;

        public static bool Pressed;
        public static int FreeCount = 0;

        private static int _frames = 0;
        public static int _fps = 0;
        public static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem = new CosmosVFS();
        public static Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();

        public static void BeforeRun()
        {
            //Start Filesystem
            VFSManager.RegisterVFS(VirtualFileSystem);

            //Load Localization
            CustomConsole.WriteLineInfo("Initializing localization...");

            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
            KeyboardManager.SetKeyLayout(new Sys.ScanMaps.USStandardLayout());

            //START PROCESSES
            CustomConsole.WriteLineInfo("Starting process manager...");
            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            LoadFiles();

            CustomConsole.WriteLineInfo("Starting dock...");
            dock = new Dock();
            dock.Initialize();

            CustomConsole.WriteLineInfo("Starting command manager...");
            CommandManager = new CommandManager();
            CommandManager.Initialize();

            if (File.Exists(CurrentDirectory + "boot.bat"))
            {
                CustomConsole.WriteLineInfo("Detected boot.bat, executing script...");

                Batch.Execute(CurrentDirectory + "boot.bat");
            }

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(screenWidth, screenHeight, ColorDepth.ColorDepth32));

            aConsole = null;

            console = new Terminal(700, 600, 40, 40);
            console.Initialize();

            memoryInfo = new MemoryInfo(400, 300, 40, 40);
            memoryInfo.Initialize();

            systemInfo = new SystemInfo(402, 360, 40, 40);
            systemInfo.Initialize();

            cube = new Cube(200, 200, 40, 40);
            cube.Initialize();

            gameBoyEmu = new GameBoyEmu(160 + 4, 144 + 22, 40, 40);
            gameBoyEmu.Initialize();

            dock.UpdateApplicationButtons();

            WindowManager = new WindowManager();
            WindowManager.Initialize();

            PackageManager = new PackageManager();
            PackageManager.Initialize();

            //START MOUSE
            MouseManager.ScreenWidth = screenWidth;
            MouseManager.ScreenHeight = screenHeight;

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            Running = true;
        }

        public static void LoadFiles()
        {
            System.CustomConsole.WriteLineInfo("Loading files...");

            var vols = VirtualFileSystem.GetVolumes();
            string isoVol = CurrentVolume;

            foreach (var vol in vols)
            {
                if (VirtualFileSystem.GetFileSystemType(vol.mName).Equals("ISO9660"))
                {
                    isoVol = vol.mName;

                    CustomConsole.WriteLineOK("ISO9660 vol is " + isoVol);
                }
            }

            // LOAD FILE
            errorLogo = new Bitmap(Files.ErrorImage);
            CustomConsole.WriteLineOK("error.bmp image loaded.");

            // Wallpapers
            wallpaper = new Bitmap(Files.Wallpaper);
            CustomConsole.WriteLineOK("wallpaper-1.bmp wallpaper loaded.");

            // Images
            AuraLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\AuraLogo.bmp"));
            CustomConsole.WriteLineOK("AuraLogo.bmp image loaded.");

            AuraLogo2 = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\aura.bmp"));
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            CosmosLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\CosmosLogo.bmp"));
            CustomConsole.WriteLineOK("CosmosLogo.bmp image loaded.");

            // Fonts
            font = PCScreenFont.LoadFont(File.ReadAllBytes(isoVol + "UI\\Fonts\\zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");

            Heap.Collect();

            // Icons
            CloseNormal = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\close_normal.bmp"));
            CustomConsole.WriteLineOK("close_normal.bmp icon loaded.");

            Start = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\start.bmp"));
            CustomConsole.WriteLineOK("start.bmp icon loaded.");

            terminalIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\utilities-terminal.bmp"));
            CustomConsole.WriteLineOK("utilities-terminal.bmp icon loaded.");

            programIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\program.bmp"));
            CustomConsole.WriteLineOK("program.bmp icon loaded.");

            cursor = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\cursor.bmp"));
            CustomConsole.WriteLineOK("cursor.bmp icon loaded.");

            networkTransmitIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\network-transmit.bmp"));
            CustomConsole.WriteLineOK("network-transmit.bmp icon loaded.");

            networkIdleIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\network-idle.bmp"));
            CustomConsole.WriteLineOK("network-idle.bmp icon loaded.");

            networkOfflineIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\network-offline.bmp"));
            CustomConsole.WriteLineOK("network-offline.bmp icon loaded.");

            directoryIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\folder.bmp"));
            CustomConsole.WriteLineOK("folder.bmp icon loaded.");

            fileIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\document-new.bmp"));
            CustomConsole.WriteLineOK("document-new.bmp icon loaded.");

            Heap.Collect();

            rebootIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\system-reboot.bmp"));
            CustomConsole.WriteLineOK("system-reboot.bmp icon loaded.");

            shutdownIco = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\system-shutdown.bmp"));
            CustomConsole.WriteLineOK("system-shutdown.bmp icon loaded.");

            programIco2 = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\window-big.bmp"));
            CustomConsole.WriteLineOK("window-big.bmp icon loaded.");

            terminalIco2 = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\Icons\\console-24.bmp"));
            CustomConsole.WriteLineOK("console-24.bmp icon loaded.");
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

                //canvas.Clear(0x000000);

                canvas.DrawImage(wallpaper, 0, 0);

                //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

                canvas.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhiteColor, 2, 0);
                canvas.DrawString("fps=" + _fps, font, WhiteColor, 2, font.Height);

                if (VirtualFileSystem != null && VirtualFileSystem.GetVolumes().Count > 0)
                {
                    DrawDesktopItems();
                }

                ProcessManager.Update();

                DrawCursor(MouseManager.X, MouseManager.Y);

                canvas.Display();
            }
            catch (Exception ex)
            {
                System.Crash.WriteException(ex);
            }
        }

        public static void DrawDesktopItems()
        {
            int startX = 10;
            int startY = 40;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            string[] directories = Directory.GetDirectories(Kernel.CurrentVolume);
            string[] files = Directory.GetFiles(Kernel.CurrentVolume);

            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                DrawIconAndText(directoryIco, folderName, currentX, currentY);

                currentY += iconSpacing;
                if (currentY + iconSpacing > screenHeight - 64)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                DrawIconAndText(fileIco, fileName, currentX, currentY);

                currentY += iconSpacing;
                if (currentY + iconSpacing > screenHeight - 64)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }
        }

        private static void DrawIconAndText(Bitmap bitmap, string text, int x, int y)
        {
            canvas.DrawImageAlpha(bitmap, x + 6, y);
            canvas.DrawString(text, font, WhiteColor, x, y + 35);
        }


        public static void DrawCursor(uint x, uint y)
        {
            canvas.DrawImageAlpha(cursor, (int)x, (int)y);
        }
    }
}
