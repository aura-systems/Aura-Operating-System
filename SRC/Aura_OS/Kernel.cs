/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel.cs, the main init class + main loop class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS.System;
using Aura_OS.Processing;
using Aura_OS.System.Processing;
using Aura_OS.System.Graphics;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Graphics.UI.GUI.Skin;
using Aura_OS.System.Utils;

namespace Aura_OS
{
    public class Kernel
    {
        public static Dictionary<string, string> EnvironmentVariables;
        public static string ComputerName = "aura-pc";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static bool LoggedIn = true;
        public static bool Installed = false;
        public static string Version = "0.7.4";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string Clipboard { get; internal set; }
        public static string UserDirectory { get; internal set; }

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        private static bool _networkConnected = false;
        public static bool NetworkConnected
        {
            get
            {
                if (Explorer.Taskbar != null)
                {
                    Explorer.Taskbar.MarkDirty();
                }

                return _networkConnected;
            }
            set
            {
                _networkConnected = value;

                if (Explorer.Taskbar != null)
                {
                    Explorer.Taskbar.MarkDirty();
                }
            }
        }

        public static bool NetworkTransmitting = false;

        public static bool GuiDebug = false;

        //FILES
        public static Bitmap programLogo;
        public static Bitmap errorLogo;

        public static Bitmap AuraLogo2;
        public static Bitmap AuraLogo;
        public static Bitmap AuraLogoWhite;
        public static Bitmap CosmosLogo;

        public static Bitmap wallpaper1;
        public static Bitmap wallpaper2;
        public static Bitmap auralogo_white;

        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

        //GRAPHICS
        public static uint ScreenWidth = 1920;
        public static uint ScreenHeight = 1080;

        public static Canvas Canvas;

        public static Color WhiteColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        public static int WhiteColorInt = WhiteColor.ToArgb();
        public static Color BlackColor = Color.FromArgb(0xff, 0x00, 0x00, 0x00);
        public static Color avgColPen = Color.PowderBlue;

        //WIN95 Colors
        public static Color Gray = Color.FromArgb(0xff, 0xdf, 0xdf, 0xdf);
        public static Color DarkGrayLight = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
        public static Color DarkGray = Color.FromArgb(0xff, 0x80, 0x80, 0x80);
        public static Color DarkBlue = Color.FromArgb(0xff, 0x00, 0x00, 0x80);
        public static Color Pink = Color.FromArgb(0xff, 0xe7, 0x98, 0xde);

        // Managers
        public static ProcessManager ProcessManager;
        public static System.Input.MouseManager MouseManager;
        public static System.Input.KeyboardManager KeyboardManager;
        public static ApplicationManager ApplicationManager;
        public static PackageManager PackageManager;
        public static ResourceManager ResourceManager;
        public static ThemeManager ThemeManager;
        public static Explorer Explorer;

        // Textmode Console
        public static System.Graphics.UI.CUI.Console TextmodeConsole;

        public static int FreeCount = 0;

        private static int _frameCount = 0;
        private static int _frames = 0;
        private static int _fps = 0;
        private static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem;

        public static string CommandOutput = "";
        public static bool Redirect = false;

        public static void BeforeRun()
        {
            EnvironmentVariables = new Dictionary<string, string>();
            VirtualFileSystem = new CosmosVFS();

            //Start Filesystem
            VFSManager.RegisterVFS(VirtualFileSystem);

            if (File.Exists(@"0:\System\settings.ini"))
            {
                Installed = true;

                Settings config = new Settings(@"0:\System\settings.ini");
                ScreenWidth = uint.Parse(config.GetValue("screenWidth"));
                ScreenHeight = uint.Parse(config.GetValue("screenHeight"));
            }

            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            PackageManager = new PackageManager();
            PackageManager.Initialize();

            CustomConsole.WriteLineInfo("Loading files...");
            Files.LoadFiles();

            CustomConsole.WriteLineInfo("Checking for boot.bat script...");
            for (int i = 0; i <= 5; i++)
            {
                string volumePath = i + @":\";
                if (Directory.Exists(volumePath))
                {
                    CustomConsole.WriteLineInfo($"Checking for boot.bat on volume {volumePath}...");
                    if (File.Exists(volumePath + "boot.bat"))
                    {
                        CustomConsole.WriteLineOK($"Detected boot.bat on {volumePath}, executing script...");
                        Batch.Execute(volumePath + "boot.bat");
                        CurrentVolume = volumePath;
                        break;
                    }
                }
            }

            global::System.Console.ReadKey();

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            Canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));
            Canvas.DrawImage(AuraLogoWhite, (int)((ScreenWidth / 2) - (AuraLogoWhite.Width / 2)), (int)((ScreenHeight / 2) - (AuraLogoWhite.Height / 2)));
            Canvas.Display();

            CustomConsole.BootConsole = new(0, 0, (int)ScreenWidth, (int)ScreenHeight);
            CustomConsole.BootConsole.DrawBackground = false;

            TextmodeConsole = null;

            ResourceManager = new ResourceManager();
            ResourceManager.Initialize();

            ThemeManager = new ThemeManager();
            ThemeManager.Initialize();

            ApplicationManager = new ApplicationManager();
            ApplicationManager.Initialize();

            Explorer = new Explorer();
            Explorer.Initialize();

            MouseManager = new System.Input.MouseManager();
            MouseManager.Initialize();

            KeyboardManager = new System.Input.KeyboardManager();
            KeyboardManager.Initialize();

            //Load Localization
            CustomConsole.WriteLineInfo("Initializing localization...");
            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);

            CustomConsole.WriteLineInfo("Initializing ASCII encoding...");
            global::System.Console.InputEncoding = Encoding.ASCII;
            global::System.Console.OutputEncoding = Encoding.ASCII;

            CustomConsole.WriteLineInfo("Try cleaning memory...");
            FreeCount = Heap.Collect();
            CustomConsole.WriteLineInfo("Cosmos Memory Manager works.");

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            CustomConsole.WriteLineOK("Aura Operating System boot sequence done.");

            Running = true;
        }

        public static string Debug = "";

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
                _frameCount++;

                if (_frameCount == 4)
                {
                    FreeCount = Heap.Collect();
                    _frameCount = 0;
                }

                ProcessManager.Update();

                Explorer.Screen.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhiteColorInt, 2, 0);
                Explorer.Screen.DrawString("fps=" + _fps, font, WhiteColorInt, 2, font.Height);

                if (GuiDebug)
                {
                    Explorer.Screen.DrawString(Debug, font, WhiteColorInt, 2, font.Height * 2);
                }

                Canvas.DrawImage(Explorer.Screen.Bitmap, 0, 0);
                Canvas.Display();
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
    }
}
