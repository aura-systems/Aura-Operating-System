/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel.cs, the main init class + main loop class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Vfs;
using Aura_OS.System;
using Aura_OS.System.Audio;
using Aura_OS.Processing;
using Aura_OS.System.Processing;
using Aura_OS.System.Graphics;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Graphics.UI.GUI.Skin;
using Aura_OS.System.Filesystem;
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
        public static string Version = VersionInfo.version;
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string Clipboard { get; internal set; }
        public static string UserDirectory { get; internal set; }

        /// <summary>
        /// Current volume: "/N/" (the N-th FAT volume), or "/" when no volume is mounted. Always ends with '/'.
        /// </summary>
        public static string CurrentVolume = "/";

        private static string _currentDirectory = "/";

        /// <summary>
        /// Current directory, always ending with '/'. The setter also moves the BCL current directory
        /// (Lua, FTP, relative System.IO paths).
        /// </summary>
        public static string CurrentDirectory
        {
            get
            {
                return _currentDirectory;
            }
            set
            {
                _currentDirectory = value;

                try
                {
                    // GEN3-GAP(cwd): the current directory is kernel-global (VfsManager.CurrentDirectory is internal).
                    Directory.SetCurrentDirectory(value);
                }
                catch (Exception)
                {
                    // DirectoryNotFoundException when the volume is gone: keep Aura's own value.
                }
            }
        }

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

        /// <summary>
        /// The size the UI is laid out and drawn at: the display resolution (Canvas.Width/Height)
        /// divided by ScreenScale once the Explorer is up, the display resolution before that.
        /// </summary>
        public static uint ScreenWidth = 1920;
        public static uint ScreenHeight = 1080;

        /// <summary>
        /// UI scale in percent (Explorer.Scales: 100, 150 or 200), from settings.ini screenScale.
        /// At 150 and 200 the UI is drawn on a smaller canvas that Present() stretches to the display.
        /// </summary>
        public static int ScreenScale = 100;

        public static Canvas Canvas;

        /// <summary>
        /// Console.Out/Error once the canvas is acquired: keeps Console output (serial) off Aura's frame,
        /// because the KernelConsole draws on the same canvas. The Terminal swaps in its own writer
        /// while focused and must restore this one, never the original console writer.
        /// </summary>
        public static TextWriter GuiSink;

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

        public static int FreeCount = 0;

        private static int _frames = 0;
        private static int _fps = 0;
        private static long _lastFpsTick = 0;
        private static long _lastCollectTick = 0;

        public static string CommandOutput = "";
        public static bool Redirect = false;

        public static void BeforeRun()
        {
            EnvironmentVariables = new Dictionary<string, string>();

            //Start Filesystem
            CustomConsole.WriteLineInfo("Mounting volumes...");
            Volumes.Initialize();

            CurrentVolume = AuraPaths.SystemVolume ?? "/";
            CurrentDirectory = CurrentVolume;

            if (AuraPaths.SystemVolume != null && File.Exists(AuraPaths.SettingsIni))
            {
                Installed = true;

                Settings config = new Settings(AuraPaths.SettingsIni);

                uint width;
                if (uint.TryParse(config.GetValue("screenWidth"), out width) && width > 0)
                {
                    ScreenWidth = width;
                }

                uint height;
                if (uint.TryParse(config.GetValue("screenHeight"), out height) && height > 0)
                {
                    ScreenHeight = height;
                }

                // Checked against the display once the canvas exists (Explorer falls back to 100).
                int scale;
                if (int.TryParse(config.GetValue("screenScale"), out scale))
                {
                    ScreenScale = scale;
                }
            }

            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            PackageManager = new PackageManager();
            PackageManager.Initialize();

            CustomConsole.WriteLineInfo("Loading files...");
            Files.LoadFiles();

            CustomConsole.WriteLineInfo("Checking for boot.bat script...");
            IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts; // immutable snapshot
            for (int i = 0; i < mounts.Count; i++)
            {
                string volumePath = AuraPath.AsDirectory(mounts[i].MountPoint);
                CustomConsole.WriteLineInfo($"Checking for boot.bat on volume {volumePath}...");
                if (File.Exists(volumePath + "boot.bat"))
                {
                    CustomConsole.WriteLineOK($"Detected boot.bat on {volumePath}, executing script...");
                    Batch.Execute(volumePath + "boot.bat");
                    CurrentVolume = volumePath;
                    break;
                }
            }

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            try
            {
                // Display that can switch modes (VMware SVGA II): switches. Firmware framebuffer / virtio-gpu:
                // ignored, the resolution is the one limine.conf asked for.
                Canvas = Canvas.GetFullScreen(new Mode((int)ScreenWidth, (int)ScreenHeight, ColorDepth.ColorDepth32));
            }
            catch (ArgumentOutOfRangeException)
            {
                // Mode refused by the display (screenWidth/screenHeight come from a user-editable settings.ini).
                Canvas = Canvas.GetFullScreen();
            }

            // GEN3-GAP(display-mode): the real framebuffer size from here on.
            ScreenWidth = (uint)Canvas.Width;
            ScreenHeight = (uint)Canvas.Height;

            // GEN3-GAP(kernelconsole): no public way to detach or hide the KernelConsole, which draws on this
            // same canvas; redirecting Console.Out/Error keeps it from painting over the GUI.
            GuiSink = new SerialTextWriter();
            global::System.Console.SetOut(GuiSink);
            global::System.Console.SetError(GuiSink);

            // DrawImage blends the logo by its alpha: clear the kernel console text from under it first.
            Canvas.Clear(BlackColor);
            Canvas.DrawImage(AuraLogoWhite, (Canvas.Width - AuraLogoWhite.Width) / 2, (Canvas.Height - AuraLogoWhite.Height) / 2);
            Present();

            // The boot console keeps its black background: Canvas.DrawCanvas blends, so a transparent
            // console would leave the previous lines under the new ones.
            CustomConsole.BootConsole = new(0, 0, (int)ScreenWidth, (int)ScreenHeight);

            ResourceManager = new ResourceManager();
            ResourceManager.Initialize();

            ThemeManager = new ThemeManager();
            ThemeManager.Initialize();

            ApplicationManager = new ApplicationManager();
            ApplicationManager.Initialize();

            // The Explorer constructor (LoginScreen.Hide()) writes MouseManager.FocusedComponent:
            // construct the input managers first (a null dereference is a fatal #PF in gen3).
            MouseManager = new System.Input.MouseManager();
            KeyboardManager = new System.Input.KeyboardManager();

            Explorer = new Explorer();
            Explorer.Initialize();

            // Process order stays Explorer -> Mouse (cursor drawn on top) -> Keyboard.
            MouseManager.Initialize();
            KeyboardManager.Initialize();

            if (!string.IsNullOrEmpty(ComputerName))
            {
                Cosmos.Kernel.System.Network.Config.DnsConfig.HostName = ComputerName;
            }

            CustomConsole.WriteLineInfo("Try cleaning memory...");
            FreeCount = MemoryInfo.Collect();
            CustomConsole.WriteLineInfo("Cosmos Memory Manager works.");

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            CustomConsole.WriteLineOK("Aura Operating System boot sequence done.");

            // On its own thread: plays while the first frames (desktop, or login screen) draw.
            Sounds.PlayBoot();

            _lastFpsTick = Environment.TickCount64;
            _lastCollectTick = _lastFpsTick;

            Running = true;
        }

        public static string Debug = "";

        public static void Run()
        {
            try
            {
                long now = Environment.TickCount64;

                if (now - _lastFpsTick >= 1000)
                {
                    _fps = _frames;
                    _frames = 0;
                    _lastFpsTick = now;

                    // GEN3-GAP(mounts): no hot-plug event, poll once per second.
                    Volumes.PollHotplug();
                }

                _frames++;

                // GEN3-GAP(gc-trigger): OrionGC only collects on page-allocator exhaustion.
                // 1 Hz, 10 Hz under memory pressure (rate-limited: a collection is stop-the-world).
                bool lowMemory = MemoryInfo.FreePages < MemoryInfo.TotalPages / 4;
                if (now - _lastCollectTick >= (lowMemory ? 100 : 1000))
                {
                    FreeCount = MemoryInfo.Collect();
                    _lastCollectTick = now;
                }

                ProcessManager.Update();

                Explorer.Screen.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhiteColor, 2, 0);
                Explorer.Screen.DrawString("fps=" + _fps, font, WhiteColor, 2, font.Height);

                if (GuiDebug && Debug != null)
                {
                    Explorer.Screen.DrawString(Debug, font, WhiteColor, 2, font.Height * 2);
                }

                Present();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Crash.StopKernel(ex.Message, ex.InnerException.Message, "", "0");
                }
                else
                {
                    Crash.StopKernel("Fatal dotnet exception occured.", ex.Message, "", "0");
                }
            }
        }

        /// <summary>
        /// Shows the frame. Explorer.Screen is the full-screen canvas, so this is a single
        /// Canvas.Display(); an off-screen screen canvas (if any) is stretched onto it first.
        /// </summary>
        public static void Present()
        {
            if (Canvas == null)
            {
                // Before BeforeRun acquired the screen (a null dereference is a fatal #PF in gen3).
                return;
            }

            Canvas screen = Explorer != null ? Explorer.Screen : null;
            if (screen != null && screen != Canvas)
            {
                Canvas.DrawCanvas(screen, 0, 0, Canvas.Width, Canvas.Height);
            }

            Canvas.Display();
        }
    }
}
