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

namespace Aura_OS
{
    public class Dock
    {
        Element StartButton;
        int taskbarHeight = 33;
        int startY;

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
        }
    }
}
