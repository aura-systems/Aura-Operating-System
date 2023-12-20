/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.System;
using Aura_OS.System.UI.GUI.Components;
using Aura_OS.Processing;

namespace Aura_OS.System.UI.GUI
{
    public class StartMenu : Window
    {
        List<Button> buttons;
        public Panel Ribbon;
        public Bitmap Logo;

        public StartMenu(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Ribbon = new Panel(Kernel.DarkBlue, X + 1, Y + 1, 26, height - 5);
            Logo = Kernel.AuraLogo2;
            buttons = new List<Button>();

            int buttonY = 0;
            foreach (var app in Kernel.ProcessManager.Processes)
            {
                if (app.Type == ProcessType.Program)
                {
                    var button = new Button(Kernel.shutdownIco, app.Name, X + 4, Y + buttonY, width, 36);
                    button.NoBorder = true;
                    buttons.Add(button);
                    buttonY += 36;
                }
            }
        }

        public override void Update()
        {
            base.Update();

            Ribbon.X = X + 3;
            Ribbon.Y = Y + 3;
            Ribbon.Update();
            Kernel.canvas.DrawImageAlpha(Logo, X + 5, Y + Height - 66);

            foreach (var button in buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    button.BackColor = Color.DarkBlue;
                }
                else
                {
                    button.BackColor = Color.Gray;
                }

                button.Update();
            }
        }
    }
}