﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File editor application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Collections.Generic;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Processing.Applications
{
    public class EditorApp : Application
    {
        public static string ApplicationName = "Editor";

        private Panel _topPanel;
        private Button _save;
        private TextBox _fileContentBox;
        private string _filePath;

        private Dialog _dialog;
        private bool _showDialog;

        public EditorApp(string filePath, int width, int height, int x = 0, int y = 0)
            : base(ApplicationName + " - " + filePath, width, height, x, y)
        {
            _topPanel = new Panel(Kernel.Gray, 3, Window.TopBar.Height + 3, width - 6, 22);
            _topPanel.Borders = true;
            AddChild(_topPanel);

            string text = "Save";
            int textWidth = (text.Length + 2) * (Kernel.font.Width);
            _save = new Button(text, 5, Window.TopBar.Height + 5, textWidth, 18);
            _save.Click = new Action(() =>
            {
                SaveFile();
            });
            AddChild(_save);

            _filePath = filePath;

            _fileContentBox = new TextBox(3, _topPanel.Height + Window.TopBar.Height + 3, width - 5, height - _topPanel.Height - Window.TopBar.Height - 6, "");
            _fileContentBox.Multiline = true;
            _fileContentBox.Text = File.ReadAllText(filePath);
            AddChild(_fileContentBox);

            _dialog = new("Save", "Your file has been saved!", (int)Width / 2 - 302 / 2, Height / 2 - 119 / 2);
            _dialog.Visible = false;
            _dialog.AddButton("OK", new Action(() =>
            {
                _showDialog = false;
                _dialog.Visible = false;

                foreach (var child in Window.Children)
                {
                    child.MarkDirty();
                }
                MarkDirty();
            }));
            _dialog.Close.Click = new Action(() =>
            {
                _showDialog = false;
                _dialog.Visible = false;

                foreach (var child in Window.Children)
                {
                    child.MarkDirty();
                }
                MarkDirty();
            });
            AddChild(_dialog);
        }

        private void SaveFile()
        {
            File.WriteAllText(_filePath, _fileContentBox.Text);
            _showDialog = true;
            _dialog.Visible = true;
            Explorer.WindowManager.BringToFront(_dialog);

            foreach (var child in Window.Children)
            {
                child.MarkDirty();
            }
            MarkDirty();
        }

        public override void Update()
        {
            base.Update();

            if (_showDialog)
            {
                _dialog.Update();
            }
            else
            {
                _save.Update();
                _fileContentBox.Update();
            }
        }

        public override void HandleLeftClick()
        {
            if (!_showDialog)
            {
                base.HandleLeftClick();
                _fileContentBox.HandleLeftClick();
            }
            else
            {
                _save.HandleLeftClick();
            }

            List<Button> buttons = _dialog.GetButtons();
            if (buttons[0].IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y))
            {
                buttons[0].Click();
            }
        }

        public override void Draw()
        {
            base.Draw();

            _fileContentBox.Draw();
            _fileContentBox.DrawInParent();
            _topPanel.Draw();
            _topPanel.DrawInParent();
            _save.Draw();
            _save.DrawInParent();

            if (_showDialog)
            {
                _dialog.Draw();
                _dialog.DrawInParent();
            }
        }
    }
}
