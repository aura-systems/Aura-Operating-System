/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File editor application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using Aura_OS.System.Graphics.UI.GUI;
using System.IO;
using System.Collections.Generic;

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
            _topPanel = new Panel(Kernel.Gray, x + 1, y + 1, width - 6, 22);
            _topPanel.Borders = true;

            string text = "Save";
            int textWidth = (text.Length + 2) * (Kernel.font.Width);
            _save = new Button(text, x + 3, y + 3, textWidth, 18);
            _save.Click = new Action(() =>
            {
                SaveFile();
            });

            _filePath = filePath;

            _fileContentBox = new TextBox(x, y + _topPanel.Height, width - 5, height - _topPanel.Height - Window.TopBar.Height - 7, "");
            _fileContentBox.Multiline = true;
            _fileContentBox.Text = File.ReadAllText(filePath);

            _dialog = new("Save", "Your file has been saved!");
            _dialog.AddButton("OK", new Action(() =>
            {
                _showDialog = false;
            }));
        }

        private void SaveFile()
        {
            File.WriteAllText(_filePath, _fileContentBox.Text);
            _showDialog = true;
        }

        public override void Update()
        {
            base.Update();
            _save.Update();
            _fileContentBox.Update();

            if (_showDialog)
            {
                _dialog.Update();
            }
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();
            _fileContentBox.HandleLeftClick();
            _save.HandleLeftClick();

            List<Button> buttons = _dialog.GetButtons();
            if (buttons[0].IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y)) {
                buttons[0].Click();
            }
        }

        public override void Draw()
        {
            base.Draw();

            _topPanel.X = X + 1;
            _topPanel.Y = Y + 1;
            _save.X = X + 3;
            _save.Y = Y + 3;
            _fileContentBox.X = X + 1;
            _fileContentBox.Y = Y + _topPanel.Height + 3;

            _fileContentBox.Draw();
            _topPanel.Draw();
            _save.Draw();

            if (_showDialog)
            {
                _dialog.Draw();
            }
        }
    }
}
