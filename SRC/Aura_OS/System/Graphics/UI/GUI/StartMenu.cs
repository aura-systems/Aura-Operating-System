/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.System;
using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class StartMenu : Window
    {
        List<Button> buttons;
        public Button Shutdown;
        public Button Reboot;
        public Panel Ribbon;
        public Bitmap Logo;

        public StartMenu(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Ribbon = new Panel(Kernel.DarkBlue, X + 1, Y + 1, 26, height - 5);
            Logo = Kernel.AuraLogo2;
            buttons = new List<Button>();
            Shutdown = new Button(ResourceManager.GetImage("24-shutdown.bmp"), "Shut Down.", X + 1 + Ribbon.Width, Y + Height - 32 - 4, Width - Ribbon.Width - 3, 36);
            Shutdown.NoBorder = true;
            Shutdown.action = new Action(() =>
            {
                Power.Shutdown();
            });
            Reboot = new Button(ResourceManager.GetImage("24-reboot.bmp"), "Reboot.", X + 1 + Ribbon.Width, Y + Height - 64 - 4, Width - Ribbon.Width - 3, 36);
            Reboot.NoBorder = true;
            Reboot.action = new Action(() =>
            {
                Power.Reboot();
            });
        }

        public override void Update()
        {
            base.Update();

            Ribbon.X = X + 3;
            Ribbon.Y = Y + 3;
            Ribbon.Update();
            Kernel.canvas.DrawImageAlpha(Logo, X + 5, Y + Height - 66);

            buttons.Clear();

            int buttonY = 0;
            foreach (var app in Kernel.ProcessManager.Processes)
            {
                if (app.Type == ProcessType.Program)
                {
                    var button = new Button(ResourceManager.GetImage("24-program.bmp"), app.Name, X + 1 + Ribbon.Width, Y + buttonY + 1, Width - Ribbon.Width - 3, 36);
                    button.NoBorder = true;
                    buttons.Add(button);
                    buttonY += 32;
                }
            }

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

            if (Kernel.Pressed)
            {
                if (Shutdown.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    Shutdown.action();
                }

                if (Reboot.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    Reboot.action();
                }
            }            
        }
    }
}