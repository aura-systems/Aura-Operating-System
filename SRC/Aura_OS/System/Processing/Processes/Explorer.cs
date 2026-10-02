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
        public static DirectBitmap Screen;

        private static bool _showStartMenu = false;

        public Explorer() : base("Explorer", ProcessType.KernelComponent)
        {
            // Zero-copy alias of the canvas back buffer (Screen.Bitmap is null): the composited frame
            // is shown by Kernel.Present() with a single Canvas.Display(), no extra 8 MB blit.
            // ChangeResolution re-creates it (and calls SetScreen again) after a mode change.
            Screen = new DirectBitmap(Kernel.Canvas);
            WindowManager.Initialize();
            WindowManager.SetScreen(Screen);

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
        /// Switches the screen to width x height and fits the desktop, login screen, taskbar, start menu,
        /// windows and mouse to it. Only a display that can switch modes (VMware SVGA II) does. Returns
        /// false with the reason in error when the mode needs more memory than is free, when the display
        /// refused it, or when the display cannot switch (firmware framebuffer, virtio-gpu); nothing
        /// changed then. Called on the UI thread (a click), between two frames.
        /// </summary>
        public static bool ChangeResolution(int width, int height, out string error)
        {
            error = null;

            // GEN3-GAP(oom): a failed allocation returns null instead of throwing, so the switch cannot
            // fail halfway: check first that the screen-sized buffers fit. They are the canvas back buffer,
            // the desktop, its file panel and its scaled wallpaper, plus the login screen and its wallpaper
            // when shown. The old canvas buffer and wallpaper stay alive until the new ones exist.
            long newBytes = (long)width * height * 4;
            long oldBytes = (long)Kernel.ScreenWidth * Kernel.ScreenHeight * 4;
            int buffers = Login.Visible ? 6 : 4;

            MemoryInfo.Collect();
            long freeBytes = (long)(MemoryInfo.FreePages * MemoryInfo.PageSizeBytes);
            long neededBytes = newBytes * buffers + ResolutionHeadroomBytes;
            long availableBytes = freeBytes + oldBytes * (buffers - 2);

            if (neededBytes > availableBytes)
            {
                error = "Not enough memory for " + width + "x" + height + " (about " + ((neededBytes - availableBytes) >> 20) + " MB more needed).";
                return false;
            }

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
            if (Kernel.ScreenWidth == (uint)Kernel.Canvas.Width && Kernel.ScreenHeight == (uint)Kernel.Canvas.Height)
            {
                if (Kernel.Canvas.Width != width || Kernel.Canvas.Height != height)
                {
                    error = "This display cannot change resolution.";
                    return false;
                }

                return true;
            }

            Kernel.ScreenWidth = (uint)Kernel.Canvas.Width;
            Kernel.ScreenHeight = (uint)Kernel.Canvas.Height;

            // The canvas reallocated its back buffer: alias the new one.
            Screen = new DirectBitmap(Kernel.Canvas);
            WindowManager.SetScreen(Screen);

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

            if (Kernel.Canvas.Width != width || Kernel.Canvas.Height != height)
            {
                error = "The display switched to " + Kernel.Canvas.Width + "x" + Kernel.Canvas.Height + " instead.";
                return false;
            }

            return true;
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
