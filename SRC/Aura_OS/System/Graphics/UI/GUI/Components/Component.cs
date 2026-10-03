/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Aura_OS.System.Graphics.UI.GUI.Skin;
using Aura_OS.System.Processing.Processes;
using System;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public enum State
    {
        Normal,
        Highlighted,
        Pressed,
    }

    public class Component : IDisposable
    {
        public static List<Component> Components = new List<Component>();

        public int AbsoluteX
        {
            get
            {
                return _absoluteX;
            }
        }

        public int AbsoluteY
        {
            get
            {
                return _absoluteY;
            }
        }

        public int X
        {
            get
            {
                return _rectangle.Left;
            }
            set
            {
                int width = Width;
                _rectangle.Left = value;
                _rectangle.Right = value + width;

                ComputeAbsoluteX();
            }
        }

        public int Y
        {
            get
            {
                return _rectangle.Top;
            }
            set
            {
                int height = Height;
                _rectangle.Top = value;
                _rectangle.Bottom = value + height;

                ComputeAbsoluteY();
            }
        }

        public int Width
        {
            get
            {
                return _rectangle.Right - _rectangle.Left;
            }
        }

        public int Height
        {
            get
            {
                return _rectangle.Bottom - _rectangle.Top;
            }
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    foreach (Component component in Children)
                    {
                        component.Visible = value;
                    }
                }
            }
        }

        /// <summary>
        /// Size the content wants (a label's text, a picture's image), -1 when the component has
        /// none: a layout then keeps the size it was created with.
        /// </summary>
        public virtual int PreferredWidth => -1;
        public virtual int PreferredHeight => -1;

        public bool ForceDirty { get; set; }
        public RightClick RightClick { get; set; }
        public Component Parent { get; set; }
        public List<Component> Children { get; set; }
        public int zIndex { get; set; }
        public bool IsRoot { get; set; }
        public Frame Frame { get; set; }
        public State State { get; set; }

        private Rectangle _rectangle;
        private Canvas _buffer;
        private Canvas _cacheBuffer;
        private bool _dirty;
        private bool _visible = true;

        private Frame _normalFrame;
        private Frame _highlightedFrame;
        private Frame _pressedFrame;

        private int _absoluteX;
        private int _absoluteY;

        public Component(int x, int y, int width, int height)
        {
            _rectangle = new Rectangle(y, x, y + height, x + width);
            _buffer = NewBuffer(width, height);
            _dirty = true;
            Visible = true;
            ForceDirty = false;
            IsRoot = true;
            Children = new List<Component>();
            zIndex = 0;
            Explorer.WindowManager.AddComponent(this);
            State = State.Normal;
        }

        public virtual void Update()
        {
            bool isInside = IsInside((int)MouseManager.X, (int)MouseManager.Y);
            State prevState = State;

            if (isInside && State != State.Highlighted)
            {
                State = State.Highlighted;
                MarkDirty();
            }
            else if (!isInside && State != State.Normal)
            {
                State = State.Normal;
                MarkDirty();
            }

            if (prevState != State) 
            {
                UpdateFrame();
            }
        }

        public void UpdateFrame()
        {
            switch (State)
            {
                case State.Normal:
                    Frame = _normalFrame;
                    break;
                case State.Highlighted:
                    Frame = _highlightedFrame;
                    break;
                case State.Pressed:
                    Frame = _pressedFrame;
                    break;
            }
        }

        /// <summary>
        /// Draws the frame (nine-slice skin regions) over the whole component. The regions do not
        /// overlap and start from a transparent buffer, so the transparent pixels of the corners
        /// stay transparent (rounded corners) and a redraw does not blend a translucent pixel twice.
        /// </summary>
        public virtual void Draw()
        {
            if (Frame != null && Frame.Regions.Length > 0)
            {
                Clear(Color.Transparent);
                DrawFrame(Frame, 0, 0, Width, Height);
            }
        }

        /// <summary>
        /// Draws a skin frame's regions over that rectangle of the buffer, blending (a part of the
        /// component: a scroll bar's rail and thumb).
        /// </summary>
        protected void DrawFrame(Frame frame, int x, int y, int width, int height)
        {
            if (frame == null)
            {
                return;
            }

            foreach (Frame.Region region in frame.Regions)
            {
                Rectangle destination = CalculateDestinationRect(frame, region, width, height);
                DrawRegion(region, new Rectangle(destination.Top + y, destination.Left + x, destination.Bottom + y, destination.Right + x));
            }
        }

        public virtual void Draw(Component component)
        {
            Draw();

            // Blend over the parent's background, not over this component's previous pixels: a
            // translucent pixel (a logo's shadow, a skin's rounded corner) blended onto itself gets
            // darker on every redraw.
            component.RestoreBackground(X, Y, Width, Height);
            component._buffer.DrawCanvas(_buffer, X, Y);
        }

        public void DrawInParent()
        {
            if (!IsRoot)
            {
                Parent._buffer.DrawCanvas(_buffer, X, Y);
            }
        }

        /// <summary>
        /// Draws a skin region stretched over its place in the frame, blending (nine-slice drawing).
        /// </summary>
        private void DrawRegion(Frame.Region region, Rectangle destination)
        {
            Rectangle source = region.SourceRegion;

            // A null dereference is a fatal #PF in gen3: skip a region the theme left incomplete.
            // A component smaller than its borders leaves a stretched region no room.
            if (region.Texture == null || source == null || destination.Width <= 0 || destination.Height <= 0)
            {
                return;
            }

            _buffer.DrawImage(region.Texture,
                new global::System.Drawing.Rectangle(destination.Left, destination.Top, destination.Width, destination.Height),
                new global::System.Drawing.Rectangle(source.Left, source.Top, source.Width, source.Height));
        }

        /// <summary>
        /// An off-screen canvas for a component of that size (a negative size from a shrunk window is empty).
        /// </summary>
        private static Canvas NewBuffer(int width, int height)
        {
            return new Canvas(Math.Max(0, width), Math.Max(0, height));
        }

        private static Rectangle CalculateDestinationRect(Frame frame, Frame.Region region, int frameWidth, int frameHeight)
        {
            int x = 0, y = 0, width = region.SourceRegion.Width, height = region.SourceRegion.Height;

            switch (region.HorizontalPlacement)
            {
                case "left":
                    x = 0;
                    break;
                case "right":
                    x = frameWidth - width;
                    break;
                case "center":
                    x = (frameWidth - width) / 2;
                    break;
                case "stretch":
                    // Between the left and right regions, which keep their own pixels.
                    x = frame.LeftBorder;
                    width = frameWidth - frame.LeftBorder - frame.RightBorder;
                    break;
            }

            switch (region.VerticalPlacement)
            {
                case "top":
                    y = 0;
                    break;
                case "bottom":
                    y = frameHeight - height;
                    break;
                case "center":
                    y = (frameHeight - height) / 2;
                    break;
                case "stretch":
                    y = frame.TopBorder;
                    height = frameHeight - frame.TopBorder - frame.BottomBorder;
                    break;
            }

            return new Rectangle(y, x, y + height, x + width);
        }

        /// <summary>
        /// Copies the buffer into the cache buffer, allocated on first use: only windows being moved or
        /// resized use it, and a screen-sized component (desktop, login screen) would pay a second
        /// screen buffer for nothing.
        /// </summary>
        public void SaveCacheBuffer()
        {
            if (_cacheBuffer == null || _cacheBuffer.Width != Width || _cacheBuffer.Height != Height)
            {
                _cacheBuffer = NewBuffer(Width, Height);
            }

            // An exact copy, transparent pixels included (DrawCanvas would blend them over the old cache).
            _cacheBuffer.DrawArray(_buffer.GetBuffer(), 0, 0, Width, Height);
        }

        public void DrawCacheBuffer()
        {
            if (_cacheBuffer != null)
            {
                _buffer.DrawArray(_cacheBuffer.GetBuffer(), 0, 0, Width, Height);
            }
        }

        /// <summary>
        /// Copies that rectangle of the cache buffer (an app window's background, saved without its
        /// controls) back into the buffer. Nothing without a cache buffer of the current size.
        /// </summary>
        private void RestoreBackground(int x, int y, int width, int height)
        {
            if (_cacheBuffer == null || _cacheBuffer.Width != Width || _cacheBuffer.Height != Height)
            {
                return;
            }

            int left = Math.Max(0, x);
            int top = Math.Max(0, y);
            int right = Math.Min(Width, x + width);
            int bottom = Math.Min(Height, y + height);

            if (right <= left)
            {
                return;
            }

            int[] cache = _cacheBuffer.GetBuffer();
            int[] buffer = _buffer.GetBuffer();

            for (int row = top; row < bottom; row++)
            {
                int index = row * Width + left;
                Array.Copy(cache, index, buffer, index, right - left);
            }
        }

        public virtual void HandleLeftClick()
        {
            RightClick contextMenu = Explorer.WindowManager.ContextMenu;

            if (contextMenu != null && contextMenu.Opened)
            {
                Explorer.WindowManager.ContextMenu = null;
                contextMenu.Opened = false;

                foreach (var entry in contextMenu.Entries)
                {
                    if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
                        if (entry.Click != null)
                        {
                            entry.Click();
                        }
                        return;
                    }
                }
            }
        }

        public virtual void HandleRightClick()
        {
            RightClick contextMenu = Explorer.WindowManager.ContextMenu;

            if (contextMenu != null && contextMenu.Opened)
            {
                contextMenu.Opened = false;
                Explorer.WindowManager.ContextMenu = null;
            }

            if (RightClick != null)
            {
                if ((int)MouseManager.Y + RightClick.Height >= Kernel.ScreenHeight)
                {
                    RightClick.Y = (int)MouseManager.Y - RightClick.Height;
                }
                else
                {
                    RightClick.Y = (int)MouseManager.Y;
                }

                if ((int)MouseManager.X + RightClick.Width >= Kernel.ScreenWidth)
                {
                    RightClick.X = (int)MouseManager.X - RightClick.Width;
                }
                else
                {
                    RightClick.X = (int)MouseManager.X;
                }

                RightClick.Opened = true;
                Explorer.WindowManager.ContextMenu = RightClick;
                Explorer.WindowManager.BringToFront(RightClick);
            }
        }

        public bool IsInside(int x, int y)
        {
            return x >= AbsoluteX && x <= AbsoluteX + Width && y >= AbsoluteY && y <= AbsoluteY + Height;
        }

        /// <summary>
        /// The largest width or height Resize accepts.
        /// </summary>
        public const int MaxSize = 999;

        public virtual void Resize(int width, int height)
        {
            if (width <= 0 || width > MaxSize || height <= 0 || height > MaxSize)
            {
                return;
            }

            if (width % 2 != 0)
            {
                width++;
            }

            int deltaWidth = width - Width;

            _rectangle = new Rectangle(Y, X, Y + height, X + width);
            _buffer = NewBuffer(width, height);
            _cacheBuffer = null;

            OnResized(deltaWidth);
            Draw();
            SaveCacheBuffer();
        }

        /// <summary>
        /// Called by Resize once the component has its new size, before it redraws: the place to
        /// move the children anchored to the right edge.
        /// </summary>
        protected virtual void OnResized(int deltaWidth)
        {
        }

        /// <summary>
        /// Gives the component a new size and blank buffers, without the window limits of Resize and
        /// without moving the children. For the screen-sized components after a resolution change,
        /// and the controls a layout stretches.
        /// </summary>
        public virtual void SetSize(int width, int height)
        {
            // Drop the old buffers first, so a collection during the allocation can reclaim them.
            _buffer = null;
            _cacheBuffer = null;

            _rectangle = new Rectangle(Y, X, Y + height, X + width);
            _buffer = NewBuffer(width, height);

            ComputeAbsoluteCoordinates();
            MarkDirty();
        }

        public virtual bool IsDirty()
        {
            return _dirty;
        }

        public virtual void MarkDirty()
        {
            _dirty = true;
        }

        public virtual void MarkCleaned()
        {
            _dirty = false;
        }

        public void SetNormalFrame(Frame frame)
        {
            Frame = frame;
            _normalFrame = frame;
        }

        public void SetHighlightedFrame(Frame frame)
        {
            _highlightedFrame = frame;
        }

        public void SetPressedFrame(Frame frame)
        {
            _pressedFrame = frame;
        }

        public void AddChild(Component child)
        {
            child.Parent = this;
            child.zIndex = zIndex + 1 + Children.Count;
            child.IsRoot = false;
            child.ComputeAbsoluteCoordinates();
            Children.Add(child);
        }

        public Rectangle GetRectangle()
        {
            return _rectangle;
        }

        public Rectangle GetAbsoluteRectangle()
        {
            Rectangle rectangle = new Rectangle(AbsoluteY, AbsoluteX, AbsoluteY + Height, AbsoluteX + Width);
            return rectangle;
        }

        /// <summary>
        /// The component's off-screen canvas. Replaced by Resize and SetSize: ask again afterwards.
        /// </summary>
        public Canvas GetBuffer()
        {
            return _buffer;
        }

        public void ComputeAbsoluteCoordinates()
        {
            ComputeAbsoluteX();
            ComputeAbsoluteY();
        }

        public void ComputeAbsoluteX()
        {
            int absoluteX = 0;
            Component currentComponent = this;

            while (currentComponent != null)
            {
                absoluteX += currentComponent.X;

                if (currentComponent.IsRoot) break;

                currentComponent = currentComponent.Parent;
            }

            _absoluteX = absoluteX;

            for (int i = 0; i < Children.Count; i++)
            {
                Component child = Children[i];
                child.ComputeAbsoluteX();
            }
        }

        public void ComputeAbsoluteY()
        {
            int absoluteY = 0;
            Component currentComponent = this;

            while (currentComponent != null)
            {
                absoluteY += currentComponent.Y;

                if (currentComponent.IsRoot) break;

                currentComponent = currentComponent.Parent;
            }

            _absoluteY = absoluteY;

            for (int i = 0; i < Children.Count; i++)
            {
                Component child = Children[i];
                child.ComputeAbsoluteY();
            }
        }

        #region Draw

        // Canvas throws on a null string, font or image; a null dereference is a fatal #PF in gen3,
        // so the wrappers skip the draw instead.

        public void Clear(Color color)
        {
            _buffer.Clear(color);
        }

        public void Clear()
        {
            _buffer.Clear(Color.LightGray);
        }

        public void DrawString(string str, Color color, int x, int y)
        {
            DrawString(str, Kernel.font, color, x, y);
        }

        public void DrawString(string str, Font font, Color color, int x, int y)
        {
            if (str != null && font != null)
            {
                _buffer.DrawString(str, font, color, x, y);
            }
        }

        public void DrawChar(char c, Font font, int color, int x, int y)
        {
            if (font != null)
            {
                _buffer.DrawChar(c, font, Color.FromArgb(color), x, y);
            }
        }

        public void DrawString(string str, int x, int y)
        {
            DrawString(str, Kernel.font, Color.Black, x, y);
        }

        public void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
        {
            _buffer.DrawFilledRectangle(color, xStart, yStart, width, height);
        }

        public void DrawLine(Color color, int xStart, int yStart, int xEnd, int yEnd)
        {
            _buffer.DrawLine(color, xStart, yStart, xEnd, yEnd);
        }

        public void DrawImage(Image image, int x, int y)
        {
            if (image != null)
            {
                _buffer.DrawImage(image, x, y);
            }
        }

        public void DrawFilledCircle(Color color, int x, int y, int radius)
        {
            _buffer.DrawFilledCircle(color, x, y, radius);
        }

        public void DrawGradient(Color color1, Color color2, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                // Calculate the ratio of the current position relative to the total width
                float ratio = (float)i / width;

                // Interpolate the RGB values based on the ratio
                byte r = (byte)((color2.R - color1.R) * ratio + color1.R);
                byte g = (byte)((color2.G - color1.G) * ratio + color1.G);
                byte b = (byte)((color2.B - color1.B) * ratio + color1.B);

                _buffer.DrawFilledRectangle(Color.FromArgb(0xff, r, g, b), x + i, y, 1, height);
            }
        }

        #endregion

        public virtual void Dispose()
        {
            foreach (Component child in Children)
            {
                child.Dispose();
            }

            Components.Remove(this);
        }
    }
}