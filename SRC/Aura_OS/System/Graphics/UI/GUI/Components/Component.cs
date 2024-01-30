/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;

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

        public virtual void Draw()
        {

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
    }
}