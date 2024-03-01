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

        public FilesystemPanel(string path, Color textcolor, int x, int y, int width, int height) : base(Color.Transparent, x, y, width, height)
        {
            Borders = false;

            CurrentPath = path;
            _textColor = textcolor;

            _buttons = new List<Button>();

            RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 3 * RightClickEntry.ConstHeight);

            RightClickEntry entry = new("Open in Terminal", RightClick.Width, RightClick);
            entry.Click = new Action(() =>
            {
                Kernel.CurrentDirectory = CurrentPath;
                Kernel.ApplicationManager.StartApplication(typeof(TerminalApp));
            });

            RightClickEntry entry2 = new("Paste", RightClick.Width, RightClick);
            entry2.Click = new Action(() =>
            {
                if (Kernel.Clipboard != null)
                {
                    Entries.ForceCopy(Kernel.Clipboard, CurrentPath);
                    RefreshFilesystem();
                    Kernel.Clipboard = null;
                }
            });

            RightClickEntry entry3 = new("Refresh", RightClick.Width, RightClick);
            entry3.Click = new Action(() =>
            {
                RefreshFilesystem();
            });

            RightClick.AddEntry(entry);
            RightClick.AddEntry(entry2);
            RightClick.AddEntry(entry3);
        }

        public override void Draw()
        {
            base.Draw();

            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

            foreach (var button in _buttons)
            {
                button.X = 0 + startX + currentX;
                button.Y = 0 + currentY;

                currentY += iconSpacing;
                if (currentY + iconSpacing > Height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
                button.Draw(this);
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

            _buttons.Clear();
            Children.Clear();
            foreach (string directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                var button = new FileButton(folderName, _textColor, Kernel.ResourceManager.GetIcon("32-folder.bmp"), 0 + startX + currentX, 0 + currentY, 70, 70);
                button.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                    UpdateCurrentFolder();
                });

                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 3 * RightClickEntry.ConstHeight);

                RightClickEntry entry = new("Open", button.RightClick.Width, button.RightClick);
                entry.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                });

                RightClickEntry entry2 = new("Copy", button.RightClick.Width, button.RightClick);
                entry2.Click = new Action(() =>
                {
                    string path = CurrentPath + folderName;
                    Kernel.Clipboard = path;
                });

                RightClickEntry entry3 = new("Delete", button.RightClick.Width, button.RightClick);
                entry3.Click = new Action(() =>
                {
                    Entries.ForceRemove(CurrentPath + folderName);
                    RefreshFilesystem();
                });

                button.RightClick.AddEntry(entry);
                button.RightClick.AddEntry(entry2);
                button.RightClick.AddEntry(entry3);

                _buttons.Add(button);
                AddChild(button);

                currentY += iconSpacing;
                if (currentY + iconSpacing > Height - 32)
                {
                    currentY = startY;
                    currentX += iconSpacing;
                }
            }

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                var button = new FileButton(fileName, _textColor, Kernel.ResourceManager.GetIcon("32-file.bmp"), 0 + startX + currentX, 0 + currentY, 70, 70);
                button.Click = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });

                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 3 * RightClickEntry.ConstHeight);

                RightClickEntry entry = new("Open", button.RightClick.Width, button.RightClick);
                entry.Click = new Action(() =>
                {
                    Kernel.ApplicationManager.StartFileApplication(fileName, CurrentPath);
                });

                RightClickEntry entry2 = new("Copy", button.RightClick.Width, button.RightClick);
                entry2.Click = new Action(() =>
                {
                    string path = CurrentPath + fileName;
                    Kernel.Clipboard = path;
                });

                RightClickEntry entry3 = new("Delete", button.RightClick.Width, button.RightClick);
                entry3.Click = new Action(() =>
                {
                    Entries.ForceRemove(CurrentPath + fileName);
                    RefreshFilesystem();
                });

                button.RightClick.AddEntry(entry);
                button.RightClick.AddEntry(entry2);
                button.RightClick.AddEntry(entry3);

                _buttons.Add(button);
                AddChild(button);

                currentY += iconSpacing;
                if (currentY + iconSpacing > Height - 32)
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

        public void RefreshFilesystem()
        {
            UpdateCurrentFolder();
            Clear(Color.Transparent);
            Draw();
            Parent.Draw();
            DrawInParent();
        }
    }

    public class FileButton : Button
    {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public Bitmap Icon { get; private set; }

        public FileButton(string filePath, Color textColor, Bitmap icon, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            NoBackground = true;
            FileName = filePath;
            Icon = icon;
            TextColor = textColor;
        }

        public override void Draw()
        {
            base.Draw();

            int imageY = 4;
            DrawImage(Icon, (Width / 2) - ((int)Icon.Width / 2), imageY);

            int textX = 0;
            int textY = 37;
            int maxWidth = Width;
            int charWidth = Kernel.font.Width;
            int maxCharsPerLine = maxWidth / charWidth;

            if (FileName.Length * charWidth > maxWidth)
            {
                for (int startIdx = 0; startIdx < FileName.Length; startIdx += maxCharsPerLine)
                {
                    string segment = FileName.Substring(startIdx, Math.Min(maxCharsPerLine, FileName.Length - startIdx));

                    DrawString(segment, Kernel.font, TextColor, textX, textY);

                    textY += Kernel.font.Height;
                }
            }
            else
            {
                DrawString(FileName, Kernel.font, TextColor, textX, textY);
            }
        }
    }
}