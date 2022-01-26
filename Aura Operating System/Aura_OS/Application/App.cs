using Cosmos.System;
using System.Drawing;
using Aura_OS.Processing;

namespace Aura_OS
{
    public class App : Process
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

        public App(string name, uint width, uint height, uint x = 0, uint y = 0) : base(name, ProcessType.Program)
        {
            baseWidth = width;
            baseHeight = height;
            baseX = x;
            baseY = y;

            this.x = x + 2;
            this.y = y + MoveBarHeight;
            this.width = width - 4;
            this.height = height - MoveBarHeight - 1;

            this.name = name;
        }

        public virtual void UpdateApp() { }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        public override void Update()
        {
            if (_i != 0)
            {
                _i--;
            }

            if (MouseManager.X > dockX && MouseManager.X < dockX + dockWidth && MouseManager.Y > dockY && MouseManager.Y < dockY + dockHeight)
            {
                Kernel.canvas.DrawString(name, Kernel.font, Kernel.WhitePen, (int)(dockX - ((name.Length * 8) / 2) + dockWidth / 2), (int)(dockY - 20));
            }

            if (MouseManager.MouseState == MouseState.Left && _i == 0)
            {
                if (MouseManager.X > dockX && MouseManager.X < dockX + dockWidth && MouseManager.Y > dockY && MouseManager.Y < dockY + dockHeight)
                {
                    visible = !visible;

                    if (visible)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }

                    _i = 60;
                }
            }

            if (visible)
            {
                if (Kernel.Pressed)
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
                    pressed = false;
                    lck = false;
                }

                if (pressed)
                {
                    baseX = (uint)(MouseManager.X - px);
                    baseY = (uint)(MouseManager.Y - py);

                    x = (uint)(MouseManager.X - px + 2);
                    y = (uint)(MouseManager.Y - py + MoveBarHeight);
                }

                Kernel.canvas.DrawFilledRectangle(Kernel.WhitePen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);
                Kernel.canvas.DrawRectangle(Kernel.avgColPen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);

                Kernel.canvas.DrawString(name, Kernel.font, Kernel.BlackPen, (int)(baseX + 2), (int)(baseY + 2));

                UpdateApp();
            }
        }
    }
}
