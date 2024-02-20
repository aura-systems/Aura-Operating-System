/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Explorer process
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System;

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
        public static WindowManager WindowManager;

        private static bool _showStartMenu = false;

        public Explorer() : base("Explorer", ProcessType.KernelComponent)
        {
            CustomConsole.WriteLineInfo("Starting desktop...");
            Desktop = new Desktop(0, 0, (int)Kernel.screenWidth, (int)Kernel.screenHeight);

            WindowManager = new WindowManager();
            WindowManager.Initialize();

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
            CustomConsole.WriteLineInfo("Starting Explorer process...");

            base.Initialize();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        public override void Update()
        {
            Taskbar.Update();

            //WindowManager.UpdateWindowStack();
            WindowManager.DrawWindows();

            if (ShowStartMenu)
            {
                StartMenu.Update();
            }
        }
    }
}
