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
        public readonly int width;
        public readonly int height;

        public int x;
        public int y;
        int px;
        int py;
        bool lck = false;
        bool pressed;
        public bool visible = false;

        public Window Window;

        public App(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            Window = new Window(name, x, y, width + 1, height + 1);

            this.x = x + 3;
            this.y = y + Window.TopBar.Height + 3;
            this.width = width - 4;
            this.height = height - (Window.TopBar.Height + 4);
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
                    if (!HasWindowMoving && Window.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        //Focus window
                        foreach (var app in Kernel.WindowManager.apps)
                        {
                            if (app.Equals(this))
                            {
                                Kernel.WindowManager.apps.Remove(app);
                                Kernel.WindowManager.apps.Add(this);
                                Kernel.WindowManager.Focused = this;
                                break;
                            }
                        }
                    }
                    
                    if (!HasWindowMoving && Window.Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Stop();
                        return;
                    }
                    else if (!HasWindowMoving && Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        HasWindowMoving = true;

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

                    x = (int)(MouseManager.X - px + 3);
                    y = (int)(MouseManager.Y - py + Window.TopBar.Height + 3);
                }

                DrawWindow();
            }
        }

        public void DrawWindow()
        {
            Window.Update();
            UpdateApp();
        }
    }
}
