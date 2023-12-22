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
using Aura_OS.System.Application.Emulators.GameBoyEmu;
using System.IO;
using Aura_OS.System.Processing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics;
using Aura_OS.System.Application;

namespace Aura_OS
{
    public class Kernel
    {
        public static Dictionary<string, string> EnvironmentVariables;
        public static string ComputerName = "aura-pc";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static string Version = "0.7.0";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        public static bool NetworkConnected = false;
        public static bool NetworkTransmitting = false;

        //FILES
        public static Bitmap programLogo;
        public static Bitmap errorLogo;

        public static Bitmap AuraLogo2;
        public static Bitmap AuraLogo;
        public static Bitmap AuraLogoWhite;
        public static Bitmap CosmosLogo;

        public static Bitmap wallpaper;

        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

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

        public static Taskbar dock;

        // Managers
        public static ProcessManager ProcessManager;
        public static WindowManager WindowManager;
        public static ApplicationManager ApplicationManager;
        public static CommandManager CommandManager;
        public static PackageManager PackageManager;

        // Textmode Console
        public static System.Graphics.UI.CUI.Console aConsole;

        // Console application
        public static Terminal console;

        public static bool Pressed;
        public static int FreeCount = 0;

        private static int _frames = 0;
        public static int _fps = 0;
        public static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem;

        public static void BeforeRun()
        {
            EnvironmentVariables = new Dictionary<string, string>();
            VirtualFileSystem = new CosmosVFS();

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

            CustomConsole.WriteLineInfo("Loading files...");
            LoadFiles();

            CustomConsole.WriteLineInfo("Starting command manager...");
            CommandManager = new CommandManager();
            CommandManager.Initialize();

            CustomConsole.WriteLineInfo("Checking for boot.bat script...");
            if (File.Exists(CurrentDirectory + "boot.bat"))
            {
                CustomConsole.WriteLineOK("Detected boot.bat, executing script...");

                Batch.Execute(CurrentDirectory + "boot.bat");
            }

            CustomConsole.WriteLineInfo("Starting package manager...");
            PackageManager = new PackageManager();
            PackageManager.Initialize();

            CustomConsole.WriteLineInfo("Starting application manager...");
            ApplicationManager = new ApplicationManager();
            LoadApplications();

            CustomConsole.WriteLineInfo("Starting window manager...");
            WindowManager = new WindowManager();

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(screenWidth, screenHeight, ColorDepth.ColorDepth32));
            canvas.DrawImage(AuraLogoWhite, (int)((screenWidth / 2) - (AuraLogoWhite.Width / 2)), (int)((screenHeight / 2) - (AuraLogoWhite.Height / 2)));
            canvas.Display();

            aConsole = null;

            // CustomConsole.WriteLineInfo("Starting dock...");
            dock = new Taskbar();
            dock.Initialize();

            dock.UpdateApplicationButtons();

            //START MOUSE
            MouseManager.ScreenWidth = screenWidth;
            MouseManager.ScreenHeight = screenHeight;

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            Running = true;
        }

        public static void LoadApplications()
        {
            System.CustomConsole.WriteLineInfo("Registering applications...");

            ApplicationManager.RegisterApplication(typeof(Terminal), 40, 40, 700, 600);
            ApplicationManager.RegisterApplication(typeof(MemoryInfo), 40, 40, 400, 300);
            ApplicationManager.RegisterApplication(typeof(Explorer), 40, 40, 500, 400);
            ApplicationManager.RegisterApplication(typeof(SystemInfo), 40, 40, 402, 360);
            ApplicationManager.RegisterApplication(typeof(Cube), 40, 40, 200, 200);
            ApplicationManager.RegisterApplication(typeof(GameBoyEmu), 40, 40, 160 + 4, 144 + 22);
        }

