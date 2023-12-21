/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Taskbar
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Cosmos.System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Taskbar : Process
    {
        int taskbarHeight = 33;
        int startY;

        Button StartButton;
        Button HourButton;
        Button NetworkButton;
        StartMenu StartMenu;

        public bool Clicked = false;
        public bool ShowStartMenu = false;

        public Dictionary<string, Button> Buttons;

        public Taskbar() : base("Docker", ProcessType.KernelComponent)
        {
            startY = (int)(Kernel.screenHeight - taskbarHeight);

            // Start button
            int startButtonWidth = 28;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = (int)Kernel.screenHeight - startButtonHeight - 3;
            StartButton = new Button(ResourceManager.GetImage("00-start.bmp"), startButtonX, startButtonY, startButtonWidth, startButtonHeight);

            string time = Time.TimeString(true, true, true);
            int hourButtonWidth = time.Length * (Kernel.font.Width + 1);
            int hourButtonHeight = 28;
            int hourButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2);
            int hourButtonY = (int)Kernel.screenHeight - 28 - 3;
            HourButton = new Button(time, hourButtonX, hourButtonY, hourButtonWidth, hourButtonHeight);
            HourButton.Light = true;

            int networkButtonWidth = 16;
            int networkButtonHeight = 16;
            int netoworkButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2) - 20;
            int networkButtonY = (int)Kernel.screenHeight - 25;
            NetworkButton = new Button(ResourceManager.GetImage("16-network-offline.bmp"), netoworkButtonX, networkButtonY, networkButtonWidth, networkButtonHeight);
            NetworkButton.NoBorder = true;

            int menuWidth = 168;
            int menuHeight = 32 * 8;
            int menuX = 0;
            int menuY = (int)(Kernel.screenHeight - menuHeight - taskbarHeight);
            StartMenu = new StartMenu(menuX, menuY, menuWidth, menuHeight);

            Buttons = new Dictionary<string, Button>();
        }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public void UpdateApplicationButtons()
        {
            Buttons.Clear();

            int buttonX = 36;
            foreach (var app in Kernel.WindowManager.apps)
            {
                var spacing = app.Name.Length * 9 + (int)app.Window.Icon.Width;
                var button = new Button(app.Window.Icon, app.Name, buttonX, (int)Kernel.screenHeight - 28 - 3, spacing, 28);

                Buttons.Add(app.Name, button);

                buttonX += spacing + 4;
            }
        }

        public override void Update()
        {
            if (MouseManager.MouseState == MouseState.Left)
            {
                if (!Clicked)
                {
                    Clicked = true;
                }
            }
            else if (Clicked)
            {
                Clicked = false;
                HandleClick();
            }

            Draw();
        }

        private void HandleClick()
        {
            if (StartButton.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                ShowStartMenu = !ShowStartMenu;
            }

            foreach (var application in  Kernel.WindowManager.apps)
            {
                if (application.Focused)
                {
                    Buttons[application.Name].Focused = true;
                    application.Window.TopBar.Color1 = Kernel.DarkBlue;
                    application.Window.TopBar.Color2 = Kernel.Pink;
                }
                else
                {
                    Buttons[application.Name].Focused = false;
                    application.Window.TopBar.Color1 = Kernel.DarkGray;
                }

                if (Buttons[application.Name].IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    application.Window.Minimize.Action();
                }
            }
        }

        public void Draw()
        {
            // Taskbar
            Kernel.canvas.DrawLine(Kernel.WhiteColor, 0, startY, (int)Kernel.screenWidth + 10, startY);
            Kernel.canvas.DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Update();

            // Hour
            string time = Time.TimeString(true, true, true);
            HourButton.Text = time;
            HourButton.Update();

            // Notifications
            DrawNotifications();

            // Applications
            DrawApplications();

            // Start menu
            DrawStartMenu();
        }

        private void DrawApplications()
        {
            foreach (var application in Kernel.WindowManager.apps)
            {
                if (application.Focused)
                {
                    Buttons[application.Name].Focused = true;
                    application.Window.TopBar.Color1 = Kernel.DarkBlue;
                    application.Window.TopBar.Color2 = Kernel.Pink;
                }
                else
                {
                    Buttons[application.Name].Focused = false;
                    application.Window.TopBar.Color1 = Kernel.DarkGray;
                }

                Buttons[application.Name].Update();
            }
        }

        private void DrawStartMenu()
        {
            if (ShowStartMenu)
            {
                StartMenu.Update();
            }
        }

        public void DrawNotifications()
        {
            if (Kernel.NetworkTransmitting)
            {
                //NetworkButton.Image = Kernel.networkTransmitIco;
            }
            else
            {
                if (Kernel.NetworkConnected)
                {
                    NetworkButton.Image = ResourceManager.GetImage("16-network-idle.bmp");
                }
                else
                {
                    NetworkButton.Image = ResourceManager.GetImage("16-network-offline.bmp");
                }
            }

            NetworkButton.Update();
        }
    }
}
