/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class TextBox : Component
    {
        public string Text;
        public Action Enter;

        private bool _isSelected = false;
        private bool _cursorVisible = true;
        private DateTime _lastCursorBlink = DateTime.Now;
        private const int _cursorBlinkInterval = 400;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            Text = text;
        }

        public override void HandleLeftClick()
        {
            if (IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                _isSelected = true;
                Kernel.MouseManager.FocusedComponent = this;
            }
            else
            {
                _isSelected = false;
            }
        }

        public override void Update()
        {
            if (_isSelected)
            {
                KeyEvent keyEvent = null;

                while (KeyboardManager.TryReadKey(out keyEvent))
                {
                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Backspace:
                            if (Text.Length > 0)
                            {
                                Text = Text.Remove(Text.Length - 1);
                            }
                            break;
                        case ConsoleKeyEx.Enter:
                            Enter();
                            break;
                        default:
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
                            {
                                Text += keyEvent.KeyChar.ToString();
                            }
                            break;
                    }
                }
            }
        }

        public override void Draw()
        {
            Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, X + 2, Y + 2, Width - 3, Height - 3);

            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);

            Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4, Y + (Height / 2 - Kernel.font.Height / 2));

            if ((DateTime.Now - _lastCursorBlink).TotalMilliseconds > _cursorBlinkInterval)
            {
                _cursorVisible = !_cursorVisible;
                _lastCursorBlink = DateTime.Now;
            }

            if (_isSelected && _cursorVisible)
            {
                int textWidth = Text.Length * Kernel.font.Width;

                int cursorX = X + 4 + textWidth;
                int cursorY = Y + 4;
                int cursorWidth = 2;
                int cursorHeight = Height - 8;

                Kernel.canvas.DrawFilledRectangle(Kernel.BlackColor, cursorX, cursorY, cursorWidth, cursorHeight);
            }
        }
    }
}