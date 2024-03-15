/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class TextBox : Component
    {
        public string Text;
        public Action Enter;
        public bool Multiline = false;
        public bool Password = false;

        private bool _isSelected = false;
        private bool _cursorVisible = true;
        private DateTime _lastCursorBlink = DateTime.Now;
        private const int _cursorBlinkInterval = 200;

        private int _cursorPosition = 0;
        private int _linePosition = 0;
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
                if (Multiline)
                {
                    _cursorPosition = Text.Length - Text.LastIndexOf('\n') - 1;
                    _linePosition = Text.Split('\n').Length - 1;
                }
                else
                {
                    _cursorPosition = Text.Length;
                }
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
                            HandleBackspace();
                            break;
                        case ConsoleKeyEx.Enter:
                            HandleEnter();
                            break;
                        case ConsoleKeyEx.LeftArrow:
                            HandleLeftArrow();
                            break;
                        case ConsoleKeyEx.RightArrow:
                            HandleRightArrow();
                            break;
                        default:
                            HandleDefaultKey(keyEvent);
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

        private void HandleLeftArrow()
        {
            if (_cursorPosition > 0)
            {
                _cursorPosition--;
                AdjustScrollOffset();
                _cursorVisible = true;

                MarkDirty();
            }
            else if (_linePosition > 0)
            {
                // Move to the end of the previous line
                _linePosition--;
                _cursorPosition = Text.Split('\n')[_linePosition].Length;
                AdjustScrollOffset();
                _cursorVisible = true;
                MarkDirty();
            }
        }

        private void HandleRightArrow()
        {
            var lines = Text.Split('\n');

            if (_linePosition < lines.Length)
            {
                string currentLine = lines[_linePosition];
                if (_cursorPosition < currentLine.Length)
                {
                    _cursorPosition++;
                    AdjustScrollOffset();
                    _cursorVisible = true;
                    MarkDirty();
                }
                else if (_linePosition < lines.Length - 1)
                {
                    // Move to the beginning of the next line if not on the last line
                    _linePosition++;
                    _cursorPosition = 0; // Reset cursor position for the new line
                    AdjustScrollOffset();
                    _cursorVisible = true;
                    MarkDirty();
                }
            }
        }


        private void HandleBackspace()
        {
            if (_cursorPosition > 0 || _linePosition > 0)
            {
                var lines = Text.Split('\n');

                if (_cursorPosition == 0 && _linePosition > 0)
                {
                    // Concatenate the current line to the end of the previous line, then remove the current line
                    string prevLine = lines[_linePosition - 1];
                    string currentLine = lines[_linePosition];
                    lines[_linePosition - 1] = prevLine + currentLine;
                    List<string> linesList = lines.ToList();
                    linesList.RemoveAt(_linePosition);
                    Text = string.Join("\n", linesList.ToArray());

                    _linePosition--;
                    _cursorPosition = prevLine.Length; // Move the cursor to the end of the previous line
                }
                else
                {
                    // Normal backspace operation within the same line
                    string currentLine = lines[_linePosition];
                    string newLine = currentLine.Remove(_cursorPosition - 1, 1);
                    lines[_linePosition] = newLine;
                    Text = string.Join("\n", lines);

                    _cursorPosition--;
                }

                MarkDirty();
            }
        }


        private void HandleEnter()
        {
            if (Multiline)
            {
                // Insert a new line at the current cursor position within the text
                var lines = Text.Split('\n');
                if (_linePosition < lines.Length)
                {
                    // Inserting within existing lines
                    lines[_linePosition] = lines[_linePosition].Insert(_cursorPosition, "\n");
                    Text = string.Join("\n", lines);
                }
                else
                {
                    // Appending a new line at the end
                    Text += "\n";
                }

                _linePosition++;
                _cursorPosition = 0; // Reset cursor position for the new line
                MarkDirty();
            }
            else
            {
                Enter?.Invoke();
                MarkDirty();
            }
        }

        private void HandleDefaultKey(KeyEvent keyEvent)
        {
            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
            {
                if (Multiline)
                {
                    // Find the correct line and position to insert the character
                    var lines = Text.Split('\n');
                    if (_linePosition < lines.Length)
                    {
                        lines[_linePosition] = lines[_linePosition].Insert(_cursorPosition, keyEvent.KeyChar.ToString());
                        Text = string.Join("\n", lines);
                    }
                    else
                    {
                        // If for some reason the line position is out of bounds, append the character
                        Text += keyEvent.KeyChar;
                    }
                }
                else
                {
                    Text = Text.Insert(_cursorPosition, keyEvent.KeyChar.ToString());
                }
                _cursorPosition++;
                AdjustScrollOffset();
                MarkDirty();
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
                int cursorY = offsetY + _linePosition * Kernel.font.Height;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    DrawString(line, Kernel.font, Kernel.BlackColor, cursorX, offsetY + (i * Kernel.font.Height));

                    if (_isSelected && _cursorVisible && i == _linePosition)
                    {
                        DrawFilledRectangle(Kernel.BlackColor, cursorX + (_cursorPosition * Kernel.font.Width), cursorY, 2, Kernel.font.Height);
                    }
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

                if (Password)
                {
                    int px = 0 + 6;
                    for (int i = 0; i < visibleText.Length; i++)
                    {
                        DrawFilledCircle(Kernel.BlackColor, px, Height / 2 - 3/2, 3);
                        px += 6 + 2;
                    }
                }
                else
                {
                    DrawString(visibleText, Kernel.font, Kernel.BlackColor, 0 + 4, 0 + 4);

                    if (_isSelected && _cursorVisible)
                    {
                        int cursorX = ((_cursorPosition - _scrollOffset) * Kernel.font.Width) + 4;
                        DrawFilledRectangle(Kernel.BlackColor, cursorX, 0 + 4, 2, Kernel.font.Height);
                    }
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