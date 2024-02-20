/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Filesystem panel class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Applications.Terminal;
using Aura_OS.System.Processing.Processes;
using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class FilesystemPanel : Panel
    {
        public string CurrentPath;
        public bool OpenNewWindow = false;

        private List<Button> _buttons;
        private Color _textColor;

        public FilesystemPanel(string path, Color textcolor, Color background, int x, int y, int width, int height) : base(background, x, y, width, height)
        {
            CurrentPath = path;
            _textColor = textcolor;

            _buttons = new List<Button>();

            /*
            RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 1 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();

            RightClickEntry entry2 = new("Open in Terminal", 0, 0, RightClick.Width);
            entry2.Click = new Action(() =>
            {
                Kernel.CurrentDirectory = CurrentPath;
                Kernel.ApplicationManager.StartApplication(typeof(TerminalApp));
            });

            rightClickEntries.Add(entry2);
            RightClickEntry entryPaste = new("Paste", 0, 0, RightClick.Width);
            entryPaste.Click = new Action(() =>
            {
                if (Kernel.Clipboard != null)
                {
                    Entries.ForceCopy(Kernel.Clipboard, CurrentPath);
                    UpdateCurrentFolder(x, y, height);
                    Kernel.Clipboard = null;
                }
            });
            rightClickEntries.Add(entryPaste);

            RightClickEntry entryRefresh = new("Refresh", 0, 0, RightClick.Width);
            entryRefresh.Click = new Action(() =>
            {
                UpdateCurrentFolder(x, y, height);
            });
            rightClickEntries.Add(entryRefresh);

            RightClick.Entries = rightClickEntries;
            /*

            */
        }

        public void Draw(int x, int y, int height)
        {
            base.Draw();

            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            foreach (var button in _buttons)
            {
                button.X = x + startX + currentX;
                button.Y = y + currentY;

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
                AddChild(button);
                button.Draw();
            }

            if (RightClick != null && RightClick.Opened)
            {
                foreach (var entry in RightClick.Entries)
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
                RightClick.Draw();
            }

            foreach (var button in _buttons)
            {
                if (button.RightClick != null && button.RightClick.Opened)
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
                    button.RightClick.Draw();

                    return;
                }
            }
        }

        public override void HandleLeftClick()
        {
            foreach (var button in _buttons)
            {
                if (button.RightClick.Opened)
                {
                    button.RightClick.Opened = false;

                    foreach (var entry in button.RightClick.Entries)
                    {
                        if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                        {
                            entry.Click();
                            return;
                        }
                    }
                }

                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.Click != null)
                    {
                        button.Click();
                        return;
                    }
                }
            }

            base.HandleLeftClick();
        }

        public override void HandleRightClick()
        {
            foreach (var button in _buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (button.RightClick != null && button.RightClick.Opened)
                    {
                        return;
                    }

                    button.RightClick.X = (int)MouseManager.X;
                    button.RightClick.Y = (int)MouseManager.Y;
                    button.RightClick.Opened = true;
                    return;
                }
            }

            base.HandleRightClick();
        }

        public void UpdateCurrentFolder(int x, int y, int height)
        {
            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            string[] directories = Directory.GetDirectories(CurrentPath);
            string[] files = Directory.GetFiles(CurrentPath);

            _buttons.Clear();
            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                var button = new FileButton(folderName, _textColor, Kernel.ResourceManager.GetIcon("32-folder.bmp"), x + startX + currentX, y + currentY, 32, 32);
                button.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                    UpdateCurrentFolder(x, y, height);
                });

                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 3 * RightClickEntry.ConstHeight);
                List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
                RightClickEntry entry = new("Open", 0, 0, button.RightClick.Width);
                entry.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                    UpdateCurrentFolder(x, y, height);
                });
                rightClickEntries.Add(entry);

                RightClickEntry copyEntry = new("Copy", 0, 0, button.RightClick.Width);
                copyEntry.Click = new Action(() =>
                {
                    string path = CurrentPath + folderName;
                    Kernel.Clipboard = path;
                });
                rightClickEntries.Add(copyEntry);

                RightClickEntry entry2 = new("Delete", 0, 0, button.RightClick.Width);
                entry2.Click = new Action(() =>
                {
                    Entries.ForceRemove(CurrentPath + folderName);
                    UpdateCurrentFolder(x, y, height);
                });
                rightClickEntries.Add(entry2);

                button.RightClick.Entries = rightClickEntries;

                _buttons.Add(button);

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
                var button = new FileButton(fileName, _textColor, Kernel.ResourceManager.GetIcon("32-file.bmp"), x + startX + currentX, y + currentY, 32, 32);
                button.Click = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });

                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 1 * RightClickEntry.ConstHeight);
                List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();

                RightClickEntry entry = new("Open", 0, 0, button.RightClick.Width);
                entry.Click = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });
                rightClickEntries.Add(entry);

                RightClickEntry copyEntry = new("Copy", 0, 0, button.RightClick.Width);
                copyEntry.Click = new Action(() =>
                {
                    string path = CurrentPath + fileName;
                    Kernel.Clipboard = path;
                });
                rightClickEntries.Add(copyEntry);

                RightClickEntry entry2 = new("Delete", 0, 0, button.RightClick.Width);
                entry2.Click = new Action(() =>
                {
                    Entries.ForceRemove(CurrentPath + fileName);
                    UpdateCurrentFolder(x, y, height);
                });
                rightClickEntries.Add(entry2);

                button.RightClick.Entries = rightClickEntries;

                _buttons.Add(button);

                currentY += iconSpacing;
                if (currentY + iconSpacing > height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }
        }

        public void OpenFolder(string folderName)
        {
            if (OpenNewWindow)
            {
                /*
                ExplorerApp app = new(CurrentPath + folderName + "\\", 500, 400, 40, 40);
                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
                */
            }
            else
            {
                CurrentPath = CurrentPath + folderName + "\\";
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
        public Color TextColor { get; private set; }

        public FileButton(string filePath, Color textColor, Bitmap icon, int x, int y, int width, int height)
            : base(icon, x, y, width, height)
        {
            NoBorder = true;
            NoBackground = true;
            FileName = filePath;
            Icon = icon;
            X = x;
            Y = y;
            TextColor = textColor;
        }

        public override void Draw()
        {
            base.Draw();

            DrawString(FileName, Kernel.font, TextColor, base.X, base.Y + 35);
        }
    }
}