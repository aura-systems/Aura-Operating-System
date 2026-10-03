/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Text box class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI.Skin;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Mouse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Text input, on one line or several (Multiline). On several lines the arrows move the cursor
    /// through the text, a click places it, the view follows it, and scroll bars appear when the
    /// text is wider (at the bottom) or taller (on the right) than the box.
    /// </summary>
    public class TextBox : Component
    {
        public Action Enter;
        public bool Multiline = false;
        public bool Password = false;

        // Space between the box's edges and the text.
        private const int TextPadding = 4;

        // Thickness of the skin's rails.
        private const int ScrollBarSize = 15;

        private const int MinThumbSize = 16;
        private const int _cursorBlinkInterval = 200;

        private string _text = "";

        // _text split at '\n' and its longest line, computed on demand (null and -1 when outdated).
        private string[] _lines;
        private int _longestLine = -1;

        private bool _isSelected = false;
        private bool _cursorVisible = true;
        private DateTime _lastCursorBlink = DateTime.Now;

        // On several lines the cursor is _cursorPosition in line _linePosition, else an index in the text.
        private int _cursorPosition = 0;
        private int _linePosition = 0;

        // The column Up and Down aim for, kept through shorter lines; -1 outside vertical moves.
        private int _preferredColumn = -1;

        // One line: first character shown. Several lines: first column and first line shown.
        private int _scrollOffset = 0;
        private int _scrollX = 0;
        private int _scrollY = 0;

        private enum ScrollDrag
        {
            None,
            Horizontal,
            Vertical
        }

        private Frame _horizontalRail;
        private Frame _horizontalThumb;
        private Frame _horizontalThumbHighlighted;
        private Frame _horizontalThumbPressed;
        private Frame _verticalRail;
        private Frame _verticalThumb;
        private Frame _verticalThumbHighlighted;
        private Frame _verticalThumbPressed;

        // The thumb frames last drawn, to redraw when the mouse highlights or leaves a thumb.
        private Frame _drawnHorizontalThumb;
        private Frame _drawnVerticalThumb;

        private ScrollDrag _dragging = ScrollDrag.None;

        // Mouse minus the thumb's start, along the bar being dragged.
        private int _scrollGrab;

        public TextBox(int x, int y, int width, int height, string text = "") : base(x, y, width, height)
        {
            SetNormalFrame(Kernel.ThemeManager.GetFrame("input.normal"));
            SetHighlightedFrame(Kernel.ThemeManager.GetFrame("input.highlighted"));

            _horizontalRail = Kernel.ThemeManager.GetFrame("rail.horizontal");
            _horizontalThumb = Kernel.ThemeManager.GetFrame("slider.horizontal.normal");
            _horizontalThumbHighlighted = Kernel.ThemeManager.GetFrame("slider.horizontal.highlighted");
            _horizontalThumbPressed = Kernel.ThemeManager.GetFrame("slider.horizontal.depressed");
            _verticalRail = Kernel.ThemeManager.GetFrame("rail.vertical");
            _verticalThumb = Kernel.ThemeManager.GetFrame("slider.vertical.normal");
            _verticalThumbHighlighted = Kernel.ThemeManager.GetFrame("slider.vertical.highlighted");
            _verticalThumbPressed = Kernel.ThemeManager.GetFrame("slider.vertical.depressed");

            Text = text;
        }

        /// <summary>
        /// The text, lines separated by '\n' (never null).
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? "";
                _lines = null;
                _longestLine = -1;

                // A shorter text set while the box has the keys (an app's code on Enter): the next key
                // types at its end rather than past it.
                if (!Multiline && _cursorPosition > _text.Length)
                {
                    _cursorPosition = _text.Length;
                }
            }
        }

        #region Geometry

        private string[] Lines
        {
            get
            {
                if (_lines == null)
                {
                    _lines = _text.Split('\n');
                }

                return _lines;
            }
        }

        private int LongestLine
        {
            get
            {
                if (_longestLine < 0)
                {
                    _longestLine = 0;

                    foreach (string line in Lines)
                    {
                        _longestLine = Math.Max(_longestLine, line.Length);
                    }
                }

                return _longestLine;
            }
        }

        // The cursor after a line's last character takes a column too.
        private int ContentColumns => LongestLine + 1;

        private int ColumnsIn(int width) => Math.Max(1, (width - 2 * TextPadding) / Kernel.font.Width);

        private int LinesIn(int height) => Math.Max(1, (height - TextPadding) / Kernel.font.Height);

        /// <summary>
        /// The scroll bars the text needs. Each takes room from the other direction: a vertical bar
        /// can make the lines too wide for the box, and a horizontal one the text too tall.
        /// </summary>
        private void GetScrollBars(out bool horizontal, out bool vertical)
        {
            horizontal = false;
            vertical = false;

            if (!Multiline)
            {
                return;
            }

            horizontal = ContentColumns > ColumnsIn(Width);
            vertical = Lines.Length > LinesIn(horizontal ? Height - 1 - ScrollBarSize : Height);

            if (vertical && !horizontal)
            {
                horizontal = ContentColumns > ColumnsIn(Width - 1 - ScrollBarSize);
            }
        }

        private bool HasHorizontalScrollBar
        {
            get
            {
                bool horizontal, vertical;
                GetScrollBars(out horizontal, out vertical);
                return horizontal;
            }
        }

        private bool HasVerticalScrollBar
        {
            get
            {
                bool horizontal, vertical;
                GetScrollBars(out horizontal, out vertical);
                return vertical;
            }
        }

        // The box less its scroll bars, which sit inside the input frame's 1-pixel border.
        private int TextAreaWidth => HasVerticalScrollBar ? Width - 1 - ScrollBarSize : Width;
        private int TextAreaHeight => HasHorizontalScrollBar ? Height - 1 - ScrollBarSize : Height;

        private int VisibleColumns => ColumnsIn(TextAreaWidth);
        private int VisibleLines => LinesIn(TextAreaHeight);

        private int MaxScrollX => Math.Max(0, ContentColumns - VisibleColumns);
        private int MaxScrollY => Math.Max(0, Lines.Length - VisibleLines);

        // The horizontal bar under the text and the vertical bar on its right; with both, the corner
        // between them stays empty.
        private int HorizontalBarX => 1;
        private int HorizontalBarY => Height - 1 - ScrollBarSize;
        private int HorizontalBarWidth => Math.Max(0, (HasVerticalScrollBar ? Width - 1 - ScrollBarSize : Width - 1) - HorizontalBarX);

        private int VerticalBarX => Width - 1 - ScrollBarSize;
        private int VerticalBarY => 1;
        private int VerticalBarHeight => Math.Max(0, (HasHorizontalScrollBar ? Height - 1 - ScrollBarSize : Height - 1) - VerticalBarY);

        // A thumb is as long, relative to its bar, as the part of the text in view.
        private int HorizontalThumbWidth => Math.Min(HorizontalBarWidth, Math.Max(MinThumbSize, HorizontalBarWidth * VisibleColumns / ContentColumns));
        private int HorizontalThumbX => HorizontalBarX + (MaxScrollX > 0 ? (HorizontalBarWidth - HorizontalThumbWidth) * _scrollX / MaxScrollX : 0);

        private int VerticalThumbHeight => Math.Min(VerticalBarHeight, Math.Max(MinThumbSize, VerticalBarHeight * VisibleLines / Lines.Length));
        private int VerticalThumbY => VerticalBarY + (MaxScrollY > 0 ? (VerticalBarHeight - VerticalThumbHeight) * _scrollY / MaxScrollY : 0);

        #endregion

        public override void HandleLeftClick()
        {
            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;

            if (!IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                _isSelected = false;
                return;
            }

            TextBox focusedComponent = Kernel.MouseManager.FocusedComponent as TextBox;

            if (focusedComponent != null)
            {
                focusedComponent._isSelected = false;
                focusedComponent._cursorVisible = false;
                focusedComponent.MarkDirty();
            }

            _isSelected = true;
            Kernel.MouseManager.FocusedComponent = this;
            _preferredColumn = -1;

            bool horizontal, vertical;
            GetScrollBars(out horizontal, out vertical);

            // On a rail: drag its thumb, brought under the mouse when the press is beside it.
            if (vertical && x >= VerticalBarX && y < VerticalBarY + VerticalBarHeight)
            {
                bool onThumb = y >= VerticalThumbY && y < VerticalThumbY + VerticalThumbHeight;
                _scrollGrab = onThumb ? y - VerticalThumbY : VerticalThumbHeight / 2;
                _dragging = ScrollDrag.Vertical;
                ScrollToThumbY(y - _scrollGrab);
                MarkDirty();
            }
            else if (horizontal && y >= HorizontalBarY && x < HorizontalBarX + HorizontalBarWidth)
            {
                bool onThumb = x >= HorizontalThumbX && x < HorizontalThumbX + HorizontalThumbWidth;
                _scrollGrab = onThumb ? x - HorizontalThumbX : HorizontalThumbWidth / 2;
                _dragging = ScrollDrag.Horizontal;
                ScrollToThumbX(x - _scrollGrab);
                MarkDirty();
            }
            else if ((vertical && x >= VerticalBarX) || (horizontal && y >= HorizontalBarY))
            {
                // The corner between the bars.
            }
            else if (Multiline)
            {
                // The cursor goes to the character boundary nearest the click.
                string[] lines = Lines;
                _linePosition = Math.Max(0, Math.Min(_scrollY + (y - TextPadding) / Kernel.font.Height, lines.Length - 1));

                int column = _scrollX + (x - TextPadding + Kernel.font.Width / 2) / Kernel.font.Width;
                _cursorPosition = Math.Max(0, Math.Min(column, lines[_linePosition].Length));
                _cursorVisible = true;
                ScrollToCursor();
                MarkDirty();
            }
            else
            {
                _cursorPosition = _text.Length;
                AdjustScrollOffsetToEnd();
            }
        }

        public void Update(KeyEvent keyEvent)
        {
            base.Update();
            UpdateScrollBar();

            if (_isSelected)
            {
                if (keyEvent != null)
                {
                    HandleKey(keyEvent);
                }

                BlinkCursor();
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateScrollBar();

            if (_isSelected)
            {
                KeyEvent keyEvent = null;

                while (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
                    HandleKey(keyEvent);
                }

                BlinkCursor();
            }
        }

        public void UpdateNoGetKey()
        {
            base.Update();
            UpdateScrollBar();

            if (_isSelected)
            {
                BlinkCursor();
            }
        }

        private void BlinkCursor()
        {
            if ((DateTime.Now - _lastCursorBlink).TotalMilliseconds > _cursorBlinkInterval)
            {
                _cursorVisible = !_cursorVisible;
                _lastCursorBlink = DateTime.Now;

                MarkDirty();
            }
        }

        /// <summary>
        /// Drags the thumb while the button its rail was pressed with is down, and highlights the
        /// thumb under the mouse.
        /// </summary>
        private void UpdateScrollBar()
        {
            bool horizontal, vertical;
            GetScrollBars(out horizontal, out vertical);

            if (!Kernel.MouseManager.IsLeftButtonDown
                || (_dragging == ScrollDrag.Horizontal && !horizontal)
                || (_dragging == ScrollDrag.Vertical && !vertical))
            {
                _dragging = ScrollDrag.None;
            }

            if (_dragging == ScrollDrag.Horizontal)
            {
                ScrollToThumbX((int)MouseManager.X - AbsoluteX - _scrollGrab);
            }
            else if (_dragging == ScrollDrag.Vertical)
            {
                ScrollToThumbY((int)MouseManager.Y - AbsoluteY - _scrollGrab);
            }

            if ((horizontal && GetHorizontalThumbFrame() != _drawnHorizontalThumb)
                || (vertical && GetVerticalThumbFrame() != _drawnVerticalThumb))
            {
                MarkDirty();
            }
        }

        /// <summary>
        /// Scrolls so the horizontal thumb starts at that x (box coordinates).
        /// </summary>
        private void ScrollToThumbX(int thumbX)
        {
            int range = HorizontalBarWidth - HorizontalThumbWidth;
            int scrollX = range > 0 ? ((thumbX - HorizontalBarX) * MaxScrollX + range / 2) / range : 0;
            scrollX = Math.Max(0, Math.Min(scrollX, MaxScrollX));

            if (scrollX != _scrollX)
            {
                _scrollX = scrollX;
                MarkDirty();
            }
        }

        /// <summary>
        /// Scrolls so the vertical thumb starts at that y (box coordinates).
        /// </summary>
        private void ScrollToThumbY(int thumbY)
        {
            int range = VerticalBarHeight - VerticalThumbHeight;
            int scrollY = range > 0 ? ((thumbY - VerticalBarY) * MaxScrollY + range / 2) / range : 0;
            scrollY = Math.Max(0, Math.Min(scrollY, MaxScrollY));

            if (scrollY != _scrollY)
            {
                _scrollY = scrollY;
                MarkDirty();
            }
        }

        private Frame GetHorizontalThumbFrame()
        {
            if (_dragging == ScrollDrag.Horizontal)
            {
                return _horizontalThumbPressed;
            }

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;
            bool over = x >= HorizontalThumbX && x < HorizontalThumbX + HorizontalThumbWidth && y >= HorizontalBarY && y < HorizontalBarY + ScrollBarSize;

            return over ? _horizontalThumbHighlighted : _horizontalThumb;
        }

        private Frame GetVerticalThumbFrame()
        {
            if (_dragging == ScrollDrag.Vertical)
            {
                return _verticalThumbPressed;
            }

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;
            bool over = x >= VerticalBarX && x < VerticalBarX + ScrollBarSize && y >= VerticalThumbY && y < VerticalThumbY + VerticalThumbHeight;

            return over ? _verticalThumbHighlighted : _verticalThumb;
        }

        private void HandleKey(KeyEvent keyEvent)
        {
            // The app may have changed Text under the cursor.
            ClampCursor();

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
                case ConsoleKeyEx.UpArrow:
                    HandleUpArrow();
                    break;
                case ConsoleKeyEx.DownArrow:
                    HandleDownArrow();
                    break;
                default:
                    HandleDefaultKey(keyEvent);
                    break;
            }

            if (keyEvent.Key != ConsoleKeyEx.UpArrow && keyEvent.Key != ConsoleKeyEx.DownArrow)
            {
                _preferredColumn = -1;
            }

            ScrollToCursor();
        }

        private void ClampCursor()
        {
            if (Multiline)
            {
                _linePosition = Math.Max(0, Math.Min(_linePosition, Lines.Length - 1));
                _cursorPosition = Math.Max(0, Math.Min(_cursorPosition, Lines[_linePosition].Length));
            }
            else
            {
                _cursorPosition = Math.Max(0, Math.Min(_cursorPosition, _text.Length));
            }
        }

        private void HandleLeftArrow()
        {
            if (_cursorPosition > 0)
            {
                _cursorPosition--;
                _cursorVisible = true;

                MarkDirty();
            }
            else if (_linePosition > 0)
            {
                // Move to the end of the previous line
                _linePosition--;
                _cursorPosition = Lines[_linePosition].Length;
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
                    _cursorVisible = true;
                    MarkDirty();
                }
                else if (_linePosition < lines.Length - 1)
                {
                    // Move to the beginning of the next line if not on the last line
                    _linePosition++;
                    _cursorPosition = 0; // Reset cursor position for the new line
                    _cursorVisible = true;
                    MarkDirty();
                }
            }
        }

        private void HandleUpArrow()
        {
            if (Multiline && _linePosition > 0)
            {
                MoveToLine(_linePosition - 1);
            }
        }

        private void HandleDownArrow()
        {
            if (Multiline && _linePosition < Lines.Length - 1)
            {
                MoveToLine(_linePosition + 1);
            }
        }

        /// <summary>
        /// Moves the cursor to that line, at the column the vertical moves started from, or at the
        /// line's end when the line is shorter.
        /// </summary>
        private void MoveToLine(int line)
        {
            if (_preferredColumn < 0)
            {
                _preferredColumn = _cursorPosition;
            }

            _linePosition = line;
            _cursorPosition = Math.Min(_preferredColumn, Lines[line].Length);
            _cursorVisible = true;

            MarkDirty();
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
                MarkDirty();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (Multiline)
            {
                DrawLines();
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

        /// <summary>
        /// The lines in view (whole lines only), the cursor, and the scroll bars the text needs.
        /// </summary>
        private void DrawLines()
        {
            string[] lines = Lines;
            int columns = VisibleColumns;
            int textHeight = TextAreaHeight;

            // The text may have changed under the view.
            _scrollX = Math.Max(0, Math.Min(_scrollX, MaxScrollX));
            _scrollY = Math.Max(0, Math.Min(_scrollY, MaxScrollY));

            for (int row = 0; _scrollY + row < lines.Length; row++)
            {
                int y = TextPadding + row * Kernel.font.Height;

                if (y + Kernel.font.Height > textHeight)
                {
                    break;
                }

                int index = _scrollY + row;
                string line = lines[index];

                if (line.Length > _scrollX)
                {
                    DrawString(line.Substring(_scrollX, Math.Min(columns, line.Length - _scrollX)), Kernel.font, Kernel.BlackColor, TextPadding, y);
                }

                if (_isSelected && _cursorVisible && index == _linePosition)
                {
                    int column = _cursorPosition - _scrollX;

                    if (column >= 0 && column < columns)
                    {
                        DrawFilledRectangle(Kernel.BlackColor, TextPadding + column * Kernel.font.Width, y, 2, Kernel.font.Height);
                    }
                }
            }

            bool horizontal, vertical;
            GetScrollBars(out horizontal, out vertical);

            if (horizontal)
            {
                _drawnHorizontalThumb = GetHorizontalThumbFrame();

                DrawFrame(_horizontalRail, HorizontalBarX, HorizontalBarY, HorizontalBarWidth, ScrollBarSize);
                DrawFrame(_drawnHorizontalThumb, HorizontalThumbX, HorizontalBarY, HorizontalThumbWidth, ScrollBarSize);
            }

            if (vertical)
            {
                _drawnVerticalThumb = GetVerticalThumbFrame();

                DrawFrame(_verticalRail, VerticalBarX, VerticalBarY, ScrollBarSize, VerticalBarHeight);
                DrawFrame(_drawnVerticalThumb, VerticalBarX, VerticalThumbY, ScrollBarSize, VerticalThumbHeight);
            }
        }

        /// <summary>
        /// Scrolls so the cursor is in view.
        /// </summary>
        private void ScrollToCursor()
        {
            if (!Multiline)
            {
                AdjustScrollOffset();
                return;
            }

            int columns = VisibleColumns;

            if (_cursorPosition < _scrollX)
            {
                _scrollX = _cursorPosition;
            }
            else if (_cursorPosition >= _scrollX + columns)
            {
                _scrollX = _cursorPosition - columns + 1;
            }

            int lines = VisibleLines;

            if (_linePosition < _scrollY)
            {
                _scrollY = _linePosition;
            }
            else if (_linePosition >= _scrollY + lines)
            {
                _scrollY = _linePosition - lines + 1;
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

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            _cursorVisible = selected;
            MarkDirty();
        }

        /// <summary>
        /// Gives the text box the keys, as a click on it does; a single line one puts its cursor at the
        /// end of its text.
        /// </summary>
        public void Focus()
        {
            TextBox focused = Kernel.MouseManager.FocusedComponent as TextBox;

            if (focused != null && focused != this)
            {
                focused.SetSelected(false);
            }

            SetSelected(true);
            Kernel.MouseManager.FocusedComponent = this;
            _preferredColumn = -1;

            if (!Multiline)
            {
                _cursorPosition = _text.Length;
                AdjustScrollOffsetToEnd();
            }
        }
    }
}
