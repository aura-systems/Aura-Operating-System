using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using System;
using System.Drawing;

namespace Aura_OS
{
    public class Dock
    {
        uint Width = 200;
        uint Height = 30;
        uint Devide = 20;

        public void Update()
        {
            Width = (uint)(Kernel.apps.Count * Kernel.programlogo.Width + Kernel.apps.Count * Devide);

            Kernel.canvas.DrawFilledRectangle(Kernel.avgColPen, 0, 0, (int)Kernel.screenWidth, 20);

            string text = "PowerOFF";
            uint strX = 2;
            uint strY = (20 - 16) / 2;
            Kernel.canvas.DrawString("PowerOFF", Kernel.font, Kernel.WhitePen, (int)(strX), (int)(strY));
            
            string time = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}";
            Kernel.canvas.DrawString(time, Kernel.font, Kernel.WhitePen, (int)(Kernel.screenWidth - strX - time.Length * Kernel.font.Width), (int)(strY));
            if (Kernel.Pressed)
            {
                if (MouseManager.X > strX && MouseManager.X < strX + (text.Length * 8) && MouseManager.Y > strY && MouseManager.Y < strY + 16)
                {
                    ACPI.Shutdown();
                }
            }

            Kernel.canvas.DrawFilledRectangle(Kernel.avgColPen, (int)(Kernel.screenWidth - Width) / 2, (int)(Kernel.screenHeight - Height), (int)Width, (int)Height);

            for (int i = 0; i < Kernel.apps.Count; i++)
            {
                Kernel.apps[i].dockX = (uint)(Devide / 2 + ((Kernel.screenWidth - Width) / 2) + (Kernel.programlogo.Width * i) + (Devide * i));
                Kernel.apps[i].dockY = Kernel.screenHeight - Kernel.programlogo.Height - Devide / 2;
                Kernel.canvas.DrawImage(Kernel.programlogo, (int)Kernel.apps[i].dockX, (int)Kernel.apps[i].dockY);
            }
        }
    }
}
