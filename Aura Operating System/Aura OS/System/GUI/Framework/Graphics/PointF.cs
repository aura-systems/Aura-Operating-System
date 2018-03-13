using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.Graphics
{
    public class PointF
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float TriangleArea(Point b, Point c)
        {
            float x1 = b.X - X;
            float y1 = b.Y - Y;

            float x2 = c.X - X;
            float y2 = c.Y - Y;

            return (x1 * y2 - x2 * y1);
        }
    }
}
