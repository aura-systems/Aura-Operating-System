using Aura_OS.System;
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

            Kernel.canvas.DrawFilledRectangle(Kernel.WhitePen, 0, 0, (int)Kernel.screenWidth, 24);

            uint strX = 0;
            uint strY = 0;
            Kernel.canvas.DrawImage(Kernel.powerIco, (int)strX, (int)strY);
            
            string time = Time.TimeString(true, true, true);
            Kernel.canvas.DrawString(time, Kernel.font, Kernel.BlackPen, (int)((Kernel.screenWidth / 2) - ((time.Length * Kernel.font.Width) / 2)), (int)(strY + 4));
            if (Kernel.Pressed)
            {
                if (MouseManager.X > strX && MouseManager.X < strX + Kernel.powerIco.Width && MouseManager.Y > strY && MouseManager.Y < strY + 24)
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
