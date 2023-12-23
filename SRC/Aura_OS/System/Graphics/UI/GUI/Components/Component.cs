/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Element base class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Component
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public RightClick RightClick;

        public bool LeftClicked = false;
        public bool RightClicked = false;

        public Component(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public virtual void Draw()
        {
            if (RightClick != null && RightClick.Opened)
            {
                foreach (var entry in RightClick.Entries)
                {
                    if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        entry.BackColor = Kernel.DarkBlue;
                        entry.TextColor = Kernel.WhiteColor;
                    }
                    else
                    {
                        entry.BackColor = Color.LightGray;
                        entry.TextColor = Kernel.BlackColor;
                    }
                }
                RightClick.Update();
            }
        }

        public virtual void Update()
        {
            if (MouseManager.MouseState == MouseState.Left)
            {
                if (!LeftClicked)
                {
                    LeftClicked = true;
                }
            }
            else if (MouseManager.MouseState == MouseState.Right)
            {
                if (RightClick != null)
                {
                    RightClicked = true;
                }
            }
            else if (LeftClicked)
            {
                LeftClicked = false;
                HandleLeftClick();
            }
            else if (RightClicked)
            {
                RightClicked = false;
                HandleRightClick();
            }
        }

        public virtual void HandleLeftClick()
        {
            if (RightClick != null)
            {
                if (RightClick.Opened)
                {
                    foreach (var entry in RightClick.Entries)
                    {
                        if (entry.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                        {
                            entry.Action();
                            return;
                        }
                    }

                    RightClick.Opened = false;
                }
            }
        }

        public virtual void HandleRightClick()
        {
            if (RightClick != null)
            {
                if (IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    RightClick.X = (int)MouseManager.X;
                    RightClick.Y = (int)MouseManager.Y;
                    RightClick.Opened = true;
                }
            }
        }

        public bool IsInside(int x, int y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }
    }
}