        public static void LoadFiles()
        {
            CustomConsole.WriteLineInfo("Checking for ISO9660 volume...");

            var vols = VirtualFileSystem.GetVolumes();
            string isoVol = CurrentVolume;

            foreach (var vol in vols)
            {
                if (VirtualFileSystem.GetFileSystemType(vol.mName).Equals("ISO9660"))
                {
                    isoVol = vol.mName;

                    CustomConsole.WriteLineOK("Volume is " + isoVol);
                }
            }

            // Files
            errorLogo = new Bitmap(Files.ErrorImage);
            CustomConsole.WriteLineOK("error.bmp image loaded.");

            // Wallpapers
            wallpaper = new Bitmap(Files.Wallpaper);
            CustomConsole.WriteLineOK("wallpaper-1.bmp wallpaper loaded.");

            // Images
            AuraLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\AuraLogo.bmp"));
            CustomConsole.WriteLineOK("AuraLogo.bmp image loaded.");

            AuraLogoWhite = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\AuraLogoWhite.bmp"));
            CustomConsole.WriteLineOK("AuraLogoWhite.bmp image loaded.");

            AuraLogo2 = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\aura.bmp"));
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            CosmosLogo = new Bitmap(File.ReadAllBytes(isoVol + "UI\\Images\\CosmosLogo.bmp"));
            CustomConsole.WriteLineOK("CosmosLogo.bmp image loaded.");

            // Fonts
            font = PCScreenFont.LoadFont(File.ReadAllBytes(isoVol + "UI\\Fonts\\zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");

            // Icons
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\up.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\close.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\minimize.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-idle.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-offline.bmp", "16");
            //LoadImage(isoVol + "UI\\Images\\Icons\\16\\network-transmit.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\program.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\terminal.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\drive.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\16\\drive-readonly.bmp", "16");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\program.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\reboot.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\shutdown.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\terminal.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\24\\explorer.bmp", "24");
            LoadImage(isoVol + "UI\\Images\\Icons\\32\\file.bmp", "32");
            LoadImage(isoVol + "UI\\Images\\Icons\\32\\folder.bmp", "32");
            LoadImage(isoVol + "UI\\Images\\Icons\\cursor.bmp", "00");
            LoadImage(isoVol + "UI\\Images\\Icons\\start.bmp", "00");
        }

        public static void LoadImage(string path, string type)
        {
            string fileName = Path.GetFileName(path);
            Bitmap bitmap = new(File.ReadAllBytes(path));
            ResourceManager.Icons.Add(type + "-" + fileName, bitmap);
            CustomConsole.WriteLineOK(fileName + " icon loaded.");
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

                UpdateUI();

                canvas.Display();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Crash.StopKernel(ex.Message, ex.InnerException.Message, "0x00000000", "0");
                }
                else
                {
                    Crash.StopKernel("Fatal dotnet exception occured.", ex.Message, "0x00000000", "0");
                }
            }
        }

        private static void UpdateUI()
        {
            //canvas.Clear(0x000000);

            canvas.DrawImage(wallpaper, 0, 0);

            //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

            canvas.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhiteColor, 2, 0);
            canvas.DrawString("fps=" + _fps, font, WhiteColor, 2, font.Height);

            try
            {
                if (VirtualFileSystem != null && VirtualFileSystem.GetVolumes().Count > 0)
                {
                    DrawDesktopItems();
                }
            }
            catch (Exception ex)
            {
                Crash.StopKernel("Fatal dotnet exception occured while drawing dekstop items.", ex.Message, "0x00000000", "0");
            }

            try
            {
                dock.Update();
            }
            catch (Exception ex)
            {
                Crash.StopKernel("Fatal dotnet exception occured while drawing taskbar.", ex.Message, "0x00000000", "0");
            }

            try
            {
                WindowManager.UpdateWindowStack();
                WindowManager.DrawWindows();
            }
            catch (Exception ex)
            {
                Crash.StopKernel("Fatal dotnet exception occured while drawing windows.", ex.Message, "0x00000000", "0");
            }

            DrawCursor(MouseManager.X, MouseManager.Y);
        }

        public static void DrawDesktopItems()
        {
            int startX = 10;
            int startY = 40;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            string[] directories = Directory.GetDirectories(CurrentVolume);
            string[] files = Directory.GetFiles(CurrentVolume);

            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                DrawIconAndText(ResourceManager.GetImage("32-folder.bmp"), folderName, currentX, currentY);

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
                DrawIconAndText(ResourceManager.GetImage("32-file.bmp"), fileName, currentX, currentY);

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
            canvas.DrawImageAlpha(ResourceManager.GetImage("00-cursor.bmp"), (int)x, (int)y);
        }
    }
}
