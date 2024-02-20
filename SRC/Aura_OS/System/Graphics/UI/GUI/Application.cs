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
using System.Drawing;
using Cosmos.System.Graphics.Fonts;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Application : Process
    {
        public readonly int Width;
        public readonly int Height;

        public bool ForceDirty
        {
            get => _forceDirty;
            set
            {
                _forceDirty = value;
                Window.ForceDirty = true;
            }
        }

        public int X;
        public int Y;
        public Window Window;
        public bool Focused = false;
        public bool Visible = false;
        public int zIndex = 0;

        private bool _forceDirty = false;
        private int _px;
        private int _py;
        private bool _lck = false;
        private bool _pressed;
        private bool _hasWindowMoving = false;

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
                    Visible = false;
                    Window.Visible = false;

                    Stop();
                }
            });
            Window.Maximize.Click = new Action(() =>
            {
                if (!Visible)
                {
                    Visible = true;
                    Window.Visible = true;

                    Start();
                }
            });

            Window.TopBar.RightClick = new RightClick((int)MouseManager.X, (int)MouseManager.Y, 200, 2 * RightClickEntry.ConstHeight);
            List<RightClickEntry> rightClickEntries = new List<RightClickEntry>();
            RightClickEntry entry = new("Close", 0, 0, Window.TopBar.RightClick.Width);
            entry.Click = new Action(() =>
            {
                Window.Close.Click();
            });
            entry.Visible = false;
            rightClickEntries.Add(entry);
            RightClickEntry entry2 = new("Minimize", 0, 0, Window.TopBar.RightClick.Width);
            entry2.Click = new Action(() =>
            {
                Window.Minimize.Click();
            });
            entry2.Visible = false;
            rightClickEntries.Add(entry2);
            Window.TopBar.RightClick.Entries = rightClickEntries;
            Window.TopBar.RightClick.Visible = false;

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
            return;

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

                    Explorer.Desktop.MarkDirty();
                }
            }
        }

        public virtual void Draw()
        {
            if (_isCached)
            {
                Window.DrawCacheBuffer();
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

        public void AddChild(Component component)
        {
            Window.AddChild(component);
        }

        private void BringToFront()
        {
            Explorer.WindowManager.BringToFront(Window);
            //zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
            //Explorer.WindowManager.MarkStackDirty();
        }

        public void DrawLine(Color color, int xStart, int yStart, int width, int height)
        {
            Window.DrawLine(color, xStart + 5, yStart + Window.TopBar.Height + 5, width, height);
        }

        public void DrawString(string str, int x, int y)
        {
            Window.DrawString(str, x + 5, y + Window.TopBar.Height + 5);
        }

        public void DrawString(string str, Cosmos.System.Graphics.Fonts.Font font, Color color, int x, int y)
        {
            Window.DrawString(str, font, color, x + 5, y + Window.TopBar.Height + 5);
        }

        public void DrawImage(Cosmos.System.Graphics.Bitmap image, int x, int y)
        {
            Window.DrawImage(image, x + 5, y + Window.TopBar.Height + 5);
        }
    }
}
