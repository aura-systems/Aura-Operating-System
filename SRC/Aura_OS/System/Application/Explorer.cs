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

namespace Aura_OS
{
    public class Explorer : App
    {
        public static string ApplicationName = "Explorer";

        string CurrentPath = "";
        public bool Clicked = false;
        TextBox PathTextBox;
        Button Up;
        List<Button> Buttons;

        public Explorer(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            CurrentPath = Kernel.CurrentVolume;
            PathTextBox = new TextBox(x + 18 + 6, y + 3, width - 15 - 18, 18, CurrentPath);
            Buttons = new List<Button>();
            Up = new Button(ResourceManager.GetImage("16-up.bmp"), x + 3, y + 3, 18, 18);
            Up.Action = new Action(() =>
            {
                CurrentPath = RemoveLastDirectory(CurrentPath);
            });
            UpdateCurrentFolder();
        }

        public string RemoveLastDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path.Remove(2, path.Length - 2);
            int lastSeparatorIndex = path.LastIndexOf("\\");
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
            int startX = 3;
            int startY = 24 + 3;
            int iconSpacing = 60;

            int currentX = startX;
            int currentY = startY;

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

            foreach(var button in Buttons)
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

            PathTextBox.X = x + 9 + 18;
            PathTextBox.Y = y + 3;
            PathTextBox.Update();
            Up.X = x + 3;
            Up.Y = y + 3;
            Up.Update();
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
            Kernel.canvas.DrawString(FileName, Kernel.font, Kernel.WhiteColor, base.X, base.Y + 35);
        }
    }
}
