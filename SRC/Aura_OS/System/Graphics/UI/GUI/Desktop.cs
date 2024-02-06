/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System.Drawing;
using Aura_OS.System.Filesystem;
using System.Collections.Generic;
using Cosmos.System;
using Aura_OS.System.Processing.Applications.Terminal;
using Aura_OS.System.Processing.Applications;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public FilesystemPanel MainPanel;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            /*
            MainPanel = new FilesystemPanel(Kernel.CurrentVolume, Color.White, Kernel.WhiteColor, x + 4, y + 16 + 4, width - 7 - 75, height - Taskbar.taskbarHeight);
            MainPanel.OpenNewWindow = true;
            MainPanel.Borders = false;
            MainPanel.Background = false;
            

            MainPanel.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 5 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();

            RightClickEntry entry = new("Open in Explorer", 0, 0, MainPanel.RightClick.Width);
            entry.Click = new Action(() =>
            {
                Kernel.ApplicationManager.StartApplication(typeof(ExplorerApp));
            });
            rightClickEntries.Add(entry);

            RightClickEntry entry2 = new("Open in Terminal", 0, 0, MainPanel.RightClick.Width);
            entry2.Click = new Action(() =>
            {
                Kernel.ApplicationManager.StartApplication(typeof(TerminalApp));
            });

            rightClickEntries.Add(entry2);

            RightClickEntry entryInfo = new("OS Information", 0, 0, MainPanel.RightClick.Width);
            entryInfo.Click = new Action(() =>
            {
                Kernel.ApplicationManager.StartApplication(typeof(SystemInfoApp));
            });
            rightClickEntries.Add(entryInfo);

            RightClickEntry entryPaste = new("Paste", 0, 0, MainPanel.RightClick.Width);
            entryPaste.Click = new Action(() =>
            {
                if (Kernel.Clipboard != null)
                {
                    Entries.ForceCopy(Kernel.Clipboard, MainPanel.CurrentPath);
                    MainPanel.UpdateCurrentFolder(x, y, height);
                    Kernel.Clipboard = null;
                }
            });
            rightClickEntries.Add(entryPaste);

            RightClickEntry entryRefresh = new("Refresh Desktop", 0, 0, MainPanel.RightClick.Width);
            entryRefresh.Click = new Action(() =>
            {
                MainPanel.CurrentPath = Kernel.CurrentVolume;
                MainPanel.UpdateCurrentFolder(x, y, height);
                MarkDirty();
            });
            rightClickEntries.Add(entryRefresh);

            MainPanel.RightClick.Entries = rightClickEntries;

            MainPanel.UpdateCurrentFolder(x, y, height);
            */
        }

        public override void Draw()
        {
            //canvas.Clear(0x000000);
            //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

            DrawImage(Kernel.wallpaper, X, Y);

            //MainPanel.Draw(X, Y, Height);
        }

        public override void HandleLeftClick()
        {
            // MainPanel.HandleLeftClick();
        }

        public override void HandleRightClick()
        {
            // MainPanel.HandleRightClick();
        }
    }
}