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
        public int Width;
        public int Height;

        public bool Focused
        {
            get => Explorer.WindowManager.FocusedApp == this;
        }

        public Window Window;
        public bool ForceDirty = false;
        public bool Visible = false;
        public int zIndex = 0;

        private int _px;
        private int _py;
        private bool _lck = false;
        private bool _pressed;
        private bool _resizePressed;
        private bool _resizeWidthPressed = false;
        private bool _resizeHeightPressed = false;

        private bool _lckResize = false;
        private int _newX;
        private int _newY;

        /// <summary>
        /// Does window needs an update.
        /// </summary>
        private bool _isDirty = true;

        /// <summary>
        /// Is base window buffer cached (only true after first draw or window size update).
        /// </summary>
        private bool _isCached = false;

        // Resize regions
        private Rectangle _rectangleRight;
        private Rectangle _rectangleLeft;
        private Rectangle _rectangleTop;
        private Rectangle _rectangleBottom;

        public Application(string name, int width, int height, int x = 0, int y = 0) : base(name, ProcessType.Program)
        {
            InitWindow(name, width, height, x, y);
        }

        private void InitWindow(string name, int width, int height, int x = 0, int y = 0)
        {
            Window = new Window(name, x, y, width + 1, height + 1);
            _rectangleTop = new Rectangle(0, 0, 3, Window.Width + 1);
            _rectangleLeft = new Rectangle(0, 0, Window.Height + 1, 3);
            _rectangleBottom = new Rectangle(Window.Height + 1 - 3, 0, Window.Height + 1, Window.Width + 1);
            _rectangleRight = new Rectangle(0, Window.Width + 1 - 3, Window.Height + 1, Window.Width + 1);

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

        private int _firstX;
        private int _firstY;

        public override void Update()
        {
            Window.Update();

            if (Visible)
            {
                int clickX = (int)MouseManager.X - Window.X;
                int clickY = (int)MouseManager.Y - Window.Y;

                if (Kernel.MouseManager.IsLeftButtonDown)
                {
                    if (!_pressed)
                    {
                        if (_rectangleLeft.IsInside(clickX, clickY))
                        {
                            if (!_lckResize)
                            {
                                _firstX = (int)MouseManager.X;
                                _firstY = (int)MouseManager.Y;
                                _lckResize = true;
                            }
                            _resizeWidthPressed = true;
                            _resizePressed = true;
                        }
                        else if (_rectangleTop.IsInside(clickX, clickY))
                        {
                            if (!_lckResize)
                            {
                                _firstX = (int)MouseManager.X;
                                _firstY = (int)MouseManager.Y;
                                _lckResize = true;
                            }
                            _resizeHeightPressed = true;
                            _resizePressed = true;
                        }

                    }

                    if (!_resizePressed)
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
                }
                else
                {
                    if (_rectangleLeft.IsInside(clickX, clickY) || _resizePressed)
                    {
                        Input.MouseManager.CursorState = Input.CursorState.ResizeHorizontal;
                    }
                    else if (_rectangleTop.IsInside(clickX, clickY) || _resizePressed)
                    {
                        Input.MouseManager.CursorState = Input.CursorState.ResizeVertical;
                    }
                    else
                    {
                        Input.MouseManager.CursorState = Input.CursorState.Normal;
                    }

                    WindowManager.WindowMoving = false;

                    if (_resizePressed)
                    {
                        int currentX = (int)MouseManager.X;
                        int currentY = (int)MouseManager.Y;

                        if (_resizeWidthPressed)
                        {
                            _newX = _firstX - currentX;
                            int newWidth = Window.Width + _newX;
                            ResizeWindow(newWidth, Window.Height);
                            Window.X = currentX;
                        }

                        if (_resizeHeightPressed)
                        {
                            _newY = _firstY - currentY;
                            int newHeight = Window.Height + _newY;
                            ResizeWindow(Window.Width, newHeight);
                            Window.Resize(Window.Width, newHeight);
                            Window.Y = currentY;
                        }

                        Kernel.Debug = "_firstX=" + _firstX + ", _firstY=" + _firstY + ", _newX=" + _newX + ", _newY=" + _newY + ", newWidth=" + Window.Width + ", newHeight=" + Window.Height;
                        _resizePressed = false;
                        _lckResize = false;
                        _resizeWidthPressed = false;
                        _resizeHeightPressed = false;

                        MarkDirty();
                    }

                    _pressed = false;
                    _lck = false;
                }

                if (_pressed)
                {
                    WindowManager.WindowMoving = true;

                    Input.MouseManager.CursorState = Input.CursorState.Grab;

                    Window.X = (int)(MouseManager.X - _px);
                    Window.Y = (int)(MouseManager.Y - _py);
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

        public virtual void ResizeWindow(int width, int height)
        {
            Window.Resize(width, height);
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
