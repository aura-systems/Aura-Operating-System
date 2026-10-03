/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List box class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI.Skin;
using Aura_OS.System.Processing.Processes;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Mouse;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// A list of text rows, each with an optional 16 x 16 icon before it, one of which can be
    /// selected: a click on a row selects it and calls SelectionChanged. A click also gives the box the
    /// keys (as a text box takes them, or Focus): while its window is focused, the up and down arrows,
    /// Home, End, Page Up and Page Down move the selection the same way. Enter, or a double click on
    /// a row, calls Activated; the other keys go to the app (Application.HandleKey). Once the rows
    /// outgrow the box, a vertical scroll bar on the right scrolls through them. A row longer than the
    /// box is cut.
    /// </summary>
    public class ListBox : Component
    {
        // The scroll bar, the size of the multiline TextBox's.
        private const int ScrollBarSize = 15;
        private const int MinThumbSize = 16;

        // Space left of a row's text, or of its icon.
        private const int TextPadding = 4;

        // The rows' icons, and the space between an icon and its text.
        private const int IconSize = 16;
        private const int IconSpacing = 4;

        public List<string> Items = new List<string>();
        public Action SelectionChanged;

        /// <summary>
        /// Enter, or a double click on a row: the selected row is opened.
        /// </summary>
        public Action Activated;

        // The icon of each row of Items, null for none (AddItem keeps both in step).
        private readonly List<Bitmap> _icons = new List<Bitmap>();

        // The row the last click was on, -1 for none, and when (Environment.TickCount64): a second
        // click on it soon after is a double click.
        private int _clickedIndex = -1;
        private long _clickedAt;

        // As MouseManager's.
        private const long DoubleClickTime = 500;

        private int _selectedIndex = -1;

        // The row at the top of the view.
        private int _top = 0;

        private Frame _rail;
        private Frame _thumb;
        private Frame _thumbHighlighted;
        private Frame _thumbPressed;

        // The thumb's state when last drawn: the mouse coming over it or leaving it redraws.
        private Frame _drawnThumb;

        private bool _dragging = false;

        // Where the thumb was taken, from its top.
        private int _scrollGrab;

        public ListBox(int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Kernel.ThemeManager.GetFrame("list"));

            _rail = Kernel.ThemeManager.GetFrame("rail.vertical");
            _thumb = Kernel.ThemeManager.GetFrame("slider.vertical.normal");
            _thumbHighlighted = Kernel.ThemeManager.GetFrame("slider.vertical.highlighted");
            _thumbPressed = Kernel.ThemeManager.GetFrame("slider.vertical.depressed");
        }

        /// <summary>
        /// Index of the selected row in Items, -1 when none. Setting it scrolls the row into view and
        /// does not call SelectionChanged.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = (value >= 0 && value < Items.Count) ? value : -1;
                ScrollIntoView(_selectedIndex);
                MarkDirty();
            }
        }

        /// <summary>
        /// The selected row, null when none.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return _selectedIndex >= 0 ? Items[_selectedIndex] : null;
            }
        }

        /// <param name="icon">Drawn before the text, null for none. Once a row has one, the texts of
        /// all the rows start after the place of an icon, aligned.</param>
        public void AddItem(string item, Bitmap icon = null)
        {
            Items.Add(item ?? "");
            _icons.Add(icon);
            MarkDirty();
        }

        /// <summary>
        /// Removes every row: nothing is selected, the view goes back to the top.
        /// </summary>
        public void ClearItems()
        {
            Items.Clear();
            _icons.Clear();
            _selectedIndex = -1;
            _clickedIndex = -1;
            _top = 0;
            MarkDirty();
        }

        #region Geometry (box coordinates)

        // The rows are inside the list frame's 1-pixel border.
        private static int RowHeight => Kernel.font.Height + 2;

        private int VisibleRows => Math.Max(1, (Height - 2) / RowHeight);

        private bool HasIcons
        {
            get
            {
                for (int i = 0; i < _icons.Count; i++)
                {
                    if (_icons[i] != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private int MaxTop => Math.Max(0, Items.Count - VisibleRows);

        private bool HasScrollBar => Items.Count > VisibleRows;

        private int TextAreaWidth => HasScrollBar ? Width - 1 - ScrollBarSize : Width - 1;

        private int BarX => Width - 1 - ScrollBarSize;
        private int BarY => 1;
        private int BarHeight => Math.Max(0, Height - 2);

        // The thumb is as long, relative to its bar, as the part of the list in view.
        private int ThumbHeight => Math.Min(BarHeight, Math.Max(MinThumbSize, BarHeight * VisibleRows / Math.Max(1, Items.Count)));
        private int ThumbY => BarY + (MaxTop > 0 ? (BarHeight - ThumbHeight) * _top / MaxTop : 0);

        #endregion

        public override void HandleLeftClick()
        {
            // Closes an open menu, as a click anywhere else does.
            base.HandleLeftClick();

            Focus();
            _clickedIndex = -1;

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;

            // On the rail: drag the thumb, brought under the mouse when the press is beside it.
            if (HasScrollBar && x >= BarX)
            {
                bool onThumb = y >= ThumbY && y < ThumbY + ThumbHeight;
                _scrollGrab = onThumb ? y - ThumbY : ThumbHeight / 2;
                _dragging = true;
                ScrollToThumb(y - _scrollGrab);
                MarkDirty();
                return;
            }

            int row = (y - 1) / RowHeight;

            if (y < 1 || row >= VisibleRows)
            {
                return;
            }

            int index = _top + row;

            if (index >= Items.Count)
            {
                return;
            }

            _clickedIndex = index;
            _clickedAt = Environment.TickCount64;

            if (index != _selectedIndex)
            {
                Select(index);
            }
        }

        /// <summary>
        /// A second click on the row the first one selected activates it; anywhere else, it is a click.
        /// </summary>
        public override void HandleLeftDoubleClick()
        {
            // The first click may have been elsewhere: MouseManager times any two clicks.
            int clicked = Environment.TickCount64 - _clickedAt <= DoubleClickTime ? _clickedIndex : -1;

            HandleLeftClick();

            if (clicked >= 0 && _clickedIndex == clicked && clicked == _selectedIndex)
            {
                // A third quick click selects again rather than opening twice.
                _clickedIndex = -1;
                Activate();
            }
        }

        /// <summary>
        /// Calls Activated, when a row is selected.
        /// </summary>
        private void Activate()
        {
            // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
            if (_selectedIndex >= 0 && Activated != null)
            {
                Activated();
            }
        }

        /// <summary>
        /// Selects the row at that index, scrolled into view, and calls SelectionChanged.
        /// </summary>
        private void Select(int index)
        {
            SelectedIndex = index;

            // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
            if (SelectionChanged != null)
            {
                SelectionChanged();
            }
        }

        /// <summary>
        /// The keys come to the box from now on, rather than to the text box that had them.
        /// </summary>
        public void Focus()
        {
            TextBox textBox = Kernel.MouseManager.FocusedComponent as TextBox;

            if (textBox != null)
            {
                textBox.SetSelected(false);
            }

            Kernel.MouseManager.FocusedComponent = this;
        }

        /// <summary>
        /// The box's app while the box has the keys (it was clicked or focused last, and its window is
        /// the focused one), else null.
        /// </summary>
        private Application KeysApp()
        {
            if (!ReferenceEquals(Kernel.MouseManager.FocusedComponent, this))
            {
                return null;
            }

            Component root = this;

            while (root.Parent != null)
            {
                root = root.Parent;
            }

            Application app = Explorer.WindowManager.FocusedApp;
            return app != null && ReferenceEquals(app.Window, root) ? app : null;
        }

        /// <summary>
        /// Moves the selection with the keys typed: up and down a row, Home and End to the first and
        /// last rows, Page Up and Page Down a view. With no row selected, the first move selects the
        /// first row. Enter activates the selected row; the other keys go to the app.
        /// </summary>
        private void ReadKeys(Application app)
        {
            KeyEvent key;

            // A key's handler may give the keys to another control, or open a window: the next keys
            // are theirs.
            while (ReferenceEquals(KeysApp(), app) && Input.KeyboardManager.TryGetKey(out key))
            {
                // GEN3-GAP(null-deref): a null event would halt the kernel.
                if (key == null)
                {
                    continue;
                }

                if (key.Key == ConsoleKeyEx.Enter)
                {
                    Activate();
                    continue;
                }

                if (Items.Count == 0 || !IsMoveKey(key.Key))
                {
                    app.HandleKey(key);
                    continue;
                }

                int page = Math.Max(1, VisibleRows - 1);
                int index = _selectedIndex;

                switch (key.Key)
                {
                    case ConsoleKeyEx.UpArrow:
                        index = index < 0 ? 0 : index - 1;
                        break;
                    case ConsoleKeyEx.DownArrow:
                        index = index < 0 ? 0 : index + 1;
                        break;
                    case ConsoleKeyEx.Home:
                        index = 0;
                        break;
                    case ConsoleKeyEx.End:
                        index = Items.Count - 1;
                        break;
                    case ConsoleKeyEx.PageUp:
                        index = index < 0 ? 0 : index - page;
                        break;
                    case ConsoleKeyEx.PageDown:
                        index = index < 0 ? 0 : index + page;
                        break;
                }

                index = Math.Max(0, Math.Min(index, Items.Count - 1));

                if (index != _selectedIndex)
                {
                    Select(index);
                }
            }
        }

        private static bool IsMoveKey(ConsoleKeyEx key)
        {
            switch (key)
            {
                case ConsoleKeyEx.UpArrow:
                case ConsoleKeyEx.DownArrow:
                case ConsoleKeyEx.Home:
                case ConsoleKeyEx.End:
                case ConsoleKeyEx.PageUp:
                case ConsoleKeyEx.PageDown:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the keys while the box has them, drags the thumb while the button pressed on the rail
        /// is down, and highlights the thumb under the mouse. The box itself does not change with the
        /// mouse.
        /// </summary>
        public override void Update()
        {
            Application app = KeysApp();

            if (app != null)
            {
                ReadKeys(app);
            }

            if (!HasScrollBar || !Kernel.MouseManager.IsLeftButtonDown)
            {
                _dragging = false;
            }

            if (!HasScrollBar)
            {
                return;
            }

            if (_dragging)
            {
                ScrollToThumb((int)MouseManager.Y - AbsoluteY - _scrollGrab);
            }

            if (GetThumbFrame() != _drawnThumb)
            {
                MarkDirty();
            }
        }

        /// <summary>
        /// Scrolls so the thumb starts at that y (box coordinates).
        /// </summary>
        private void ScrollToThumb(int thumbY)
        {
            int range = BarHeight - ThumbHeight;
            int top = range > 0 ? ((thumbY - BarY) * MaxTop + range / 2) / range : 0;
            top = Math.Max(0, Math.Min(top, MaxTop));

            if (top != _top)
            {
                _top = top;
                MarkDirty();
            }
        }

        /// <summary>
        /// Scrolls the least that shows that row, none for -1.
        /// </summary>
        private void ScrollIntoView(int index)
        {
            if (index < 0)
            {
                return;
            }

            if (index < _top)
            {
                _top = index;
            }
            else if (index >= _top + VisibleRows)
            {
                _top = index - VisibleRows + 1;
            }
        }

        private Frame GetThumbFrame()
        {
            if (_dragging)
            {
                return _thumbPressed;
            }

            int x = (int)MouseManager.X - AbsoluteX;
            int y = (int)MouseManager.Y - AbsoluteY;
            bool over = x >= BarX && x < BarX + ScrollBarSize && y >= ThumbY && y < ThumbY + ThumbHeight;

            return over ? _thumbHighlighted : _thumb;
        }

        /// <summary>
        /// The selected row keeps its place in view when the box gets smaller.
        /// </summary>
        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            ScrollIntoView(_selectedIndex);
        }

        public override void Draw()
        {
            base.Draw();

            // Rows may have gone, or the box grown, under the view.
            _top = Math.Max(0, Math.Min(_top, MaxTop));

            int rowWidth = TextAreaWidth - 1;
            bool icons = HasIcons;
            int textX = icons ? TextPadding + IconSize + IconSpacing : TextPadding;
            int columns = Math.Max(0, (rowWidth - textX) / Kernel.font.Width);

            for (int row = 0; row < VisibleRows && _top + row < Items.Count; row++)
            {
                int index = _top + row;
                int y = 1 + row * RowHeight;
                bool selected = index == _selectedIndex;

                if (selected)
                {
                    DrawFilledRectangle(Kernel.SelectionColor, 1, y, rowWidth, RowHeight);
                }

                Bitmap icon = index < _icons.Count ? _icons[index] : null;

                if (icon != null)
                {
                    DrawImage(icon, TextPadding, y + (RowHeight - (int)icon.Height) / 2);
                }

                string text = Items[index];

                if (text.Length > columns)
                {
                    text = text.Substring(0, columns);
                }

                DrawString(text, Kernel.font, selected ? Kernel.WhiteColor : Kernel.BlackColor, textX, y + 1);
            }

            if (HasScrollBar)
            {
                _drawnThumb = GetThumbFrame();

                DrawFrame(_rail, BarX, BarY, ScrollBarSize, BarHeight);
                DrawFrame(_drawnThumb, BarX, ThumbY, ScrollBarSize, ThumbHeight);
            }
        }
    }
}
