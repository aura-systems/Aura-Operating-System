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
        Element StartButton;
        public bool Clicked = false;
        
        public Dock()
        {
            startY = (int)(Kernel.screenHeight - taskbarHeight);

            // Start button
            int startButtonWidth = 28;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = (int)Kernel.screenHeight - startButtonHeight - 3;
            StartButton = new Element(startButtonX, startButtonY, startButtonWidth, startButtonHeight);
        }
        
        public void Update()
        {
            // Taskbar
            Kernel.canvas.DrawLine(Kernel.WhiteColor, 0, startY, (int)Kernel.screenWidth + 10, startY);
            Kernel.canvas.DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Update();
            Kernel.canvas.DrawImage(Kernel.Start, 5, (int)Kernel.screenHeight - 28);

            int buttonX = 36;
            foreach (var process in Kernel.ProcessManager.Processes)
            {
                if (process.Type == ProcessType.Program)
                {
                    var app = process as App;

                    var button = new Button(app.name, buttonX, (int)Kernel.screenHeight - 28 - 3, app.name.Length * (8 + 1) , 28);
                    buttonX += app.name.Length * (8 + 1) + 4;
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
