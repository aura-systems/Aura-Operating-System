/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using Cosmos.System;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Application;
using System.IO;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 2 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
            RightClickEntry entry = new("Open in Explorer", 0, 0, RightClick.Width);
            entry.Action = new Action(() =>
            {
                Explorer app = new(500, 400, 40, 40);
                app.Initialize();

                Kernel.WindowManager.apps.Add(app);
                app.zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
                Kernel.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Kernel.Taskbar.UpdateApplicationButtons();
                Kernel.WindowManager.UpdateFocusStatus();

                RightClick.Opened = false;
            });
            rightClickEntries.Add(entry);
            RightClickEntry entry2 = new("OS Information", 0, 0, RightClick.Width);
            entry2.Action = new Action(() =>
            {
                SystemInfo app = new(402, 360, 40, 40);
                app.Initialize();

                Kernel.WindowManager.apps.Add(app);
                app.zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
                Kernel.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Kernel.Taskbar.UpdateApplicationButtons();
                Kernel.WindowManager.UpdateFocusStatus();

                RightClick.Opened = false;
            });
            rightClickEntries.Add(entry2);
            RightClick.Entries = rightClickEntries;
        }

        public override void Draw()
        {
            //canvas.Clear(0x000000);
            //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

            Kernel.canvas.DrawImage(Kernel.wallpaper, X, Y);

            try
            {
                if (Kernel.VirtualFileSystem != null && Kernel.VirtualFileSystem.GetVolumes().Count > 0)
                {
                    DrawDesktopItems();
                }
            }
            catch (Exception ex)
            {
                Crash.StopKernel("Fatal dotnet exception occured while drawing dekstop items.", ex.Message, "0x00000000", "0");
            }

            base.Draw();
        }

        public static void DrawDesktopItems()
        {
            int startX = 10;
            int startY = 40;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            string[] directories = Directory.GetDirectories(Kernel.CurrentVolume);
            string[] files = Directory.GetFiles(Kernel.CurrentVolume);

            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                DrawIconAndText(ResourceManager.GetImage("32-folder.bmp"), folderName, currentX, currentY);

                currentY += iconSpacing;
                if (currentY + iconSpacing > Kernel.screenHeight - 64)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                DrawIconAndText(ResourceManager.GetImage("32-file.bmp"), fileName, currentX, currentY);

                currentY += iconSpacing;
                if (currentY + iconSpacing > Kernel.screenHeight - 64)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }
        }

        private static void DrawIconAndText(Bitmap bitmap, string text, int x, int y)
        {
            Kernel.canvas.DrawImageAlpha(bitmap, x + 6, y);
            Kernel.canvas.DrawString(text, Kernel.font, Kernel.WhiteColor, x, y + 35);
        }
    }
}