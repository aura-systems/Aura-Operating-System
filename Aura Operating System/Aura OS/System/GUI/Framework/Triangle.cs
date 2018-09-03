using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.GUI.Graphics;

namespace Aura_OS.System.GUI
{
    public class Triangle
    {
        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }
        

        public Point[] ToArray()
        {
            return new[]
            {
                A, B, C
            };
        }

        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }
    }

}
