/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Explorer process
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI;
using System;

namespace Aura_OS.System.Processing.Processes
{
    public class Explorer : Process
    {
        private static bool _showStartMenu = false;
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
            }
        }

        public static Taskbar Taskbar;
        public static StartMenu StartMenu;
        public static Desktop Desktop;
        public static WindowManager WindowManager;

        public Explorer() : base("Explorer", ProcessType.KernelComponent)
        {
            CustomConsole.WriteLineInfo("Starting desktop...");
            Desktop = new Desktop(0, 0, (int)Kernel.screenWidth, (int)Kernel.screenHeight);

            CustomConsole.WriteLineInfo("Starting window manager...");
            WindowManager = new WindowManager();

            CustomConsole.WriteLineInfo("Starting task bar...");
            Taskbar = new Taskbar();
            Taskbar.UpdateApplicationButtons();

            CustomConsole.WriteLineInfo("Starting start menu...");
            int menuWidth = 168;
            int menuHeight = 32 * 9;
            int menuX = 0;
            int menuY = (int)(Kernel.screenHeight - menuHeight - Taskbar.taskbarHeight);
            StartMenu = new StartMenu(menuX, menuY, menuWidth, menuHeight);
        }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public override void Update()
        {
            Desktop.Update();
            Desktop.Draw();

            try
            {
                Taskbar.Update();
                Taskbar.Draw();
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

            if (ShowStartMenu)
            {
                StartMenu.Update();
                StartMenu.Draw();
            }
        }
    }
}
