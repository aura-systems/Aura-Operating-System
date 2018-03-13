using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.Graphics
{
    public class Rectangle
    {
        public Rectangle(Point location, Size size)
        {
            Location = location;
            Size = size;
        }

        public Rectangle(int x, int y, int w, int h)
        {
            Location = new Point(x, y);
            Size = new Size(w, h);
        }

        public Point Location { get; set; }
        public Size Size { get; set; }


        public bool Intersects(int x, int y)
        {
            return Intersects(new Point(x, y));
        }

        public bool Intersects(Point a)
        {
            if (a.X > Location.X && a.X < Location.X + Size.Width)
            {
                if (a.Y > Location.Y && a.Y < Location.Y + Size.Height)
                {
                    return true;
                }
            }
            return false;
        }
    }
}