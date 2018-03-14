using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.UI.Util
{
    public class Point
    {
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
    }

    public class Area
    {
        public Area(int x, int y, int xmax, int ymax)
        {
            this.X = x;
            this.Y = y;
            this.XMAX = xmax;
            this.YMAX = ymax;
        }
        int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        int xmax;
        public int XMAX
        {
            get { return xmax; }
            set { xmax = value; }
        }
        int ymax;
        public int YMAX
        {
            get { return ymax; }
            set { ymax = value; }
        }
    }
}
