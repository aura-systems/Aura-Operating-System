/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dialog class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Cosmos.System;
using Cosmos.System.Graphics;

namespace Aura_OS.System.Graphics.UI.GUI
{
    internal class Dialog : Window
    {
        private string _title;
        private string _message;
        private List<Button> _buttons;
        private Bitmap AlertIcon;

        private int _px;
        private int _py;
        private bool _lck = false;
        private bool _pressed;
        private bool _hasWindowMoving = false;

        public Dialog(string title, string message, int x, int y) : base(title, x, y, 302, 119, true, false)
        {
            _title = title;
            _message = message;
            _buttons = new List<Button>();
            AlertIcon = Kernel.ResourceManager.GetIcon("32-dialog-information.bmp");
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

        public override void Update()
        {
            base.Update();

            if (Kernel.MouseManager.IsLeftButtonDown)
            {
                if (!_hasWindowMoving && Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    Close.Click();

                    return;
                }
                else if (!_hasWindowMoving && TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    _hasWindowMoving = true;

                    _pressed = true;
                    if (!_lck)
                    {
                        _px = (int)MouseManager.X - X;
                        _py = (int)MouseManager.Y - Y;
                        _lck = true;
                    }
                }
            }
            else
            {
                _pressed = false;
                _lck = false;
                _hasWindowMoving = false;
            }

            if (_pressed)
            {
                X = (int)(MouseManager.X - _px);
                Y = (int)(MouseManager.Y - _py);

                X = (int)(MouseManager.X - _px + 3);
                Y = (int)(MouseManager.Y - _py + TopBar.Height + 3);
            }
        }

        public override void Draw()
        {
            base.Draw();

            int messageY = 0 + TopBar.Height + 25;
            DrawString(_message, Kernel.font, Kernel.BlackColor, (int)(0 + 10 + AlertIcon.Width + 10), messageY);

            if (AlertIcon != null)
            {
                DrawImageAlpha(AlertIcon, 0 + 10, messageY - 8);
            }

            foreach (var button in _buttons)
            {
                button.Draw(this);
            }
        }
    }
}
