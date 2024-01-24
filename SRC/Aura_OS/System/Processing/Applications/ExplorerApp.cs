/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics;
using Cosmos.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using Cosmos.System;
using System.Collections.Generic;
using System.Drawing;

namespace Aura_OS.System.Processing.Application
{
    public class ExplorerApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Explorer";

        public bool Clicked = false;
        public bool RightClicked = false;
        public Panel TopPanel;
        public Panel LeftPanel;
        public FilesystemPanel MainPanel;
        Button SpaceButton;
        TextBox PathTextBox;
        Button Up;
        List<Button> Disks;

        public ExplorerApp(string currentPath, int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            Window.Icon = ResourceManager.GetImage("16-explorer.bmp");

            TopPanel = new Panel(Kernel.Gray, x + 1, y + 1, width - 6, 23);
            TopPanel.Borders = true;
            SpaceButton = new Button("", x + 3, y + Window.Height - 19, Window.Width - 6, 20);
            SpaceButton.Light = true;
            MainPanel = new FilesystemPanel(currentPath, Color.Black, Kernel.WhiteColor, x + 1 + 75, y + 1 + 22, width - 7 - 75, Window.Height - Window.TopBar.Height - TopPanel.Height - SpaceButton.Height - 8);
            MainPanel.Borders = true;

            LeftPanel = new Panel(Kernel.WhiteColor, x + 1, y + 1 + 22, 75, Window.Height - Window.TopBar.Height - TopPanel.Height - SpaceButton.Height - 8);
            LeftPanel.Borders = true;
            PathTextBox = new TextBox(x + 18 + 6, y + 3, width - 15 - 18, 18, MainPanel.CurrentPath);
            Up = new Button(ResourceManager.GetImage("16-up.bmp"), x + 3, y + 3, 18, 18);
            Up.Action = new Action(() =>
            {
                MainPanel.CurrentPath = Filesystem.Utils.GetParentPath(MainPanel.CurrentPath);
                PathTextBox.Text = MainPanel.CurrentPath;
                MainPanel.UpdateCurrentFolder(x, y, height);
            });

            MainPanel.UpdateCurrentFolder(x, y, height);
            UpdateDisks();
        }

        public void UpdateDisks()
        {
            Disks = new List<Button>();
            var vols = Kernel.VirtualFileSystem.GetVolumes();

            foreach (var vol in vols)
            {
                string path = vol.mName;
                Bitmap icon = null;

                if (Kernel.VirtualFileSystem.GetFileSystemType(vol.mName).Equals("ISO9660"))
                {
                    icon = ResourceManager.GetImage("16-drive-readonly.bmp");
                }
                else
                {
                    icon = ResourceManager.GetImage("16-drive.bmp");
                }

                var button = new Button(icon, path, x, y + y, 16 + path.Length * 8, 16);
                button.TextColor = Color.Black;
                button.Text = path;
                button.NoBorder = true;
                button.NoBackground = true;
                button.Action = new Action(() =>
                {
                    MainPanel.CurrentPath = path;
                    Kernel.CurrentVolume = path;
                    Kernel.CurrentDirectory = path;
                    PathTextBox.Text = path;
                    MainPanel.UpdateCurrentFolder(x, y, height);
                });

                Disks.Add(button);
            }
        }

        public override void UpdateApp()
        {
            TopPanel.X = x + 1;
            TopPanel.Y = y + 1;
            TopPanel.Update();
            LeftPanel.X = x + 1;
            LeftPanel.Y = y + TopPanel.Height;
            LeftPanel.Update();
            MainPanel.X = x + 1 + 75;
            MainPanel.Y = y + TopPanel.Height;
            MainPanel.Update(x + 78, y, height);
            PathTextBox.Text = MainPanel.CurrentPath;
            PathTextBox.X = x + 9 + 18;
            PathTextBox.Y = y + 3;
            PathTextBox.Update();
            Up.X = x + 3;
            Up.Y = y + 3;
            Up.Update();
            SpaceButton.Text = "Free Space: " + Filesystem.Utils.GetFreeSpace() + ", Capacity: " + Filesystem.Utils.GetCapacity() + ", Filesystem: " + Kernel.VirtualFileSystem.GetFileSystemType(Kernel.CurrentVolume);
            SpaceButton.X = x;
            SpaceButton.Y = y + Window.Height - Window.TopBar.Height - 26;
            SpaceButton.Update();

            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 19;

            int currentX = startX;
            int currentY = startY;

            foreach (var button in Disks)
            {
                button.X = x + startX + currentX;
                button.Y = y + currentY;

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }

                button.Update();
            }
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            if (Up.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Up.Action();
                return;
            }

            foreach (var button in Disks)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.Action != null)
                    {
                        button.Action();
                        return;
                    }
                }
            }

            MainPanel.HandleLeftClick();
        }

        public override void HandleRightClick()
        {
            base.HandleRightClick();

            MainPanel.HandleRightClick();
        }
    }
}
