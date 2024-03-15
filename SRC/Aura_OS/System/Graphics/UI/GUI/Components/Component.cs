/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
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

        public bool ForceDirty { get; set; }
        public RightClick RightClick { get; set; }
        public Component Parent { get; set; }
        public List<Component> Children { get; set; }
        public int zIndex { get; set; }
        public bool IsRoot { get; set; }
        public Frame Frame { get; set; }
        public State State { get; set; }

        private Rectangle _rectangle;
        private DirectBitmap _buffer;
        private DirectBitmap _cacheBuffer;
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
            _buffer = new DirectBitmap(width, height);
            _cacheBuffer = new DirectBitmap(width, height);
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

        public virtual void Draw()
        {
            if (Frame != null && Frame.Regions.Length > 0)
            {
                foreach (Frame.Region region in Frame.Regions)
                {
                    if (region.HorizontalPlacement == "stretch" && region.VerticalPlacement == "stretch")
                    {
                        Rectangle destRect = CalculateDestinationRect(region, Width, Height);
                        _buffer.DrawImageStretchAlpha(region.Texture, region.SourceRegion, destRect);
                    }
                }

                foreach (Frame.Region region in Frame.Regions)
                {
                    if (region.HorizontalPlacement == "stretch" ^ region.VerticalPlacement == "stretch")
                    {
                        Rectangle destRect = CalculateDestinationRect(region, Width, Height);
                        _buffer.DrawImageStretchAlpha(region.Texture, region.SourceRegion, destRect);
                    }
                }

                foreach (Frame.Region region in Frame.Regions)
                {
                    if (region.HorizontalPlacement != "stretch" && region.VerticalPlacement != "stretch")
                    {
                        Rectangle destRect = CalculateDestinationRect(region, Width, Height);
                        _buffer.DrawImageStretchAlpha(region.Texture, region.SourceRegion, destRect);
                    }
                }
            }
        }

        public virtual void Draw(Component component)
        {
            Draw();
            component._buffer.DrawImageAlpha(GetBuffer(), X, Y);
        }

        public void DrawInParent()
        {
            if (!IsRoot)
            {
                Parent._buffer.DrawImageAlpha(GetBuffer(), X, Y);
            }
        }

        private Rectangle CalculateDestinationRect(Frame.Region region, int frameWidth, int frameHeight)
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
                    x = 0;
                    width = frameWidth;
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
                    y = 0;
                    height = frameHeight;
                    break;
            }

            return new Rectangle(y, x, y + height, x + width);
        }

        public void SaveCacheBuffer()
        {
            _cacheBuffer.DrawImage(_buffer.Bitmap, 0, 0);
        }

        public void DrawCacheBuffer()
        {
            _buffer.DrawImage(_cacheBuffer.Bitmap, 0, 0);
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
                        entry.Click();
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

        public virtual void Resize(int width, int height)
        {
            if (width <= 0 || width >= 1000 || height <= 0 || height >= 1000)
            {
                return;
            }

            if (width % 2 != 0)
            {
                width++;
            }

            foreach (Component child in Children)
            {
                if (child is Button)
                {
                    child.X += width - Width;
                }
            }

            _rectangle = new Rectangle(Y, X, Y + height, X + width);
            _buffer = new DirectBitmap(width, height);
            _cacheBuffer = new DirectBitmap(width, height);

            Draw();
            foreach (Component child in Children)
            {
                if (child is Button)
                {
                    child.DrawInParent();
                }
            }
            SaveCacheBuffer();
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

        public Bitmap GetBuffer()
        {
            return _buffer.Bitmap;
        }

        public DirectBitmap GetDbBuffer()
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

        public void Clear(Color color)
        {
            _buffer.Clear(color.ToArgb());
        }

        public void Clear()
        {
            _buffer.Clear(Color.LightGray.ToArgb());
        }

        public void DrawString(string str, Color color, int x, int y)
        {
            _buffer.DrawString(str, Kernel.font, color.ToArgb(), x, y);
        }

        public void DrawString(string str, Font font, Color color, int x, int y)
        {
            _buffer.DrawString(str, font, color.ToArgb(), x, y);
        }

        public void DrawChar(char c, Font font, int color, int x, int y)
        {
            _buffer.DrawChar(c, font, color, x, y);
        }

        public void DrawString(string str, int x, int y)
        {
            _buffer.DrawString(str, Kernel.font, Color.Black.ToArgb(), x, y);
        }

        public void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
        {
            _buffer.DrawFilledRectangle(color.ToArgb(), xStart, yStart, width, height);
        }

        public void DrawLine(Color color, int xStart, int yStart, int width, int height)
        {
            _buffer.DrawLine(color.ToArgb(), xStart, yStart, width, height);
        }

        public void DrawImage(Bitmap image, int x, int y)
        {
            _buffer.DrawImageAlpha(image, x, y);
        }

        public void DrawFilledCircle(Color color, int x, int y, int radius)
        {
            _buffer.DrawFilledCircle(color.ToArgb(), x, y, radius);
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

                int interpolatedColor = Color.FromArgb(0xff, r, g, b).ToArgb();

                for (int j = 0; j < height; j++)
                {
                    _buffer.SetPixelAlpha(x + i, y + j, interpolatedColor);
                }
            }
        }

        #endregion

        public void Dispose()
        {
            foreach (Component child in Children)
            {
                child.Dispose();
            }

            Components.Remove(this);
        }
    }
}