using Aura_OS.Processing;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Network;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Xml.Linq;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class ApplicationButton
    {
        public Button Button;
        public App Application;

        public ApplicationButton(Button button, App application)
        {
            Button = button;
            Application = application;
        }
    }

    public class Dock : Process
    {
        int taskbarHeight = 33;
        int startY;

        Button StartButton;
        Button HourButton;
        Button NetworkButton;
        StartMenu StartMenu;
        List<ApplicationButton> Applications;

        public bool Clicked = false;
        public bool ShowStartMenu = false;

        public Dock() : base("Docker", ProcessType.KernelComponent)
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
            int menuHeight = 350;
            int menuX = 0;
            int menuY = (int)(Kernel.screenHeight - menuHeight - taskbarHeight);
            StartMenu = new StartMenu(menuX, menuY, menuWidth, menuHeight);

            // Applications
            Applications = new List<ApplicationButton>();
            UpdateApplicationButtons();
        }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public void UpdateApplicationButtons()
        {
            Applications.Clear();

            int buttonX = 36;
            foreach (var process in Kernel.ProcessManager.Processes)
            {
                if (process.Type == ProcessType.Program)
                {
                    var app = process as App;

                    var spacing = app.Name.Length * 9 + (int)app.Window.Icon.Width;
                    var button = new Button(app.Window.Icon, app.Name, buttonX, (int)Kernel.screenHeight - 28 - 3, spacing, 28);

                    Applications.Add(new ApplicationButton(button, app));

                    buttonX += spacing + 4;
                }
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

            // Applications
            for (int i = 0; i < Applications.Count; i++)
            {
                var app = Applications[i];

                if (Kernel.WindowManager.Focused != null)
                {
                    if (Kernel.WindowManager.Focused.Equals(app))
                    {
                        app.Button.Focused = true;
                        app.Application.Window.TopBar.Color1 = Kernel.DarkBlue;
                        app.Application.Window.TopBar.Color2 = Kernel.Pink;
                    }
                    else
                    {
                        app.Button.Focused = false;
                        app.Application.Window.TopBar.Color1 = Kernel.DarkGray;
                    }
                }

                if (app.Button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    app.Application.visible = !app.Application.visible;

                    if (app.Application.visible)
                    {
                        Kernel.ProcessManager.Start(app.Application);
                        Kernel.WindowManager.Focused = app.Application;
                    }
                    else
                    {
                        app.Application.Stop();
                    }
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
            for (int i = 0; i < Applications.Count; i++)
            {
                var app = Applications[i];

                app.Button.Update();
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
