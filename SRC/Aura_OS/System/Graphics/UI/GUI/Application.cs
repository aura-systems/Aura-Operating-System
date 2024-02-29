/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Base app class
* PROGRAMMERS:      nifanfa <nifanfa@foxmail.com>
                    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Cosmos.System;
using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Application : Process
    {
        public readonly int Width;
        public readonly int Height;

        public bool Focused
        {
            get => Explorer.WindowManager.FocusedApp == this;
        }

        public int X;
        public int Y;
        public Window Window;
        public bool ForceDirty = false;
        public bool Visible = false;
        public int zIndex = 0;

        private int _px;
        private int _py;
        private bool _lck = false;
        private bool _pressed;

        /// <summary>
        /// Does window needs an update.
        /// </summary>
        private bool _isDirty = true;

        /// <summary>
        /// Is base window buffer cached (only true after first draw or window size update).
        /// </summary>
        private bool _isCached = false;

        public Application(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            Window = new Window(name, x, y, width + 1, height + 1);
            Window.Close.Click = new Action(() =>
            {
                Window.Visible = false;
                Stop();
                Explorer.WindowManager.Applications.Remove(this);
                Kernel.ProcessManager.Processes.Remove(this);
                Explorer.Taskbar.UpdateApplicationButtons();
            });
            Window.Minimize.Click = new Action(() =>
            {
                if (Visible)
                {
                    MarkUnFocused();

                    Visible = false;
                    Window.Visible = false;

                    Stop();
                }
            });
            Window.Maximize.Click = new Action(() =>
            {
                if (!Visible)
                {
                    MarkFocused();

                    Visible = true;
                    Window.Visible = true;

                    Start();
                }
            });

            Window.TopBar.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 2 * RightClickEntry.ConstHeight);

            RightClickEntry entry = new("Close", Window.TopBar.RightClick.Width, Window.TopBar.RightClick);
            entry.Click = new Action(() =>
            {
                Window.Close.Click();
            });

            RightClickEntry entry2 = new("Minimize", Window.TopBar.RightClick.Width, Window.TopBar.RightClick);
            entry2.Click = new Action(() =>
            {
                Window.Minimize.Click();
            });

            Window.TopBar.RightClick.AddEntry(entry);
            Window.TopBar.RightClick.AddEntry(entry2);

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
            Window.Update();

            if (Visible)
            {
                if (Kernel.MouseManager.IsLeftButtonDown)
                {
                    if (!WindowManager.WindowMoving && Window.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        BringToFront();
                    }

                    if (!WindowManager.WindowMoving && Window.Close.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Close.Click();

                        return;
                    }
                    else if (!WindowManager.WindowMoving && Window.Minimize.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        Window.Minimize.Click();

                        return;
                    }
                    else if (!WindowManager.WindowMoving && Window.TopBar.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                    {
                        WindowManager.WindowMoving = true;

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
                    WindowManager.WindowMoving = false;

                    _pressed = false;
                    _lck = false;
                }

                if (_pressed)
                {
                    WindowManager.WindowMoving = true;

                    Window.X = (int)(MouseManager.X - _px);
                    Window.Y = (int)(MouseManager.Y - _py);

                    X = (int)(MouseManager.X - _px + 3);
                    Y = (int)(MouseManager.Y - _py + Window.TopBar.Height + 3);
                }
            }
        }

        public virtual void Draw()
        {
            if (_isCached)
            {
                Window.DrawCacheBuffer();
                Window.Close.DrawInParent();
                Window.Minimize.DrawInParent();
            }
            else
            {
                Window.Draw();
                Window.SaveCacheBuffer();
                _isCached = true;
            }
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        public virtual void MarkDirty()
        {
            _isDirty = true;
        }

        public void MarkCleaned()
        {
            _isDirty = false;
        }

        public void MarkUnFocused()
        {
            Explorer.WindowManager.FocusedApp = null;
        }

        public void MarkFocused()
        {
            Explorer.WindowManager.FocusedApp = this;
        }

        public void AddChild(Component component)
        {
            Window.AddChild(component);
        }

        private void BringToFront()
        {
            MarkFocused();
            Explorer.Taskbar.MarkDirty();
            Explorer.WindowManager.BringToFront(Window);
        }

        #region Drawing

        public void DrawLine(Color color, int xStart, int yStart, int width, int height)
        {
            Window.DrawLine(color, xStart + 4, yStart + Window.TopBar.Height + 6, width, height);
        }

        public void DrawString(string str, int x, int y)
        {
            Window.DrawString(str, x + 4, y + Window.TopBar.Height + 6);
        }

        public void DrawString(string str, Cosmos.System.Graphics.Fonts.Font font, Color color, int x, int y)
        {
            Window.DrawString(str, font, color, x + 4, y + Window.TopBar.Height + 6);
        }

        public void DrawImage(Cosmos.System.Graphics.Bitmap image, int x, int y)
        {
            Window.DrawImage(image, x + 4, y + Window.TopBar.Height + 6);
        }

        #endregion
    }
}
