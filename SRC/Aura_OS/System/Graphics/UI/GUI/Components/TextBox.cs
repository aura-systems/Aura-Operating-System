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
        private const int _cursorBlinkInterval = 1;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            Frame = Kernel.ThemeManager.GetFrame("input.normal");
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
                    MarkDirty();

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

                    MarkDirty();
                }

                if ((DateTime.Now - _lastCursorBlink).Seconds > _cursorBlinkInterval)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastCursorBlink = DateTime.Now;

                    MarkDirty();
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            string[] lines = Text.Split('\n');
            int offsetY = 0 + 4;
            int cursorX = 0 + 4;
            int cursorY = offsetY;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                DrawString(line, Kernel.font, Kernel.BlackColor, cursorX, offsetY);

                if (i == lines.Length - 1)
                {
                    cursorY = offsetY;
                    cursorX += line.Length * Kernel.font.Width;
                }

                offsetY += Kernel.font.Height;
            }

            if (_isSelected && _cursorVisible)
            {
                int cursorWidth = 2;
                int cursorHeight = Kernel.font.Height;

                DrawFilledRectangle(Kernel.BlackColor, cursorX, cursorY, cursorWidth, cursorHeight);
            }
        }
    }
}