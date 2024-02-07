/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.System.Processing.Processes;
using Cosmos.Core;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using JZero.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Component
    {
        public static List<Component> Components = new List<Component>();

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
        public bool HasTransparency { get; set; }
        public RightClick RightClick { get; set; }
        public List<Component> Children { get; set; }
        public int zIndex { get; set; }
        public bool IsRoot { get; set; }

        private Rectangle _rectangle;
        private DirectBitmap _buffer;
        private bool _dirty;
        private bool _visible = true;

        public Component(int x, int y, int width, int height)
        {
            _rectangle = new Rectangle(y, x, y + height, x + width);
            _buffer = new DirectBitmap(width, height);
            _dirty = true;
            Visible = true;
            ForceDirty = false;
            HasTransparency = false;
            IsRoot = true;
            Children = new List<Component>();
            zIndex = 0;
            Explorer.WindowManager.AddComponent(this);
        }

        public virtual void Update()
        {

        }

        public virtual void Draw()
        {

        }

        public void AddChild(Component child)
        {
            child.zIndex = zIndex + 1 + Children.Count;
            child.IsRoot = false;
            Children.Add(child);
        }

        public Rectangle GetRectangle()
        {
            return _rectangle;
        }

        public Bitmap GetBuffer()
        {
            return _buffer.Bitmap;
        }

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
            _buffer.DrawImage(image, x, y);
        }

        public void DrawImageAlpha(Bitmap image, int x, int y)
        {
            _buffer.DrawImage(image, x, y);
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
                    _buffer.SetPixel(x + i, y + j, interpolatedColor);
                }
            }
        }

        public virtual void HandleLeftClick()
        {
            if (RightClick != null)
            {
                if (RightClick.Opened)
                {
                    RightClick.Opened = false;

                    foreach (var entry in RightClick.Entries)
                    {
                        if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                        {
                            entry.Click();
                            return;
                        }
                    }
                }
            }
        }

        public virtual void HandleRightClick()
        {
            return;

            if (RightClick != null && IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                RightClick.X = (int)MouseManager.X;
                RightClick.Y = (int)MouseManager.Y;
                RightClick.Opened = true;
                Kernel.MouseManager.TopComponent = this;
            }
        }

        public bool IsInside(int x, int y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }

        public virtual bool IsDirty()
        {
            return _dirty;
        }

        public void MarkDirty()
        {
            _dirty = true;

            foreach (Component component in Children)
            {
                component.MarkDirty();
            }
        }

        public void MarkCleaned()
        {
            _dirty = false;
            
            foreach (Component component in Children)
            {
                component.MarkCleaned();
            }
        }
    }
}