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
        private const int _cursorBlinkInterval = 200;

        private int _cursorPosition = 0;
        private int _scrollOffset = 0;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            SetNormalFrame(Kernel.ThemeManager.GetFrame("input.normal"));
            SetHighlightedFrame(Kernel.ThemeManager.GetFrame("input.highlighted"));
            Text = text;
        }

        public override void HandleLeftClick()
        {
            if (IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                TextBox focusedComponent = Kernel.MouseManager.FocusedComponent as TextBox;

                if (Kernel.MouseManager.FocusedComponent is TextBox)
                {
                    focusedComponent._isSelected = false;
                    focusedComponent._cursorVisible = false;
                    focusedComponent.MarkDirty();
                }

                _isSelected = true;
                _cursorPosition = Text.Length;
                AdjustScrollOffsetToEnd();
                Kernel.MouseManager.FocusedComponent = this;
            }
            else
            {
                _isSelected = false;
            }
        }

        public override void Update()
        {
            base.Update();

            if (_isSelected)
            {
                KeyEvent keyEvent = null;

                while (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Backspace:
                            if (_cursorPosition > 0 && Text.Length > 0)
                            {
                                Text = Text.Remove(_cursorPosition - 1, 1);
                                _cursorPosition--;
                                AdjustScrollOffset();
                                MarkDirty();
                            }
                            break;
                        case ConsoleKeyEx.Enter:
                            if (Multiline)
                            {
                                Text += "\n";
                                MarkDirty();
                            }
                            else
                            {
                                if (Enter != null)
                                {
                                    Enter();
                                    MarkDirty();
                                }
                            }
                            break;
                        case ConsoleKeyEx.LeftArrow:
                            if (_cursorPosition > 0)
                            {
                                _cursorPosition--;
                                AdjustScrollOffset();
                                _cursorVisible = true;

                                MarkDirty();
                            }
                            break;
                        case ConsoleKeyEx.RightArrow:
                            if (_cursorPosition < Text.Length)
                            {
                                _cursorPosition++;
                                AdjustScrollOffset();
                                _cursorVisible = true;

                                MarkDirty();
                            }
                            break;
                        default:
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
                            {
                                Text = Text.Insert(_cursorPosition, keyEvent.KeyChar.ToString());
                                _cursorPosition++;
                                AdjustScrollOffset();
                                MarkDirty();
                            }
                            break;
                    }
                }

                if ((DateTime.Now - _lastCursorBlink).TotalMilliseconds > _cursorBlinkInterval)
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

            if (Multiline)
            {
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
            else
            {
                string visibleText = Text.Length > _scrollOffset ? Text.Substring(_scrollOffset) : "";
                int maxVisibleLength = Width / Kernel.font.Width;
                if (visibleText.Length > maxVisibleLength)
                {
                    visibleText = visibleText.Substring(0, maxVisibleLength);
                }

                DrawString(visibleText, Kernel.font, Kernel.BlackColor, 0 + 4, 0 + 4);

                if (_isSelected && _cursorVisible)
                {
                    int cursorX = ((_cursorPosition - _scrollOffset) * Kernel.font.Width) + 4;
                    DrawFilledRectangle(Kernel.BlackColor, cursorX, 0 + 4, 2, Kernel.font.Height);
                }
            }
        }

        private void AdjustScrollOffset()
        {
            int maxVisibleChars = Width / Kernel.font.Width - 1;
            if (_cursorPosition < _scrollOffset)
            {
                _scrollOffset = _cursorPosition;
            }
            else if (_cursorPosition > _scrollOffset + maxVisibleChars)
            {
                _scrollOffset = _cursorPosition - maxVisibleChars;
            }
        }

        private void AdjustScrollOffsetToEnd()
        {
            _scrollOffset = Math.Max(0, Text.Length - (Width / Kernel.font.Width) + 1);
        }
    }
}