/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Taskbar
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Graphics.UI.GUI.Skin;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class HourButton : Button
    {
        public HourButton(string text, int x, int y, int width, int height) : base(text, x, y, width, height)
        {
            Light = true;
            ForceDirty = true;
        }

        public override void Draw()
        {
            base.Draw();

            // Hour
            string time = Time.TimeString(true, true, true);
            Text = time;
        }
    }

    public class Taskbar : Panel
    {
        public static int taskbarHeight = 33;

        Button StartButton;
        Button HourButton;
        Button NetworkButton;

        public bool Clicked = false;

        public Dictionary<string, Button> Buttons;

        public Taskbar() : base(Kernel.Gray, 0, (int)Kernel.screenHeight - taskbarHeight, (int)Kernel.screenWidth, taskbarHeight)
        {
            // Start button
            int startButtonWidth = 70;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = (int)Kernel.screenHeight - startButtonHeight - 3;
            StartButton = new Button(ResourceManager.GetImage("00-start.bmp"), "Start", startButtonX, startButtonY, startButtonWidth, startButtonHeight);
            StartButton.HasTransparency = true;
            StartButton.Frame = Kernel.SkinParser.GetFrame("button.normal");
            AddChild(StartButton);

            // Hour button
            string time = Time.TimeString(true, true, true);
            int hourButtonWidth = time.Length * (Kernel.font.Width + 1);
            int hourButtonHeight = 28;
            int hourButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2);
            int hourButtonY = (int)Kernel.screenHeight - 28 - 3;
            HourButton = new HourButton(time, hourButtonX, hourButtonY, hourButtonWidth, hourButtonHeight);
            AddChild(HourButton);

            // Network icon
            int networkButtonWidth = 16;
            int networkButtonHeight = 16;
            int netoworkButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2) - 20;
            int networkButtonY = (int)Kernel.screenHeight - 25;
            NetworkButton = new Button(ResourceManager.GetImage("16-network-offline.bmp"), netoworkButtonX, networkButtonY, networkButtonWidth, networkButtonHeight);
            NetworkButton.NoBorder = true;
            NetworkButton.HasTransparency = true;
            AddChild(NetworkButton);

            Buttons = new Dictionary<string, Button>();
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
        }

        private void HandleClick()
        {
            MarkDirty();

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
                    application.Window.Visible = !application.Window.Visible;
                    application.Window.Minimize.Click();
                }
            }
        }

        public void UpdateApplicationButtons()
        {
            Buttons.Clear();

            int buttonX = 74;
            foreach (var app in Explorer.WindowManager.Applications)
            {
                var spacing = app.Name.Length * 9 + (int)app.Window.Icon.Width;
                var button = new Button(app.Window.Icon, app.Name, buttonX, (int)Kernel.screenHeight - 28 - 3, spacing, 28);
                button.Frame = Kernel.SkinParser.GetFrame("button.normal");

                Buttons.Add(app.Name, button);

                buttonX += spacing + 4;
            }
        }

        public override void Draw()
        {
            base.Draw();

            // Taskbar
            //DrawLine(Kernel.WhiteColor, 0, 0, (int)Kernel.screenWidth + 10, 1); TODO
            //DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Draw();

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

                Buttons[application.Name].Draw();
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

            NetworkButton.Draw();
        }
    }
}
