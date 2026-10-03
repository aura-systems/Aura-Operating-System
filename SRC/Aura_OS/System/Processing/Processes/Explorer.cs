/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Explorer process
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.Processing;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Mouse;

namespace Aura_OS.System.Processing.Processes
{
    public class Explorer : Process
    {
        public static bool ShowStartMenu
        {
            get
            {
                return _showStartMenu;
            }
            set
            {
                _showStartMenu = value;
                StartMenu.Visible = _showStartMenu;

                if (StartMenu.Visible)
                {
                    WindowManager.BringToFront(StartMenu);
                }
            }
        }

        public static Taskbar Taskbar;
        public static StartMenu StartMenu;
        public static Desktop Desktop;
        public static LoginScreen Login;
        public static WindowManager WindowManager = new WindowManager();

        /// <summary>
        /// The canvas every frame is composed on, Kernel.ScreenWidth x Kernel.ScreenHeight. At 100% it
        /// is the full-screen canvas itself, shown by Kernel.Present() with a single Canvas.Display();
        /// at 150% and 200% it is a smaller off-screen canvas that Present() stretches to the display.
        /// </summary>
        public static Canvas Screen;

        /// <summary>
        /// The UI scales the Settings app offers, in percent.
        /// </summary>
        public static readonly int[] Scales = { 100, 150, 200 };

        /// <summary>
        /// The smallest UI a scale may leave: the desktop, taskbar and windows are laid out for it.
        /// </summary>
        public const int MinScaledWidth = 640;
        public const int MinScaledHeight = 480;

        private static bool _showStartMenu = false;

