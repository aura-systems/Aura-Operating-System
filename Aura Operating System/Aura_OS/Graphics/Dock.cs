using Aura_OS.Processing;
using Aura_OS.System;
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
        uint Width = 200;
        uint Height = 30;
        uint Devide = 20;

        public bool Clicked = false;

        public void Update()
        {
            //Top bar
            Width = (uint)(Kernel.WindowManager.apps.Count * Kernel.programLogo.Width + Kernel.WindowManager.apps.Count * Devide);

            Kernel.canvas.DrawFilledRectangle(Kernel.avgColPen, 0, 0, (int)Kernel.screenWidth, 24);

            uint strX = 0;
            uint strY = 0;
            Kernel.canvas.DrawImage(Kernel.powerIco, (int)strX, (int)strY);

            if (!NetworkStack.ConfigEmpty())
            {
                Kernel.canvas.DrawImage(Kernel.connectedIco, (int)(Kernel.screenWidth - 24), (int)strY);
            }

            string time = Time.TimeString(true, true, true);
            Kernel.canvas.DrawString(time, Kernel.font, Kernel.BlackPen, (int)((Kernel.screenWidth / 2) - ((time.Length * Kernel.font.Width) / 2)), (int)(strY + 4));
            if (Kernel.Pressed)
            {
                if (MouseManager.X > strX && MouseManager.X < strX + Kernel.powerIco.Width && MouseManager.Y > strY && MouseManager.Y < strY + 24)
                {
                    ACPI.Shutdown();
                }
            }

            //Dock
            Kernel.canvas.DrawFilledRectangle(Kernel.avgColPen, (int)(Kernel.screenWidth - Width) / 2, (int)(Kernel.screenHeight - Height), (int)Width, (int)Height);

            int i = 0;
            foreach (var process in Kernel.ProcessManager.Processes)
            {
                if (process.Type == ProcessType.Program)
                {
                    var app = process as App;

                    app.dockX = (uint)(Devide / 2 + ((Kernel.screenWidth - Width) / 2) + (Kernel.programLogo.Width * i) + (Devide * i));
                    app.dockY = Kernel.screenHeight - Kernel.programLogo.Height - Devide / 2;
                    Kernel.canvas.DrawImage(Kernel.programLogo, (int)app.dockX, (int)app.dockY);
                    i++;

                    if (MouseManager.X > app.dockX && MouseManager.X < app.dockX + app.dockWidth && MouseManager.Y > app.dockY && MouseManager.Y < app.dockY + app.dockHeight)
                    {
                        Kernel.canvas.DrawString(app.name, Kernel.font, Kernel.WhitePen, (int)(app.dockX - ((app.name.Length * 8) / 2) + app.dockWidth / 2), (int)(app.dockY - 20));
                    }

                    if (MouseManager.MouseState == MouseState.Left)
                    {
                        if (!Clicked && MouseManager.X > app.dockX && MouseManager.X < app.dockX + app.dockWidth && MouseManager.Y > app.dockY && MouseManager.Y < app.dockY + app.dockHeight)
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
