/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Start menu
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.System;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class StartMenu : Window
    {
        private List<Button> buttons;
        private Button Shutdown;
        private Button Reboot;
        private Panel Ribbon;
        private Bitmap Logo;

        private bool Clicked = false;

        public StartMenu(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Ribbon = new Panel(Kernel.DarkBlue, X + 1, Y + 1, 26, height - 5);
            Logo = Kernel.AuraLogo2;
            buttons = new List<Button>();
            Shutdown = new Button(ResourceManager.GetImage("24-shutdown.bmp"), "Shut Down.", X + 1 + Ribbon.Width, Y + Height - 32 - 4, Width - Ribbon.Width - 3, 35);
            Shutdown.NoBorder = true;
            Shutdown.Action = new Action(() =>
            {
                Power.Shutdown();
            });
            Reboot = new Button(ResourceManager.GetImage("24-reboot.bmp"), "Reboot.", X + 1 + Ribbon.Width, Y + Height - 64 - 4, Width - Ribbon.Width - 3, 35);
            Reboot.NoBorder = true;
            Reboot.Action = new Action(() =>
            {
                Power.Reboot();
            });

            buttons.Clear();

            int buttonY = 0;
            foreach (var applicationConfig in Kernel.ApplicationManager.ApplicationTemplates)
            {
                Bitmap icon = null;

                if (applicationConfig.Template.Name.StartsWith("Terminal"))
                {
                    icon = ResourceManager.GetImage("24-terminal.bmp");
                }
                else if (applicationConfig.Template.Name.StartsWith("Explorer"))
                {
                    icon = ResourceManager.GetImage("24-explorer.bmp");
                }
                else
                {
                    icon = ResourceManager.GetImage("24-program.bmp");
                }

                var button = new Button(icon, applicationConfig.Template.Name, X + 1 + Ribbon.Width, Y + buttonY + 1, Width - Ribbon.Width - 3, 35);
                button.NoBorder = true;
                button.Action = new Action(() =>
                {
                    Kernel.ApplicationManager.StartApplication(applicationConfig);
                    Explorer.ShowStartMenu = false;
                });
                buttons.Add(button);
                buttonY += 32;
            }
        }

        public override void Update()
        {
            base.Update();

            Ribbon.X = X + 3;
            Ribbon.Y = Y + 3;
            Ribbon.Update();

            // Applications buttons
            foreach (var button in buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    button.BackColor = Color.DarkBlue;
                    button.TextColor = Color.White;
                }
                else
                {
                    button.BackColor = Color.LightGray;
                    button.TextColor = Color.Black;
                }

                button.Update();
            }

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

            // Shutdown + Reboot buttons
            Shutdown.Update();
            Reboot.Update();

            if (Shutdown.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Shutdown.BackColor = Color.DarkBlue;
                Shutdown.TextColor = Color.White;
            }
            else
            {
                Shutdown.BackColor = Color.LightGray;
                Shutdown.TextColor = Color.Black;
            }

            if (Reboot.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Reboot.BackColor = Color.DarkBlue;
                Reboot.TextColor = Color.White;
            }
            else
            {
                Reboot.BackColor = Color.LightGray;
                Reboot.TextColor = Color.Black;
            }
        }

        public override void Draw()
        {
            base.Draw();

            Ribbon.Draw();
            Kernel.canvas.DrawImage(Logo, X + 5, Y + Height - 66);

            foreach (var button in buttons)
            {
                button.Draw();
            }

            Shutdown.Draw();
            Reboot.Draw();
        }

        private void HandleClick()
        {
            if (Explorer.ShowStartMenu && !IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Explorer.ShowStartMenu = false;
                return;
            }

            if (Shutdown.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Shutdown.Action();
            }

            if (Reboot.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Reboot.Action();
            }

            foreach (var button in buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    button.Action();
                }
            }
        }
    }
}