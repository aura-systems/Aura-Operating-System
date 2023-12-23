/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Base app class
* PROGRAMMERS:      nifanfa <nifanfa@foxmail.com>
                    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using Aura_OS.System.Processing.Application;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Application : Process
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

        public Window Window;
        public bool Focused = false;
        public bool Visible = false;
        public int zIndex = 0;

        public Application(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            Window = new Window(name, x, y, width + 1, height + 1);
            Window.Close.Action = new Action(() =>
            {
                Stop();
                Kernel.WindowManager.apps.Remove(this);
                Kernel.ProcessManager.Processes.Remove(this);
                Kernel.Taskbar.UpdateApplicationButtons();

                if (this is Terminal)
                {
                    Kernel.console = null;
                }
            });
            Window.Minimize.Action = new Action(() =>
            {
                Visible = !Visible;

                if (Visible)
                {
                    Kernel.ProcessManager.Start(this);
                }
                else
                {
                    Stop();
                }
            });

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
            if (Visible)
            {
                if (Kernel.Pressed)
                {
                    if (!HasWindowMoving && Window.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        BringToFront();
                    }

                    if (!HasWindowMoving && Window.Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Close.Action();

                        return;
                    }
                    else if (!HasWindowMoving && Window.Minimize.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Minimize.Action();

                        return;
                    }
                    else if (!HasWindowMoving && Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        HasWindowMoving = true;

                        pressed = true;
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

        private void BringToFront()
        {
            zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
            Kernel.WindowManager.MarkStackDirty();
        }

        public void DrawWindow()
        {
            Window.Update();
            UpdateApp();
        }
    }
}
