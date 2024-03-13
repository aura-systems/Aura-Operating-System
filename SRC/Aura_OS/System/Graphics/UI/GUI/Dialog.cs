/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dialog class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public enum DialogState
    {
        Information,
        Error
    }

    public class Dialog : Window
    {
        public string Message;
        public string Title;

        private List<Button> _buttons;
        private Bitmap _alertIcon;
        private DialogState _state;

        public Dialog(string title, string message, int x, int y) : base(title, x, y, 302, 119, false, false)
        {
            Title = title;
            Message = message;
            _buttons = new List<Button>();
            _alertIcon = Kernel.ResourceManager.GetIcon("32-dialog-information.bmp");
        }

        public override void Update()
        {
            base.Update();

            foreach (var button in _buttons)
            {
                button.Update();

                if (button.IsDirty())
                {
                    MarkDirty();
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            int messageY =  TopBar.Height + 25;
            int messageX = 10 + (int)_alertIcon.Width + 10;
            int messageWidth = Width - messageX;

            List<string> lines = SplitMessageIntoLines(Message, messageWidth);

            foreach (string line in lines)
            {
                DrawString(line, Kernel.font, Kernel.BlackColor, messageX, messageY);
                messageY += Kernel.font.Height;
            }

            if (_alertIcon != null)
            {
                DrawImage(_alertIcon, 0 + 10, 0 + TopBar.Height + 25 - 8);
            }

            foreach (var button in _buttons)
            {
                button.Draw(this);
            }
        }

        public void AddButton(string buttonText, Action onClickAction)
        {
            int buttonY = 80;
            int buttonX = 10;

            foreach (var button in _buttons)
            {
                buttonX += 110;
            }

            Button newButton = new Button(buttonText, buttonX, buttonY, 78, 23);
            newButton.Click = onClickAction;
            _buttons.Add(newButton);
            AddChild(newButton);
        }

        public void SetState(DialogState state)
        {
            _state = state;
            UpdateIcon();
            MarkDirty();
        }

        private List<string> SplitMessageIntoLines(string message, int maxWidth)
        {
            List<string> lines = new List<string>();
            string[] words = message.Split(' ');
            string currentLine = "";
            int maxCharsPerLine = maxWidth / Kernel.font.Width;

            foreach (string word in words)
            {
                if ((currentLine.Length + word.Length) > maxCharsPerLine)
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                {
                    currentLine += " ";
                }

                currentLine += word;

                if (word.Length > maxCharsPerLine)
                {
                    while (currentLine.Length > maxCharsPerLine)
                    {
                        string part = currentLine.Substring(0, maxCharsPerLine);
                        lines.Add(part);
                        currentLine = currentLine.Substring(maxCharsPerLine).Trim();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        private void UpdateIcon()
        {
            switch (_state)
            {
                case DialogState.Information:
                    _alertIcon = Kernel.ResourceManager.GetIcon("32-dialog-information.bmp");
                    break;
                case DialogState.Error:
                    _alertIcon = Kernel.ResourceManager.GetIcon("32-dialog-error.bmp");
                    break;
                default:
                    throw new NotImplementedException($"Unsupported dialog state: {_state}");
            }
        }
    }
}
