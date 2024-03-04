﻿/*
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
    public class Dialog : Window
    {
        private string _title;
        private string _message;
        private List<Button> _buttons;
        private Bitmap AlertIcon;

        public Dialog(string title, string message, int x, int y) : base(title, x, y, 302, 119, false, false)
        {
            _title = title;
            _message = message;
            _buttons = new List<Button>();
            AlertIcon = Kernel.ResourceManager.GetIcon("32-dialog-information.bmp");
        }

        public override void Update()
        {
            base.Update();

            foreach (var button in _buttons)
            {
                button.Update();
            }
        }

        public List<Button> GetButtons()
        {
            return _buttons;
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

        public override void Draw()
        {
            base.Draw();

            int messageY = 0 + TopBar.Height + 25;
            DrawString(_message, Kernel.font, Kernel.BlackColor, (int)(0 + 10 + AlertIcon.Width + 10), messageY);

            if (AlertIcon != null)
            {
                DrawImage(AlertIcon, 0 + 10, messageY - 8);
            }

            foreach (var button in _buttons)
            {
                button.Draw(this);
            }
        }
    }
}
