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
        public bool Multiline = false;

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
                            if (Multiline)
                            {
                                Text += "\n";
                            }
                            else
                            {
                                Enter();
                            }
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
            // Dessin du cadre de la TextBox
            Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, X + 2, Y + 2, Width - 3, Height - 3);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X + Width, Y);
            Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X, Y + Height);
            Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);

            string[] lines = Text.Split('\n');
            int offsetY = Y + 4;
            int cursorX = X + 4;
            int cursorY = offsetY;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Kernel.canvas.DrawString(line, Kernel.font, Kernel.BlackColor, cursorX, offsetY);

                if (i == lines.Length - 1)
                {
                    cursorY = offsetY;
                    cursorX += line.Length * Kernel.font.Width;
                }

                offsetY += Kernel.font.Height;
            }

            if (_isSelected && (DateTime.Now - _lastCursorBlink).TotalMilliseconds > _cursorBlinkInterval)
            {
                _cursorVisible = !_cursorVisible;
                _lastCursorBlink = DateTime.Now;
            }

            if (_isSelected && _cursorVisible)
            {
                int cursorWidth = 2;
                int cursorHeight = Kernel.font.Height;

                Kernel.canvas.DrawFilledRectangle(Kernel.BlackColor, cursorX, cursorY, cursorWidth, cursorHeight);
            }
        }
    }
}