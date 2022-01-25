using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using System;
using System.Drawing;

namespace Aura_OS.Apps.System
{
    public class Dock
    {
        uint Width = 200;
        uint Height = 30;
        uint Devide = 20;

        public void Update()
        {
            Width = (uint)(GUI.apps.Count * 20 + GUI.apps.Count * Devide);

            GUI.canvas.DrawFilledRectangle(GUI.avgColPen, 0, 0, (int)GUI.screenWidth, 20);

            string text = "PowerOFF";
            uint strX = 2;
            uint strY = (20 - 16) / 2;
            GUI.canvas.DrawString("PowerOFF", GUI.font, GUI.WhitePen, (int)(strX), (int)(strY));
            
            string time = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}";
            GUI.canvas.DrawString(time, GUI.font, GUI.WhitePen, (int)(GUI.screenWidth - strX - time.Length * GUI.font.Width), (int)(strY));
            if (GUI.Pressed)
            {
                if (MouseManager.X > strX && MouseManager.X < strX + (text.Length * 8) && MouseManager.Y > strY && MouseManager.Y < strY + 16)
                {
                    ACPI.Shutdown();
                }
            }

            for (int i = 0; i < GUI.apps.Count; i++)
            {
                GUI.apps[i].dockX = (uint)(Devide / 2 + ((GUI.screenWidth - Width) / 2) + (20 * i) + (Devide * i));
                GUI.apps[i].dockY = GUI.screenHeight - 20 - Devide / 2;
                //GUI.canvas.DrawImage(GUI.programlogo, (int)GUI.apps[i].dockX, (int)GUI.apps[i].dockY);
            }
        }
    }
}
