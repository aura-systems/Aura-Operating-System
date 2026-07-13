/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel.cs, the main init class + main loop class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
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

        // gen3 uses Unix-style mount-point paths ("/mnt") instead of gen2 drive letters ("0:\").
        public const string RootVolume = "/mnt";
        public static string CurrentVolume = RootVolume + "/";
        public static string CurrentDirectory = RootVolume + "/";

        /// <summary>True once a FAT volume is mounted at <see cref="RootVolume"/>.</summary>
        public static bool VolumeMounted = false;

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
        // GEN3-GAP(textmode): gen3 is UEFI/GOP only, no VGA text mode; the CUI console is
        // retargeted at the graphical KernelConsole and this stays null unless explicitly used.
        public static System.Graphics.UI.CUI.Console TextmodeConsole;

        public static int FreeCount = 0;

        private static int _frameCount = 0;
        private static int _frames = 0;
        private static int _fps = 0;
        private static int _deltaT = 0;

        public static string CommandOutput = "";
        public static bool Redirect = false;

        public static void BeforeRun()
        {
            EnvironmentVariables = new Dictionary<string, string>();

            //Start Filesystem
            // gen2: new CosmosVFS() + VFSManager.RegisterVFS (auto-mounted every volume as N:\).
            // gen3: register the FAT driver and mount partition 0 at /mnt explicitly.
            if (VfsManager.RegisterFilesystem("fat", new FatFilesystemType()))
            {
                if (StorageManager.Partitions.Count > 0 &&
                    VfsManager.TryMount("fat", "0", MountFlags.None, RootVolume, out _))
                {
                    VolumeMounted = true;
                    CustomConsole.WriteLineOK("FAT volume mounted on " + RootVolume);
                }
                else
                {
                    // GEN3-GAP(iso9660): without a FAT disk attached there is no filesystem at all —
                    // gen2 could always fall back to reading the boot ISO (ISO9660), gen3 cannot.
                    CustomConsole.WriteLineInfo("No FAT volume found, running without persistent storage.");
                }
            }

            if (VolumeMounted && File.Exists(RootVolume + @"/System/settings.ini"))
            {
                Installed = true;

                Settings config = new Settings(RootVolume + @"/System/settings.ini");
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
            foreach (var mount in VfsManager.Mounts)
            {
                string volumePath = mount.MountPoint + "/";
                if (File.Exists(volumePath + "boot.bat"))
                {
                    CustomConsole.WriteLineOK($"Detected boot.bat on {volumePath}, executing script...");
                    Batch.Execute(volumePath + "boot.bat");
                    CurrentVolume = volumePath;
                    break;
                }
            }

            global::System.Console.ReadKey();

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            Canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));

            // GEN3-GAP(video-mode): GetFullScreenCanvas(Mode) silently ignores the requested mode —
            // the resolution is whatever Limine negotiated at boot. Read back the real values so the
            // whole UI sizes itself correctly (request a mode via Bootloader/limine.conf instead).
            ScreenWidth = Canvas.Mode.Width;
            ScreenHeight = Canvas.Mode.Height;

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

            // GEN3-GAP(encoding): CosmosEncodingProvider / Console.InputEncoding/OutputEncoding have
            // no gen3 equivalent (InvariantGlobalization; KernelConsole consumes UTF-16 directly).

            // gen3 has a real tracing GC — no manual Heap.Collect() needed at boot or per frame.

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            CustomConsole.WriteLineOK("Aura Operating System boot sequence done.");

            Running = true;
        }

        public static string Debug = "";

        public static void Run()
        {
            try
            {
                int second = DateTime.Now.Second;
                if (_deltaT != second)
                {
                    _fps = _frames;
                    _frames = 0;
                    _deltaT = second;
                }

                _frames++;
                _frameCount++;

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
