using Aura_OS.Processing;
using Aura_OS.System;
using Aura_OS.System.Graphics.UI;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Network;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Aura_OS
{
    public class Dock
    {
        int taskbarHeight = 33;
        int startY;
        Button StartButton;
        Button HourButton;
        public bool Clicked = false;
        
        public Dock()
        {
            startY = (int)(Kernel.screenHeight - taskbarHeight);

            // Start button
            int startButtonWidth = 28;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = (int)Kernel.screenHeight - startButtonHeight - 3;
            StartButton = new Button(Kernel.Start, startButtonX, startButtonY, startButtonWidth, startButtonHeight);

            string time = Time.TimeString(true, true, true);
            int hourButtonWidth = time.Length * (Kernel.font.Width + 1);
            int hourButtonHeight = 28;
            int hourButtonX = (int)(Kernel.screenWidth - (time.Length * (Kernel.font.Width + 1)) - 2);
            int hourButtonY = (int)Kernel.screenHeight - 28 - 3;
            HourButton = new Button(time, hourButtonX, hourButtonY, hourButtonWidth, hourButtonHeight, true);
        }
        
        public void Update()
        {
            // Taskbar
            Kernel.canvas.DrawLine(Kernel.WhiteColor, 0, startY, (int)Kernel.screenWidth + 10, startY);
            Kernel.canvas.DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Update();

            string time = Time.TimeString(true, true, true);
            HourButton.Text = time;
            HourButton.Update();

            int buttonX = 36;
            foreach (var process in Kernel.ProcessManager.Processes)
            {
                if (process.Type == ProcessType.Program)
                {
                    var app = process as App;

                    var spacing = app.Name.Length * 9 + (int)app.Window.Icon.Width;
                    var button = new Button(app.Window.Icon, app.Name, buttonX, (int)Kernel.screenHeight - 28 - 3, spacing, 28);
                    buttonX += spacing + 4;
                    button.Update();

                    if (MouseManager.MouseState == MouseState.Left)
                    {
                        if (!Clicked && button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                        {
                            Clicked = true;

                            app.visible = !app.visible;

                            if (app.visible)
                            {
                                Kernel.ProcessManager.Start(app);
                            }
                            else
                            {
                                app.Stop();
                            }
                        }
                    }
                    else 
                    {
                        Clicked = false;
                    }
                }
            }
        }
    }
}
