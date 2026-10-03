/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Filesystem panel class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Mouse;
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
                Kernel.CurrentDirectory = AuraPath.AsDirectory(CurrentPath);
                Kernel.ApplicationManager.StartPackage("Terminal");
            });

            RightClickEntry entry2 = new("Paste", RightClick.Width, RightClick);
            entry2.Click = new Action(() =>
            {
                // Nothing can be created in the volume list ("/" is read-only).
                if (Kernel.Clipboard != null && !IsVolumeList())
                {
                    // Copies into the folder. ForceCopy throws on I/O errors, and a GUI click must not
                    // reach the crash screen.
                    try
                    {
                        Entries.ForceCopy(Kernel.Clipboard, CurrentPath);
                    }
                    catch (Exception ex)
                    {
                        Logs.DoOSLog("[Error] Cannot paste " + Kernel.Clipboard + ": " + ex.Message);
                    }

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

            // At "/" the entries are the mounted volumes ("/0", "/1"), which cannot be deleted.
            bool volumeList = IsVolumeList();

            string[] directories;
            string[] files;

            // GEN3-GAP(mounts): gen2 always had drive 0 mounted. In gen3 the path may be unmounted (no FAT
            // disk, live mode, a pulled USB stick): show an empty folder instead of faulting at boot.
            if (!string.IsNullOrEmpty(CurrentPath) && Directory.Exists(CurrentPath))
            {
                try
                {
                    directories = Directory.GetDirectories(CurrentPath);
                    files = Directory.GetFiles(CurrentPath);
                }
                catch (Exception)
                {
                    directories = Array.Empty<string>();
                    files = Array.Empty<string>();
                }
            }
            else
            {
                directories = Array.Empty<string>();
                files = Array.Empty<string>();
            }

            _buttons.Clear();
            Children.Clear();
            foreach (string directory in directories)
            {
                // GetDirectories returns full paths.
                string folderName = Path.GetFileName(directory);
                Bitmap folderIcon = volumeList ? Kernel.ResourceManager.GetIcon("16-drive.bmp") : Kernel.ResourceManager.GetIcon("32-folder.bmp");
                var button = new FileButton(folderName, _textColor, folderIcon, 0 + startX + currentX, 0 + currentY, 70, 70);
                button.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                    UpdateCurrentFolder();
                });

                int entryCount = volumeList ? 2 : 3;
                button.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, entryCount * RightClickEntry.ConstHeight);

                RightClickEntry entry = new("Open", button.RightClick.Width, button.RightClick);
                entry.Click = new Action(() =>
                {
                    OpenFolder(folderName);
                });

                RightClickEntry entry2 = new("Copy", button.RightClick.Width, button.RightClick);
                entry2.Click = new Action(() =>
                {
                    string path = Path.Combine(CurrentPath, folderName);
                    Kernel.Clipboard = path;
                });

                button.RightClick.AddEntry(entry);
                button.RightClick.AddEntry(entry2);

                // A volume is never deleted from here.
                if (!volumeList)
                {
                    RightClickEntry entry3 = new("Delete", button.RightClick.Width, button.RightClick);
                    entry3.Click = new Action(() =>
                    {
                        Delete(Path.Combine(CurrentPath, folderName));
                    });

                    button.RightClick.AddEntry(entry3);
                }

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
                    string path = Path.Combine(CurrentPath, fileName);
                    Kernel.Clipboard = path;
                });

                RightClickEntry entry3 = new("Delete", button.RightClick.Width, button.RightClick);
                entry3.Click = new Action(() =>
                {
                    Delete(Path.Combine(CurrentPath, fileName));
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
                Kernel.ApplicationManager.OpenFolder(AuraPath.AsDirectory(Path.Combine(CurrentPath, folderName)));
            }
            else
            {
                CurrentPath = AuraPath.AsDirectory(Path.Combine(CurrentPath, folderName));
            }
        }

        /// <summary>
        /// True when this panel shows the virtual root "/", whose entries are the mounted volumes.
        /// </summary>
        private bool IsVolumeList()
        {
            return CurrentPath == "/";
        }

        private void Delete(string path)
        {
            // ForceRemove refuses volumes and throws on I/O errors; a GUI click must not reach the
            // crash screen.
            try
            {
                Entries.ForceRemove(path);
            }
            catch (Exception ex)
            {
                Logs.DoOSLog("[Error] Cannot delete " + path + ": " + ex.Message);
            }

            RefreshFilesystem();
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