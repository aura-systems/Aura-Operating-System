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
using System.Collections.Generic;
using Aura_OS.System.Processing.Processes;
using Cosmos.System.Graphics;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Application : Process
    {
        public VirtualCanvas Canvas;

        public readonly int Width;
        public readonly int Height;

        public int X;
        public int Y;
        public Window Window;
        public bool Focused = false;
        public bool Visible = false;
        public bool Dirty = true;
        public int zIndex = 0;

        private int _px;
        private int _py;
        private bool _lck = false;
        private bool _pressed;
        private bool _hasWindowMoving = false;

        public Application(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            Canvas = new VirtualCanvas(new Mode((uint)width, (uint)height, ColorDepth.ColorDepth32));

            Window = new Window(name, x, y, width + 1, height + 1);
            Window.Close.Click = new Action(() =>
            {
                Stop();
                Explorer.WindowManager.Applications.Remove(this);
                Kernel.ProcessManager.Processes.Remove(this);
                Explorer.Taskbar.UpdateApplicationButtons();
            });
            Window.Minimize.Click = new Action(() =>
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

            Window.TopBar.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 2 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
            RightClickEntry entry = new("Close", 0, 0, Window.TopBar.RightClick.Width);
            entry.Click = new Action(() =>
            {
                Window.Close.Click();
            });
            rightClickEntries.Add(entry);
            RightClickEntry entry2 = new("Minimize", 0, 0, Window.TopBar.RightClick.Width);
            entry2.Click = new Action(() =>
            {
                Window.Minimize.Click();
            });
            rightClickEntries.Add(entry2);
            Window.TopBar.RightClick.Entries = rightClickEntries;

            X = x + 3;
            Y = y + Window.TopBar.Height + 3;
            Width = width - 4;
            Height = height - (Window.TopBar.Height + 4);
        }

        public virtual void HandleLeftClick()
        {
            if (Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Window.TopBar.HandleLeftClick();
            }
        }

        public virtual void HandleRightClick()
        {
            if (Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Window.TopBar.HandleRightClick();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public override void Update()
        {
            if (Visible)
            {
                Window.Update();

                if (Kernel.MouseManager.IsLeftButtonDown)
                {
                    if (!_hasWindowMoving && Window.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        BringToFront();
                    }

                    if (!_hasWindowMoving && Window.Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Close.Click();

                        return;
                    }
                    else if (!_hasWindowMoving && Window.Minimize.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Minimize.Click();

                        return;
                    }
                    else if (!_hasWindowMoving && Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        _hasWindowMoving = true;

                        _pressed = true;
                        if (!_lck)
                        {
                            _px = (int)MouseManager.X - Window.X;
                            _py = (int)MouseManager.Y - Window.Y;
                            _lck = true;
                        }
                    }
                }
                else
                {
                    _pressed = false;
                    _lck = false;
                    _hasWindowMoving = false;
                }

                if (_pressed)
                {
                    Window.X = (int)(MouseManager.X - _px);
                    Window.Y = (int)(MouseManager.Y - _py);

                    X = (int)(MouseManager.X - _px + 3);
                    Y = (int)(MouseManager.Y - _py + Window.TopBar.Height + 3);
                }
            }
        }

        public virtual void Draw()
        {
            Window.Draw();
        }

        public void Display()
        {
            if (Visible)
            {
                Canvas.Display();
            }
        }

        private void BringToFront()
        {
            zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
            Explorer.WindowManager.MarkStackDirty();
        }
    }
}
