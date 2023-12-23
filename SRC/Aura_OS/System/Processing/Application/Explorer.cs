/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics;
using Cosmos.System.Graphics;
using System.IO;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using Cosmos.System;
using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System.Filesystem;
using static Cosmos.HAL.PCIDevice;

namespace Aura_OS.System.Processing.Application
{
    public class Explorer : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Explorer";

        string CurrentPath = "";
        public bool Clicked = false;
        public bool RightClicked = false;
        public Panel TopPanel;
        public Panel LeftPanel;
        public Panel MainPanel;
        Button SpaceButton;
        TextBox PathTextBox;
        Button Up;
        List<Button> Buttons;
        List<Button> Disks;

        public Explorer(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            CurrentPath = Kernel.CurrentVolume;
            TopPanel = new Panel(Kernel.Gray, x + 1, y + 1, width - 6, 23);
            TopPanel.Borders = true;
            SpaceButton = new Button("", x + 3, y + Window.Height - 19, Window.Width - 6, 20);
            SpaceButton.Light = true;
            MainPanel = new Panel(Kernel.WhiteColor, x + 1 + 75, y + 1 + 22, width - 7 - 75, Window.Height - Window.TopBar.Height - TopPanel.Height - SpaceButton.Height - 8);
            MainPanel.Borders = true;
            MainPanel.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 1 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
            RightClickEntry entry2 = new("Open in Terminal", 0, 0, MainPanel.RightClick.Width);
            entry2.Action = new Action(() =>
            {
                Kernel.CurrentDirectory = CurrentPath;
                var app = new Terminal(700, 600, 40, 40);
                Kernel.console = app;
                app.Initialize();

                Kernel.WindowManager.apps.Add(app);
                app.zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
                Kernel.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Kernel.Taskbar.UpdateApplicationButtons();
                Kernel.WindowManager.UpdateFocusStatus();

                MainPanel.RightClick.Opened = false;
            });
            rightClickEntries.Add(entry2);
            MainPanel.RightClick.Entries = rightClickEntries;
            LeftPanel = new Panel(Kernel.WhiteColor, x + 1, y + 1 + 22, 75, Window.Height - Window.TopBar.Height - TopPanel.Height - SpaceButton.Height - 8);
            LeftPanel.Borders = true;
            PathTextBox = new TextBox(x + 18 + 6, y + 3, width - 15 - 18, 18, CurrentPath);
            Buttons = new List<Button>();
            Up = new Button(ResourceManager.GetImage("16-up.bmp"), x + 3, y + 3, 18, 18);
            Up.Action = new Action(() =>
            {
                CurrentPath = Filesystem.Utils.GetParentPath(CurrentPath);
                PathTextBox.Text = CurrentPath;
                UpdateCurrentFolder();
            });
            UpdateCurrentFolder();
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
                    CurrentPath = path;
                    Kernel.CurrentVolume = path;
                    Kernel.CurrentDirectory = path;
                    PathTextBox.Text = path;
                    UpdateCurrentFolder();
                });
                Disks.Add(button);
            }
        }

        public void UpdateCurrentFolder()
        {
            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            string[] directories = Directory.GetDirectories(CurrentPath);
            string[] files = Directory.GetFiles(CurrentPath);

            Buttons.Clear();
            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                var button = new FileButton(folderName, ResourceManager.GetImage("32-folder.bmp"), x + startX + currentX, y + currentY, 32, 32);
                button.Action = new Action(() =>
                {
                    CurrentPath = CurrentPath + directory + "\\";
                    PathTextBox.Text = CurrentPath;
                    UpdateCurrentFolder();
                });

                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 2 * RightClickEntry.ConstHeight);
                List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
                RightClickEntry entry = new("Open", 0, 0, button.RightClick.Width);
                entry.Action = new Action(() =>
                {
                    string error;
                    string path = CurrentPath + folderName;
                    PathTextBox.Text = path;
                    CurrentPath = path;
                    UpdateCurrentFolder();
                    button.RightClick.Opened = false;
                });
                rightClickEntries.Add(entry);
                RightClickEntry entry2 = new("Delete", 0, 0, button.RightClick.Width);
                entry2.Action = new Action(() =>
                {
                    Entries.ForceRemove(CurrentPath + folderName);
                    UpdateCurrentFolder();
                    button.RightClick.Opened = false;
                });
                rightClickEntries.Add(entry2);
                button.RightClick.Entries = rightClickEntries;

                Buttons.Add(button);

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                var button = new FileButton(fileName, ResourceManager.GetImage("32-file.bmp"), x + startX + currentX, y + currentY, 32, 32);
                button.Action = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });
                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 1 * RightClickEntry.ConstHeight);
                List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
                RightClickEntry entry = new("Open", 0, 0, button.RightClick.Width);
                entry.Action = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });
                rightClickEntries.Add(entry);
                button.RightClick.Entries = rightClickEntries;
                Buttons.Add(button);

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }
        }

        public override void UpdateApp()
        {
            if (MouseManager.MouseState == MouseState.Left)
            {
                if (!Clicked)
                {
                    Clicked = true;
                }
            }
            else if (MouseManager.MouseState == MouseState.Right)
            {
                if (!RightClicked)
                {
                    RightClicked = true;
                }
            }
            else if (Clicked)
            {
                Clicked = false;
                HandleClick();
            }
            else if (RightClicked)
            {
                RightClicked = false;
                HandleRightClick();
            }

            TopPanel.X = x + 1;
            TopPanel.Y = y + 1;
            TopPanel.Update();
            LeftPanel.X = x + 1;
            LeftPanel.Y = y + TopPanel.Height;
            LeftPanel.Update();
            MainPanel.X = x + 1 + 75;
            MainPanel.Y = y + TopPanel.Height;
            MainPanel.Update();
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
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            foreach (var button in Buttons)
            {
                button.X = x + 78 + startX + currentX;
                button.Y = y + currentY;

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }

                button.Update();
            }

            startX = 3;
            startY = 24 + 3;
            iconSpacing = 19;

            currentX = startX;
            currentY = startY;

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

            foreach (var button in Buttons)
            {
                if (button.RightClick.Opened)
                {
                    foreach (var entry in button.RightClick.Entries)
                    {
                        if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                        {
                            entry.BackColor = Kernel.DarkBlue;
                            entry.TextColor = Kernel.WhiteColor;
                        }
                        else
                        {
                            entry.BackColor = Color.LightGray;
                            entry.TextColor = Kernel.BlackColor;
                        }
                    }
                    button.RightClick.Update();

                    return;
                }
            }

            if (MainPanel.RightClick.Opened)
            {
                foreach (var entry in MainPanel.RightClick.Entries)
                {
                    if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        entry.BackColor = Kernel.DarkBlue;
                        entry.TextColor = Kernel.WhiteColor;
                    }
                    else
                    {
                        entry.BackColor = Color.LightGray;
                        entry.TextColor = Kernel.BlackColor;
                    }
                }
                MainPanel.RightClick.Update();
            }
        }

        private void HandleRightClick()
        {
            foreach (var button in Buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    button.RightClick.X = (int)MouseManager.X;
                    button.RightClick.Y = (int)MouseManager.Y;
                    button.RightClick.Opened = true;
                    return;
                }
            }

            if (MainPanel.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                MainPanel.RightClick.X = (int)MouseManager.X;
                MainPanel.RightClick.Y = (int)MouseManager.Y;
                MainPanel.RightClick.Opened = true;
                return;
            }
        }

        private void HandleClick()
        {
            if (Up.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Up.Action();
                return;
            }

            foreach (var button in Buttons)
            {
                button.RightClick.Opened = false;

                foreach (var entry in button.RightClick.Entries)
                {
                    if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        entry.Action();
                        return;
                    }
                }

                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.Action != null)
                    {
                        button.Action();
                        return;
                    }
                }
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

            if (MainPanel.RightClick != null)
            {
                MainPanel.RightClick.Opened = false;
            }
        }
    }

    public class FileButton : Button
    {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public Bitmap Icon { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public FileButton(string filePath, Bitmap icon, int x, int y, int width, int height)
            : base(icon, x, y, width, height)
        {
            NoBorder = true;
            NoBackground = true;
            FileName = filePath;
            Icon = icon;
            X = x;
            Y = y;
        }

        public override void Update()
        {
            base.Update();
            Kernel.canvas.DrawString(FileName, Kernel.font, Kernel.BlackColor, base.X, base.Y + 35);
        }
    }
}
