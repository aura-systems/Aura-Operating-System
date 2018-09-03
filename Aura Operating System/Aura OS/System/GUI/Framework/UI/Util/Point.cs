using Aura_OS.System.GUI.Graphics;
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
     public class Areak
    {
        public Areak(int x, int y, int sizex, int sizey, Color color)
        {
            this.X = x;
            this.Y = y;
            this.SIZEX = sizex;
            this.SIZEY = sizey;
            this.COLOR = color;
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
        int sizex;
        public int SIZEX
        {
            get { return sizex; }
            set { sizex = value; }
        }
        int sizey;
        public int SIZEY
        {
            get { return sizey; }
            set { sizey = value; }
        }
        Color color;
        public Color COLOR
        {
            get { return color; }
            set { color = value; }
        }
    }
}