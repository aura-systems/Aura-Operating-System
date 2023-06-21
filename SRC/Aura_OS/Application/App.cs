using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;
using Aura_OS.Processing;
using System;
using Aura_OS.System.Graphics.UI;

namespace Aura_OS
{
    public class App : Process
    {
        public static bool HasWindowMoving = false;

        public Bitmap Icon;

        public readonly int width;
        public readonly int height;

        public int x;
        public int y;
        public string name;

        int px;
        int py;
        bool lck = false;

        bool pressed;
        public bool visible = false;

        const int MoveBarHeight = 20;

        public Window Window;

        public App(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            Icon = Kernel.programIco;

            this.x = x + 2;
            this.y = y + MoveBarHeight;
            this.width = width - 4;
            this.height = height - MoveBarHeight - 1;

            this.name = name;

            Window = new Window(name, x, y, width, height);
        }

        public virtual void UpdateApp() { }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public override void Update()
        {
            if (visible)
            {
                if (Kernel.Pressed)
                {
                    if (!HasWindowMoving && Window.Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Stop();
                        return;
                    }
                    else if (!HasWindowMoving && Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        HasWindowMoving = true;

                        //Focus window
                        foreach (var app in Kernel.WindowManager.apps)
                        {
                            if (app.Equals(this))
                            {
                                Kernel.WindowManager.apps.Remove(app);
                                Kernel.WindowManager.apps.Add(this);
                                break;
                            }
                        }

                        this.pressed = true;
                        if (!lck)
                        {
                            px = (int)MouseManager.X - Window.X;
                            py = (int)MouseManager.Y - Window.Y;
                            lck = true;
                        }
                    }
                }
                else
                {
                    pressed = false;
                    lck = false;
                    HasWindowMoving = false;
                }

                if (pressed)
                {
                    Window.X = (int)(MouseManager.X - px);
                    Window.Y = (int)(MouseManager.Y - py);

                    x = (int)(MouseManager.X - px + 2);
                    y = (int)(MouseManager.Y - py + MoveBarHeight);
                }

                DrawWindow();

                //Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);
                //Kernel.canvas.DrawRectangle(Kernel.avgColPen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);

                //Kernel.canvas.DrawImage(Icon, (int)(baseX + 2), (int)(baseY + 2));
                //Kernel.canvas.DrawString(name, Kernel.font, Kernel.BlackColor, (int)(baseX + Icon.Width + 2), (int)(baseY + 2));
            }
        }

        public void DrawWindow()
        {
            Window.Update();
            UpdateApp();
        }
    }
}
