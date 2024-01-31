/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File explorer application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics;
using Cosmos.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using Cosmos.System;
using System.Collections.Generic;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using System.IO;

namespace Aura_OS.System.Processing.Applications
{
    public class ExplorerApp : Application
    {
        public static string ApplicationName = "Explorer";

        private Panel _topPanel;
        private Panel _leftPanel;
        private FilesystemPanel _mainPanel;
        private Button _spaceButton;
        private TextBox _pathTextBox;
        private Button _up;
        private List<Button> _disks;

        private string _path = "";

        public ExplorerApp(string currentPath, int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            Window.Icon = ResourceManager.GetImage("16-explorer.bmp");

            _topPanel = new Panel(Kernel.Gray, x + 1, y + 1, width - 6, 23);
            _topPanel.Borders = true;
            _spaceButton = new Button("", x + 3, y + Window.Height - 19, Window.Width - 6, 20);
            _spaceButton.Light = true;
            _mainPanel = new FilesystemPanel(currentPath, Color.Black, Kernel.WhiteColor, x + 1 + 75, y + 1 + 22, width - 7 - 75, Window.Height - Window.TopBar.Height - _topPanel.Height - _spaceButton.Height - 8);
            _mainPanel.Borders = true;

            _leftPanel = new Panel(Kernel.WhiteColor, x + 1, y + 1 + 22, 75, Window.Height - Window.TopBar.Height - _topPanel.Height - _spaceButton.Height - 8);
            _leftPanel.Borders = true;
            _pathTextBox = new TextBox(x + 18 + 6, y + 3, width - 15 - 18, 18, _mainPanel.CurrentPath);
            _pathTextBox.Enter = new Action(() =>
            {
                if (Directory.Exists(_pathTextBox.Text))
                {
                    _mainPanel.CurrentPath = _pathTextBox.Text;
                    _mainPanel.UpdateCurrentFolder(x, y, height);
                }
            });
            _up = new Button(ResourceManager.GetImage("16-up.bmp"), x + 3, y + 3, 18, 18);
            _up.Click = new Action(() =>
            {
                _mainPanel.CurrentPath = Filesystem.Utils.GetParentPath(_mainPanel.CurrentPath);
                _pathTextBox.Text = _mainPanel.CurrentPath;
                _mainPanel.UpdateCurrentFolder(x, y, height);
            });

            _mainPanel.UpdateCurrentFolder(x, y, height);
            UpdateDisks();
        }

        public void UpdateDisks()
        {
            _disks = new List<Button>();
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

                var button = new Button(icon, path, X, Y + Y, 16 + path.Length * 8, 16);
                button.TextColor = Color.Black;
                button.Text = path;
                button.NoBorder = true;
                button.NoBackground = true;
                button.Click = new Action(() =>
                {
                    _mainPanel.CurrentPath = path;
                    Kernel.CurrentVolume = path;
                    Kernel.CurrentDirectory = path;
                    _pathTextBox.Text = path;
                    _mainPanel.UpdateCurrentFolder(X, Y, Height);
                });

                _disks.Add(button);
            }
        }

        public override void Update()
        {
            base.Update();

            if (_path != _mainPanel.CurrentPath)
            {
                _path = _mainPanel.CurrentPath;
                _pathTextBox.Text = _path;
            }

            _topPanel.X = X + 1;
            _topPanel.Y = Y + 1;
            _topPanel.Update();
            _leftPanel.X = X + 1;
            _leftPanel.Y = Y + _topPanel.Height;
            _leftPanel.Update();
            _mainPanel.X = X + 1 + 75;
            _mainPanel.Y = Y + _topPanel.Height;
            _mainPanel.Update();
            _pathTextBox.X = X + 9 + 18;
            _pathTextBox.Y = Y + 3;
            _pathTextBox.Update();
            _up.X = X + 3;
            _up.Y = Y + 3;
            _up.Update();
            _spaceButton.Text = "Free Space: " + Filesystem.Utils.GetFreeSpace() + ", Capacity: " + Filesystem.Utils.GetCapacity() + ", Filesystem: " + Kernel.VirtualFileSystem.GetFileSystemType(Kernel.CurrentVolume);
            _spaceButton.X = X;
            _spaceButton.Y = Y + Window.Height - Window.TopBar.Height - 26;
            _spaceButton.Update();

            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 19;

            int currentX = startX;
            int currentY = startY;

            foreach (var button in _disks)
            {
                button.X = X + startX + currentX;
                button.Y = Y + currentY;

                currentY += iconSpacing;
                if (currentY + iconSpacing > Height - 32)
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

            if (_up.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                _up.Click();
                return;
            }

            foreach (var button in _disks)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.Click != null)
                    {
                        button.Click();
                        return;
                    }
                }
            }

            _mainPanel.HandleLeftClick();
            _pathTextBox.HandleLeftClick();
        }

        public override void HandleRightClick()
        {
            base.HandleRightClick();

            _mainPanel.HandleRightClick();
        }

        public override void Draw()
        {
            if (Dirty)
            {
                base.Draw();

                _topPanel.Draw();
                _leftPanel.Draw();
                _mainPanel.Draw(X + 78, Y, Height);
                _pathTextBox.Draw();
                _up.Draw();
                _spaceButton.Draw();

                foreach (var button in _disks)
                {
                    button.Draw();
                }
            }
        }
    }
}