        public Explorer() : base("Explorer", ProcessType.KernelComponent)
        {
            WindowManager.Initialize();
            UpdateScreen();

            CustomConsole.WriteLineInfo("Starting desktop...");
            Desktop = new Desktop(0, 0, (int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);

            CustomConsole.WriteLineInfo("Starting setup...");
            Login = new LoginScreen(0, 0, (int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);

            CustomConsole.WriteLineInfo("Starting task bar...");
            Taskbar = new Taskbar();
            Taskbar.UpdateApplicationButtons();
            Taskbar.Visible = false;

            CustomConsole.WriteLineInfo("Starting start menu...");
            if (Kernel.Installed)
            {
                int menuWidth = 168;
                int menuHeight = 35 * 11;
                int menuX = 0;
                int menuY = (int)(Kernel.ScreenHeight - menuHeight - Taskbar.taskbarHeight);
                StartMenu = new StartMenu(menuX, menuY, menuWidth, menuHeight);
            }
            else
            {
                int menuWidth = 168;
                int menuHeight = 35 * 10;
                int menuX = 0;
                int menuY = (int)(Kernel.ScreenHeight - menuHeight - Taskbar.taskbarHeight);
                StartMenu = new StartMenu(menuX, menuY, menuWidth, menuHeight);
            }

            if (Kernel.Installed)
            {
                Settings config = new Settings(AuraPaths.SettingsIni);
                string value = config.GetValue("autologin");
                string computerName = config.GetValue("hostname");
                Kernel.ComputerName = computerName;

                if (value == "true")
                {
                    Taskbar.Visible = true;
                    Login.Hide();
                }
                else
                {
                    Kernel.LoggedIn = false;
                }
            }
            else
            {
                Taskbar.Visible = true;
                Login.Hide();
            }
        }

        /// <summary>
        /// Free memory kept after a resolution change, for everything else the frame allocates.
        /// </summary>
        private const long ResolutionHeadroomBytes = 32L * 1024 * 1024;

        /// <summary>
        /// True when a width x height display can be shown at scale percent: 100% always, 150% and
        /// 200% only while the UI they leave is at least MinScaledWidth x MinScaledHeight.
        /// </summary>
        public static bool FitsScale(int width, int height, int scale)
        {
            if (scale == 100)
            {
                return true;
            }

            return Array.IndexOf(Scales, scale) >= 0
                && width * 100 / scale >= MinScaledWidth
                && height * 100 / scale >= MinScaledHeight;
        }

        /// <summary>
        /// Sizes the UI to the display and Kernel.ScreenScale (back to 100% when the display is too small
        /// for it), and creates Screen: the full-screen canvas at 100%, a smaller off-screen one otherwise.
        /// </summary>
        private static void UpdateScreen()
        {
            int width = Kernel.Canvas.Width;
            int height = Kernel.Canvas.Height;

            if (!FitsScale(width, height, Kernel.ScreenScale))
            {
                Kernel.ScreenScale = 100;
            }

            Kernel.ScreenWidth = (uint)(width * 100 / Kernel.ScreenScale);
            Kernel.ScreenHeight = (uint)(height * 100 / Kernel.ScreenScale);

            // Drop the old screen first, so a collection during the allocation can reclaim it.
            Screen = null;
            WindowManager.SetScreen(null);

            Screen = Kernel.ScreenScale == 100 ? Kernel.Canvas : new Canvas((int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);
            WindowManager.SetScreen(Screen);
        }

        /// <summary>
        /// Switches the display to width x height and the UI to scale percent, then fits the desktop,
        /// login screen, taskbar, start menu, windows and mouse to the new UI size. Only a display that
        /// can switch modes (VMware SVGA II) changes resolution; any display can change scale. Returns
        /// false with the reason in error when the scale needs a larger resolution, when the switch needs
        /// more memory than is free, when the display refused the mode, or when it cannot switch
        /// (firmware framebuffer, virtio-gpu); nothing changed then. Called on the UI thread (a click),
        /// between two frames.
        /// </summary>
        public static bool ChangeResolution(int width, int height, int scale, out string error)
        {
            error = null;

            if (!FitsScale(width, height, scale))
            {
                error = scale + "% needs a resolution of at least " + (MinScaledWidth * scale / 100) + "x" + (MinScaledHeight * scale / 100) + ".";
                return false;
            }

            bool modeChanges = width != Kernel.Canvas.Width || height != Kernel.Canvas.Height;
            if (!modeChanges && scale == Kernel.ScreenScale)
            {
                return true;
            }

            // GEN3-GAP(oom): a failed allocation returns null instead of throwing, so the switch cannot
            // fail halfway: check first that the screen-sized buffers fit. They are the canvas back buffer
            // when the mode changes, the scaled screen below 100%, and the UI-sized desktop, its file panel
            // and its scaled wallpaper, plus the login screen and its wallpaper when shown. The old canvas
            // buffer stays alive until the new one exists; the old UI buffers are dropped as they are
            // replaced (one of them counted as still alive).
            long newPhysicalBytes = (long)width * height * 4;
            long newUiBytes = (long)(width * 100 / scale) * (height * 100 / scale) * 4;
            long oldUiBytes = (long)Kernel.ScreenWidth * Kernel.ScreenHeight * 4;
            int uiBuffers = Login.Visible ? 5 : 3;

            MemoryInfo.Collect();
            long freeBytes = (long)(MemoryInfo.FreePages * MemoryInfo.PageSizeBytes);
            long neededBytes = (modeChanges ? newPhysicalBytes : 0) + (scale != 100 ? newUiBytes : 0) + newUiBytes * uiBuffers + ResolutionHeadroomBytes;
            long availableBytes = freeBytes + oldUiBytes * (uiBuffers - 1) + (Screen != Kernel.Canvas ? oldUiBytes : 0);

            if (neededBytes > availableBytes)
            {
                error = "Not enough memory for " + width + "x" + height + " at " + scale + "% (about " + ((neededBytes - availableBytes) >> 20) + " MB more needed).";
                return false;
            }

            if (modeChanges)
            {
                int oldWidth = Kernel.Canvas.Width;
                int oldHeight = Kernel.Canvas.Height;

                try
                {
                    // The same canvas, switched through the display's IDisplayModes facet.
                    Kernel.Canvas = Canvas.GetFullScreen(new Mode(width, height, ColorDepth.ColorDepth32));
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Not listed by the display, or more than its VRAM holds: nothing changed.
                    error = width + "x" + height + " is not supported by this display.";
                    return false;
                }

                // GEN3-GAP(display-mode): the real framebuffer size from here on.
                if (Kernel.Canvas.Width == oldWidth && Kernel.Canvas.Height == oldHeight)
                {
                    error = "This display cannot change resolution.";
                    return false;
                }
            }

            Kernel.ScreenScale = scale;
            UpdateScreen();
            FitToScreen();

            if (Kernel.Canvas.Width != width || Kernel.Canvas.Height != height)
            {
                error = "The display switched to " + Kernel.Canvas.Width + "x" + Kernel.Canvas.Height + " instead.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lays the desktop, login screen, taskbar, start menu, windows and mouse out again for the
        /// current UI size (Kernel.ScreenWidth x Kernel.ScreenHeight).
        /// </summary>
        private static void FitToScreen()
        {
            if (WindowManager.ContextMenu != null)
            {
                WindowManager.ContextMenu.Opened = false;
                WindowManager.ContextMenu = null;
            }

            Desktop.ResizeToScreen();
            Login.ResizeToScreen();
            Taskbar.ResizeToScreen();
            StartMenu.Y = (int)Kernel.ScreenHeight - StartMenu.Height - Taskbar.taskbarHeight;

            // Keep every window reachable: pull back the ones now past the right or bottom edge.
            int maxBottom = (int)Kernel.ScreenHeight - Taskbar.taskbarHeight;
            foreach (Application app in WindowManager.Applications)
            {
                Window window = app.Window;
                window.MaxWidth = (int)Kernel.ScreenWidth;
                window.MaxHeight = (int)Kernel.ScreenHeight;

                if (window.X + window.Width > (int)Kernel.ScreenWidth)
                {
                    window.X = Math.Max(0, (int)Kernel.ScreenWidth - window.Width);
                }

                if (window.Y + window.Height > maxBottom)
                {
                    window.Y = Math.Max(0, maxBottom - window.Height);
                }

                app.MarkDirty();
            }

            Kernel.MouseManager.ResizeToScreen();
        }

        public override void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting Explorer process...");

            base.Initialize();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        public override void Update()
        {
            if (Kernel.LoggedIn)
            {
                StartMenu.Update();
                Taskbar.Update();

                WindowManager.DrawWindows();

                if (Kernel.MouseManager.IsLeftButtonDown)
                {
                    if (!StartMenu.IsInside((int)MouseManager.X, (int)MouseManager.Y) && !Taskbar.StartButton.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        ShowStartMenu = false;
                    }
                }
            }
            else
            {
                Login.Update();

                WindowManager.DrawWindows();
            }
        }
    }
}
