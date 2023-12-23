/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Component
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public RightClick RightClick;

        public Component(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public virtual void Update()
        {
        }

        public bool IsInside(int x, int y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }
    }
}