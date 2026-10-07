/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Text box class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI.Skin;
using Cosmos.Kernel.System.Input;
using System;
using System.Text;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Text input, on one line or several (Multiline). The arrows move the cursor through the text, a
    /// click places it, the view follows it, and on several lines scroll bars appear when the text is
    /// wider (at the bottom) or taller (on the right) than the box. Dragging the mouse, Shift with a
    /// move, a double click (a word) or Ctrl+A selects text: Ctrl+C and Ctrl+X copy it to the
    /// TextClipboard (not from a password), Ctrl+V types the text copied in its place. A right click
    /// opens the same as a menu.
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

        // The selection runs from the anchor to the cursor (line and column, as the cursor's); nothing
        // is selected when they are the same place.
        private int _anchorLine = 0;
        private int _anchorPosition = 0;

        // The left button pressed on the text is still down: the cursor follows the mouse.
        private bool _selecting = false;

        // Cut, Copy, Paste, Select all; made on the first right click.
        private EditMenu _menu;

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

                // A new text has nothing selected.
                CollapseSelection();
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

            _selecting = false;

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
            else
            {
                // The cursor goes to the character boundary nearest the click, and the selection
                // starts there: from where it started with Shift held.
                ClampCursor();
                PositionAt(x, y, out _linePosition, out _cursorPosition);

                if (!KeyboardManager.ShiftPressed)
                {
                    CollapseSelection();
                }

                _selecting = true;
                _cursorVisible = true;
                ScrollToCursor();
                MarkDirty();
            }
        }

        /// <summary>
        /// The second click of a double click selects the word under the mouse (a password, all of it).
        /// </summary>
        public override void HandleLeftDoubleClick()
        {
            HandleLeftClick();

            // On a scroll bar, or out of the box.
            if (!_selecting)
            {
                return;
            }

            _selecting = false;

            if (Password)
            {
                SelectAll();
                return;
            }

            string line = Multiline ? Lines[_linePosition] : _text;
            int x = (int)MouseManager.X - AbsoluteX;
            int first = Multiline ? _scrollX : _scrollOffset;
            int index = first + FloorDivide(x - TextPadding, Kernel.font.Width);

            if (index < 0 || index >= line.Length)
            {
                return;
            }

            // The characters around it of the same kind: a word, spaces, or one other character.
            int kind = KindOf(line[index]);
            int start = index;
            int end = index + 1;

            if (kind != OtherKind)
            {
                while (start > 0 && KindOf(line[start - 1]) == kind)
                {
                    start--;
                }

                while (end < line.Length && KindOf(line[end]) == kind)
                {
                    end++;
                }
            }

            _anchorLine = _linePosition;
            _anchorPosition = start;
            _cursorPosition = end;
            ScrollToCursor();
            MarkDirty();
        }

        /// <summary>
        /// A right click gives the box the keys, its selection kept, and opens Cut, Copy, Paste and
        /// Select all.
        /// </summary>
        public override void HandleRightClick()
        {
            TextBox focused = Kernel.MouseManager.FocusedComponent as TextBox;

            if (focused != null && focused != this)
            {
                focused.SetSelected(false);
            }

            SetSelected(true);
            Kernel.MouseManager.FocusedComponent = this;

            if (_menu == null)
            {
                _menu = new EditMenu(
                    new string[] { "Cut", "Copy", "Paste", "Select all" },
                    new Action[] { Cut, Copy, Paste, SelectAll },
                    new Func<bool>[] { CanCopy, CanCopy, CanPaste, () => _text.Length > 0 });
                RightClick = _menu;
            }

            _menu.Refresh();
            base.HandleRightClick();
        }

        public override void Dispose()
        {
            if (_menu != null)
            {
                _menu.CloseMenu();
                _menu.Dispose();
                _menu = null;
                RightClick = null;
            }

            base.Dispose();
        }

        private const int WordKind = 0;
        private const int SpaceKind = 1;
        private const int OtherKind = 2;

        private static int KindOf(char c)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                return WordKind;
            }

            return char.IsWhiteSpace(c) ? SpaceKind : OtherKind;
        }

        /// <summary>
        /// The character boundary nearest that point (box coordinates), as a line and a column. Out of
        /// the text in view, it is past its edge, so that dragging there scrolls.
        /// </summary>
        private void PositionAt(int x, int y, out int line, out int column)
        {
            int first = Multiline ? _scrollX : _scrollOffset;
            column = first + FloorDivide(x - TextPadding + Kernel.font.Width / 2, Kernel.font.Width);
            line = 0;

            if (Multiline)
            {
                string[] lines = Lines;
                line = Math.Max(0, Math.Min(_scrollY + FloorDivide(y - TextPadding, Kernel.font.Height), lines.Length - 1));
                column = Math.Max(0, Math.Min(column, lines[line].Length));
            }
            else
            {
                column = Math.Max(0, Math.Min(column, _text.Length));
            }
        }

        // Rounded down, also for a point left of or above the text.
        private static int FloorDivide(int value, int divisor)
        {
            return value >= 0 ? value / divisor : -((divisor - 1 - value) / divisor);
        }

        public void Update(KeyEvent keyEvent)
        {
            base.Update();
            UpdateScrollBar();
            UpdateSelection();

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
            UpdateSelection();

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
            UpdateSelection();

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
        /// While the button pressed on the text is down, the cursor follows the mouse: the text from
        /// where the press was is selected, and the view scrolls when the mouse is out of it.
        /// </summary>
        private void UpdateSelection()
        {
            if (!_selecting)
            {
                return;
            }

            if (!Kernel.MouseManager.IsLeftButtonDown)
            {
                _selecting = false;
                return;
            }

            int line, column;
            PositionAt((int)MouseManager.X - AbsoluteX, (int)MouseManager.Y - AbsoluteY, out line, out column);

            if (line != _linePosition || column != _cursorPosition)
            {
                _linePosition = line;
                _cursorPosition = column;
                _cursorVisible = true;
                ScrollToCursor();
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

            if (HandleShortcut(keyEvent))
            {
                _preferredColumn = -1;
                ScrollToCursor();
                return;
            }

            // A move with Shift held selects the text it goes over.
            bool extend = Input.KeyboardManager.IsShiftHeld(keyEvent);

            switch (keyEvent.Key)
            {
                case Key.Backspace:
                    HandleBackspace();
                    break;
                case Key.Delete:
                    HandleDelete();
                    break;
                case Key.Enter:
                    HandleEnter();
                    break;
                case Key.LeftArrow:
                    HandleLeftArrow(extend);
                    break;
                case Key.RightArrow:
                    HandleRightArrow(extend);
                    break;
                case Key.UpArrow:
                    HandleUpArrow(extend);
                    break;
                case Key.DownArrow:
                    HandleDownArrow(extend);
                    break;
                case Key.Home:
                    MoveCursor(_linePosition, 0, extend);
                    break;
                case Key.End:
                    MoveCursor(_linePosition, Multiline ? Lines[_linePosition].Length : _text.Length, extend);
                    break;
                default:
                    HandleDefaultKey(keyEvent);
                    break;
            }

            if (keyEvent.Key != Key.UpArrow && keyEvent.Key != Key.DownArrow)
            {
                _preferredColumn = -1;
            }

            ScrollToCursor();
        }

        /// <summary>
        /// Ctrl+A selects all the text, Ctrl+C copies the selection, Ctrl+X cuts it, and Ctrl+V types
        /// the copied text in its place. A password is neither copied nor cut.
        /// </summary>
        /// <returns>False for another key.</returns>
        private bool HandleShortcut(KeyEvent keyEvent)
        {
            if (Input.KeyboardManager.IsShortcut(keyEvent, Key.A))
            {
                SelectAll();
            }
            else if (Input.KeyboardManager.IsShortcut(keyEvent, Key.C))
            {
                Copy();
            }
            else if (Input.KeyboardManager.IsShortcut(keyEvent, Key.X))
            {
                Cut();
            }
            else if (Input.KeyboardManager.IsShortcut(keyEvent, Key.V))
            {
                Paste();
            }
            else
            {
                return false;
            }

            return true;
        }

        // A password is neither copied nor cut.
        private bool CanCopy() => HasSelection && !Password;

        private bool CanPaste() => Pastable(TextClipboard.Text).Length > 0;

        private void Copy()
        {
            if (CanCopy())
            {
                TextClipboard.Copy(SelectedText);
            }
        }

        private void Cut()
        {
            if (CanCopy())
            {
                TextClipboard.Copy(SelectedText);
                ReplaceSelection("");
            }
        }

        private void Paste()
        {
            string text = Pastable(TextClipboard.Text);

            if (text.Length > 0)
            {
                ReplaceSelection(text);
            }
        }

        /// <summary>
        /// What a paste types: the characters a key types, a tab as four spaces, and the line breaks,
        /// as spaces on one line.
        /// </summary>
        private string Pastable(string text)
        {
            if (text == null)
            {
                return "";
            }

            StringBuilder pasted = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r' || c == '\n')
                {
                    // "\r\n" is one line break.
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    pasted.Append(Multiline ? '\n' : ' ');
                }
                else if (c == '\t')
                {
                    pasted.Append("    ");
                }
                else if (IsTyped(c))
                {
                    pasted.Append(c);
                }
            }

            return pasted.ToString();
        }

        private static bool IsTyped(char c)
        {
            return char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ';
        }

        /// <summary>
        /// Keeps the cursor and the selection's start in the text, which the app may have changed.
        /// </summary>
        private void ClampCursor()
        {
            ClampPosition(ref _linePosition, ref _cursorPosition);
            ClampPosition(ref _anchorLine, ref _anchorPosition);
        }

        private void ClampPosition(ref int line, ref int column)
        {
            if (Multiline)
            {
                line = Math.Max(0, Math.Min(line, Lines.Length - 1));
                column = Math.Max(0, Math.Min(column, Lines[line].Length));
            }
            else
            {
                line = 0;
                column = Math.Max(0, Math.Min(column, _text.Length));
            }
        }

        #region Selection

        private bool HasSelection => _anchorLine != _linePosition || _anchorPosition != _cursorPosition;

        /// <summary>
        /// The selected text, "" for none.
        /// </summary>
        public string SelectedText
        {
            get
            {
                int start, end;
                GetSelection(out start, out end);
                return _text.Substring(start, end - start);
            }
        }

        /// <summary>
        /// The selection's start and end, lines and columns in the text's order (the same place when
        /// nothing is selected).
        /// </summary>
        private void GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn)
        {
            int cursorLine = _linePosition;
            int cursorColumn = _cursorPosition;
            int anchorLine = _anchorLine;
            int anchorColumn = _anchorPosition;

            ClampPosition(ref cursorLine, ref cursorColumn);
            ClampPosition(ref anchorLine, ref anchorColumn);

            bool anchorFirst = anchorLine < cursorLine || (anchorLine == cursorLine && anchorColumn <= cursorColumn);

            startLine = anchorFirst ? anchorLine : cursorLine;
            startColumn = anchorFirst ? anchorColumn : cursorColumn;
            endLine = anchorFirst ? cursorLine : anchorLine;
            endColumn = anchorFirst ? cursorColumn : anchorColumn;
        }

        /// <summary>
        /// The selection's start and end as indexes in Text.
        /// </summary>
        private void GetSelection(out int start, out int end)
        {
            int startLine, startColumn, endLine, endColumn;
            GetSelection(out startLine, out startColumn, out endLine, out endColumn);

            start = IndexOf(startLine, startColumn);
            end = IndexOf(endLine, endColumn);
        }

        private int CursorIndex => IndexOf(_linePosition, _cursorPosition);

        /// <summary>
        /// The index in Text of a line and column.
        /// </summary>
        private int IndexOf(int line, int column)
        {
            int index = column;

            if (Multiline)
            {
                string[] lines = Lines;

                for (int i = 0; i < line && i < lines.Length; i++)
                {
                    index += lines[i].Length + 1;
                }
            }

            return index;
        }

        /// <summary>
        /// Puts the cursor there. The selection goes, unless extend (Shift held) keeps its start.
        /// </summary>
        private void MoveCursor(int line, int column, bool extend)
        {
            _linePosition = line;
            _cursorPosition = column;

            if (!extend)
            {
                CollapseSelection();
            }

            _cursorVisible = true;
            MarkDirty();
        }

        /// <summary>
        /// Puts the cursor at that index in Text, as MoveCursor.
        /// </summary>
        private void MoveToIndex(int index, bool extend)
        {
            int line = 0;

            if (Multiline)
            {
                string[] lines = Lines;

                while (line < lines.Length - 1 && index > lines[line].Length)
                {
                    index -= lines[line].Length + 1;
                    line++;
                }

                index = Math.Min(index, lines[line].Length);
            }
            else
            {
                index = Math.Min(index, _text.Length);
            }

            MoveCursor(line, Math.Max(0, index), extend);
        }

        private void CollapseSelection()
        {
            _anchorLine = _linePosition;
            _anchorPosition = _cursorPosition;
        }

        private void SelectAll()
        {
            _anchorLine = 0;
            _anchorPosition = 0;
            MoveToIndex(_text.Length, true);
            ScrollToCursor();
        }

        /// <summary>
        /// Puts that text in place of the selection, or at the cursor; the cursor goes after it.
        /// </summary>
        private void ReplaceSelection(string text)
        {
            int start, end;
            GetSelection(out start, out end);

            Text = _text.Substring(0, start) + text + _text.Substring(end);
            MoveToIndex(start + text.Length, false);
            _preferredColumn = -1;

            // From the menu too, not only a key: the view follows the cursor.
            ScrollToCursor();
        }

        #endregion

        private void HandleLeftArrow(bool extend)
        {
            int start, end;
            GetSelection(out start, out end);

            // Without Shift, a selection leaves the cursor at its start.
            MoveToIndex(!extend && start != end ? start : Math.Max(0, CursorIndex - 1), extend);
        }

        private void HandleRightArrow(bool extend)
        {
            int start, end;
            GetSelection(out start, out end);

            MoveToIndex(!extend && start != end ? end : Math.Min(_text.Length, CursorIndex + 1), extend);
        }

        private void HandleUpArrow(bool extend)
        {
            if (Multiline && _linePosition > 0)
            {
                MoveToLine(_linePosition - 1, extend);
            }
        }

        private void HandleDownArrow(bool extend)
        {
            if (Multiline && _linePosition < Lines.Length - 1)
            {
                MoveToLine(_linePosition + 1, extend);
            }
        }

        /// <summary>
        /// Moves the cursor to that line, at the column the vertical moves started from, or at the
        /// line's end when the line is shorter.
        /// </summary>
        private void MoveToLine(int line, bool extend)
        {
            if (_preferredColumn < 0)
            {
                _preferredColumn = _cursorPosition;
            }

            MoveCursor(line, Math.Min(_preferredColumn, Lines[line].Length), extend);
        }

        private void HandleBackspace()
        {
            int index = CursorIndex;

            if (HasSelection)
            {
                ReplaceSelection("");
            }
            else if (index > 0)
            {
                // At a line's start, it joins the line to the one above.
                Text = _text.Remove(index - 1, 1);
                MoveToIndex(index - 1, false);
            }
        }

        private void HandleDelete()
        {
            int index = CursorIndex;

            if (HasSelection)
            {
                ReplaceSelection("");
            }
            else if (index < _text.Length)
            {
                Text = _text.Remove(index, 1);
                MoveToIndex(index, false);
            }
        }

        private void HandleEnter()
        {
            if (Multiline)
            {
                ReplaceSelection("\n");
            }
            else
            {
                Enter?.Invoke();
                MarkDirty();
            }
        }

        private void HandleDefaultKey(KeyEvent keyEvent)
        {
            if (IsTyped(keyEvent.KeyChar))
            {
                ReplaceSelection(keyEvent.KeyChar.ToString());
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

                // The selected characters in view.
                int from = 0;
                int to = 0;

                if (_isSelected)
                {
                    GetSelection(out from, out to);
                    from = Math.Max(0, from - _scrollOffset);
                    to = Math.Min(visibleText.Length, to - _scrollOffset);
                }

                if (Password)
                {
                    if (from < to)
                    {
                        DrawFilledRectangle(Kernel.SelectionColor, 2 + from * 8, TextPadding, (to - from) * 8, Kernel.font.Height);
                    }

                    int px = 0 + 6;
                    for (int i = 0; i < visibleText.Length; i++)
                    {
                        DrawFilledCircle(i >= from && i < to ? Kernel.WhiteColor : Kernel.BlackColor, px, Height / 2 - 3/2, 3);
                        px += 6 + 2;
                    }
                }
                else
                {
                    DrawString(visibleText, Kernel.font, Kernel.BlackColor, 0 + 4, 0 + 4);
                    DrawSelection(visibleText, from, to, TextPadding);

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

            int startLine, startColumn, endLine, endColumn;
            GetSelection(out startLine, out startColumn, out endLine, out endColumn);
            bool hasSelection = startLine != endLine || startColumn != endColumn;

            for (int row = 0; _scrollY + row < lines.Length; row++)
            {
                int y = TextPadding + row * Kernel.font.Height;

                if (y + Kernel.font.Height > textHeight)
                {
                    break;
                }

                int index = _scrollY + row;
                string line = lines[index];
                string visible = line.Length > _scrollX ? line.Substring(_scrollX, Math.Min(columns, line.Length - _scrollX)) : "";

                DrawString(visible, Kernel.font, Kernel.BlackColor, TextPadding, y);

                // A selected line break takes a column after the line.
                if (_isSelected && hasSelection && index >= startLine && index <= endLine)
                {
                    int from = index == startLine ? startColumn : 0;
                    int to = index == endLine ? endColumn : line.Length + 1;

                    DrawSelection(visible, Math.Max(0, from - _scrollX), Math.Min(columns, to - _scrollX), y);
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
        /// Draws the columns from..to (0 the first in view) of a line drawn at y on the selection
        /// color, their characters in white.
        /// </summary>
        private void DrawSelection(string visible, int from, int to, int y)
        {
            if (from >= to)
            {
                return;
            }

            DrawFilledRectangle(Kernel.SelectionColor, TextPadding + from * Kernel.font.Width, y, (to - from) * Kernel.font.Width, Kernel.font.Height);

            int textTo = Math.Min(to, visible.Length);

            if (from < textTo)
            {
                DrawString(visible.Substring(from, textTo - from), Kernel.font, Kernel.WhiteColor, TextPadding + from * Kernel.font.Width, y);
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

            CollapseSelection();
        }
    }
}
