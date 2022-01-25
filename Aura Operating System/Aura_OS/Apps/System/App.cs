using Cosmos.System;
using System.Drawing;

namespace Aura_OS.Apps.System
{
    public class App
    {
        public readonly uint baseWidth;
        public readonly uint baseHeight;
        public readonly uint width;
        public readonly uint height;

        public uint dockX;
        public uint dockY;
        public uint dockWidth = 40;
        public uint dockHeight = 30;

        public uint baseX;
        public uint baseY;
        public uint x;
        public uint y;
        public string name;

        int px;
        int py;
        bool lck = false;

        bool pressed;
        public bool visible = false;

        const int MoveBarHeight = 20;

        public int _i = 0;

        public App(uint width, uint height, uint x = 0, uint y = 0)
        {
            this.baseWidth = width;
            this.baseHeight = height;
            this.baseX = x;
            this.baseY = y;

            this.x = x + 2;
            this.y = y + MoveBarHeight;
            this.width = width - 4;
            this.height = height - MoveBarHeight - 1;
        }

        public void Update()
        {
            if (_i != 0)
            {
                _i--;
            }

            if (MouseManager.X > dockX && MouseManager.X < dockX + dockWidth && MouseManager.Y > dockY && MouseManager.Y < dockY + dockHeight)
            {
                GUI.canvas.DrawString(name, GUI.font, GUI.WhitePen, (int)(dockX - ((name.Length * 8) / 2) + dockWidth / 2), (int)(dockY - 20));
            }

            if (MouseManager.MouseState == MouseState.Left && _i == 0)
            {
                if (MouseManager.X > dockX && MouseManager.X < dockX + dockWidth && MouseManager.Y > dockY && MouseManager.Y < dockY + dockHeight)
                {
                    visible = !visible;
                    _i = 60;
                }
            }

            if (GUI.Pressed)
            {
                if (MouseManager.X > baseX && MouseManager.X < baseX + baseWidth && MouseManager.Y > baseY && MouseManager.Y < baseY + MoveBarHeight)
                {
                    this.pressed = true;
                    if (!lck)
                    {
                        px = (int)((int)MouseManager.X - this.baseX);
                        py = (int)((int)MouseManager.Y - this.baseY);
                        lck = true;
                    }
                }
            }
            else
            {
                this.pressed = false;
                lck = false;
            }

            if (!visible)
                goto end;

            if (this.pressed)
            {
                this.baseX = (uint)(MouseManager.X - px);
                this.baseY = (uint)(MouseManager.Y - py);

                this.x = (uint)(MouseManager.X - px + 2);
                this.y = (uint)(MouseManager.Y - py + MoveBarHeight);
            }

            GUI.canvas.DrawFilledRectangle(GUI.WhitePen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);
            GUI.canvas.DrawRectangle(GUI.avgColPen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);

            GUI.canvas.DrawString(name, GUI.font, GUI.BlackPen, (int)(baseX + 2), (int)(baseY + 2));

            _Update();

        end:;
        }

        public virtual void _Update()
        {
        }
    }
}
