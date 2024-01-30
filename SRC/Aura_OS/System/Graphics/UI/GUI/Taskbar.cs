/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Taskbar
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Taskbar
    {
        public static int taskbarHeight = 33;
        int startY;

        Button StartButton;
        Button HourButton;
        Button NetworkButton;
        

        public bool Clicked = false;

        public Dictionary<string, Button> Buttons;

        public Taskbar()
        {
            startY = (int)(Kernel.screenHeight - taskbarHeight);

            // Start button
            int startButtonWidth = 70;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = (int)Kernel.screenHeight - startButtonHeight - 3;
            StartButton = new Button(ResourceManager.GetImage("00-start.bmp"), "Start", startButtonX, startButtonY, startButtonWidth, startButtonHeight);

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

            Buttons = new Dictionary<string, Button>();
        }

        public void UpdateApplicationButtons()
        {
            Buttons.Clear();

            int buttonX = 74;
            foreach (var app in Explorer.WindowManager.Applications)
            {
                var spacing = app.Name.Length * 9 + (int)app.Window.Icon.Width;
                var button = new Button(app.Window.Icon, app.Name, buttonX, (int)Kernel.screenHeight - 28 - 3, spacing, 28);

                Buttons.Add(app.Name, button);

                buttonX += spacing + 4;
            }
        }

        public void Update()
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
        }

        private void HandleClick()
        {
            if (StartButton.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Explorer.ShowStartMenu = !Explorer.ShowStartMenu;
            }

            foreach (var application in  Explorer.WindowManager.Applications)
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
                    application.Window.Minimize.Click();
                }
            }
        }

        public void Draw()
        {
            // Taskbar
            Kernel.canvas.DrawLine(Kernel.WhiteColor, 0, startY, (int)Kernel.screenWidth + 10, startY);
            Kernel.canvas.DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Draw();

            // Hour
            string time = Time.TimeString(true, true, true);
            HourButton.Text = time;
            HourButton.Draw();

            // Notifications
            DrawNotifications();

            // Applications
            DrawApplications();
        }

        private void DrawApplications()
        {
            foreach (var application in Explorer.WindowManager.Applications)
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
