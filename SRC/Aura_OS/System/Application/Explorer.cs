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

namespace Aura_OS
{
    public class Explorer : App
    {
        public static string ApplicationName = "Explorer";

        string CurrentPath = "";
        public bool Clicked = false;
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
            LeftPanel = new Panel(Kernel.WhiteColor, x + 1, y + 1 + 22, 75, Window.Height - Window.TopBar.Height - TopPanel.Height - SpaceButton.Height - 8);
            LeftPanel.Borders = true;
            PathTextBox = new TextBox(x + 18 + 6, y + 3, width - 15 - 18, 18, CurrentPath);
            Buttons = new List<Button>();
            Up = new Button(ResourceManager.GetImage("16-up.bmp"), x + 3, y + 3, 18, 18);
            Up.Action = new Action(() =>
            {
                CurrentPath = RemoveLastDirectory(CurrentPath);
                PathTextBox.Text = CurrentPath;
                UpdateCurrentFolder();
            });
            UpdateCurrentFolder();

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
                var button = new Button(icon, path, x, y + y, 16 + (path.Length * 8), 16);
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

        public string RemoveLastDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }

            int lastSeparatorIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastSeparatorIndex <= 0)
            {
                return path + "\\";
            }
            if (lastSeparatorIndex == 2 && path[1] == ':')
            {
                return path.Substring(0, lastSeparatorIndex + 1);
            }
            return path.Substring(0, lastSeparatorIndex) + "\\";
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
                    if (fileName.EndsWith(".bmp"))
                    {
                        string path = CurrentPath + fileName;
                        string name = fileName;
                        byte[] bytes = File.ReadAllBytes(path);
                        Bitmap bitmap = new Bitmap(bytes);
                        int width = name.Length * 8 + 50;

                        if (width < bitmap.Width)
                        {
                            width = (int)bitmap.Width + 1;
                        }

                        var app = new Picture(name, bitmap, width, (int)bitmap.Height + 20);

                        app.Initialize();

                        Kernel.WindowManager.apps.Add(app);
                        app.zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
                        Kernel.WindowManager.MarkStackDirty();

                        app.Visible = true;
                        app.Focused = true;

                        Kernel.ProcessManager.Start(app);

                        Kernel.dock.UpdateApplicationButtons();
                        Kernel.WindowManager.UpdateFocusStatus();
                    }
                });
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
            else if (Clicked)
            {
                Clicked = false;
                HandleClick();
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
            SpaceButton.Text = "Free Space: " + GetFreeSpace() + ", Capacity: " + GetCapacity() + ", Filesystem:" + Kernel.VirtualFileSystem.GetFileSystemType(Kernel.CurrentVolume);
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
                button.X = x + 75 + startX + currentX;
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
        }

        private void HandleClick()
        {
            if (Up.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Up.Action();
            }

            foreach (var button in Buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.Action != null)
                    {
                        button.Action();
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
                    }
                }
            }
        }

        private string GetFreeSpace()
        {
            var available_space = Kernel.VirtualFileSystem.GetAvailableFreeSpace(Kernel.CurrentVolume);
            return ConvertSize(available_space);
        }

        private string GetCapacity()
        {
            var total_size = Kernel.VirtualFileSystem.GetTotalSize(Kernel.CurrentVolume);
            return ConvertSize(total_size);
        }

        public string ConvertSize(long bytes)
        {
            string suffix = "Bytes";
            double size = bytes;

            if (size >= 1024 * 1024 * 1024)
            {
                size /= 1024 * 1024 * 1024;
                suffix = "GB";
            }
            else if (size >= 1024 * 1024)
            {
                size /= 1024 * 1024;
                suffix = "MB";
            }
            else if (size >= 1024)
            {
                size /= 1024;
                suffix = "KB";
            }

            return $"{Round(size)}{suffix}";
        }

        public string Round(double number)
        {
            string numStr = number.ToString();
            int dotIndex = numStr.IndexOf('.');

            if (dotIndex != -1)
            {
                return numStr.Substring(0, dotIndex);
            }
            return numStr;
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
