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
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class DesktopIcon
    {
        public Bitmap Icon { get; set; }
        public string Label { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public DesktopIcon(Bitmap icon, string label, int x, int y)
        {
            Icon = icon;
            Label = label;
            X = x;
            Y = y;
        }
    }

    public class Desktop : Component
    {
        private List<string> directories;
        private List<string> files;
        private List<DesktopIcon> iconPositions;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 3 * RightClickEntry.ConstHeight);
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
            });
            rightClickEntries.Add(entry2);
            RightClickEntry entry3 = new("Refresh Desktop", 0, 0, RightClick.Width);
            entry3.Action = new Action(() =>
            {
                RefreshDesktop();

                RightClick.Opened = false;
            });
            rightClickEntries.Add(entry3);
            RightClick.Entries = rightClickEntries;

            RefreshDesktop();
        }

        public override void Draw()
        {
            //canvas.Clear(0x000000);
            //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

            Kernel.canvas.DrawImage(Kernel.wallpaper, X, Y);

            DrawDesktopItems();
        }

        private void ReadDirectoriesAndFiles()
        {
            directories = new List<string>(Directory.GetDirectories(Kernel.CurrentVolume));
            files = new List<string>(Directory.GetFiles(Kernel.CurrentVolume));
        }

        private void RefreshDesktop()
        {
            if (Kernel.VirtualFileSystem != null && Kernel.VirtualFileSystem.GetVolumes().Count > 0)
            {
                ReadDirectoriesAndFiles();
                CalculateIconPositions();
            }
        }

        private void CalculateIconPositions()
        {
            iconPositions = new();

            int startX = 10;
            int startY = 40;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                iconPositions.Add(new DesktopIcon(ResourceManager.GetImage("32-folder.bmp"), folderName, currentX, currentY));
                UpdatePosition(ref currentX, ref currentY, iconSpacing);
            }

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                iconPositions.Add(new DesktopIcon(ResourceManager.GetImage("32-file.bmp"), fileName, currentX, currentY));
                UpdatePosition(ref currentX, ref currentY, iconSpacing);
            }
        }

        private void UpdatePosition(ref int x, ref int y, int spacing)
        {
            y += spacing;
            if (y + spacing > Kernel.screenHeight - 64)
            {
                y = 40;
                x += spacing;
            }
        }

        public void DrawDesktopItems()
        {
            foreach (var iconInfo in iconPositions)
            {
                DrawIconAndText(iconInfo.Icon, iconInfo.Label, iconInfo.X, iconInfo.Y);
            }
        }

        private void DrawIconAndText(Bitmap bitmap, string text, int x, int y)
        {
            Kernel.canvas.DrawImageAlpha(bitmap, x + 6, y);
            Kernel.canvas.DrawString(text, Kernel.font, Kernel.WhiteColor, x, y + 35);
        }
    }
}