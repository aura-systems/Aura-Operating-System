using Aura_OS.Processing;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.GUI
{
    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized
    }

    [Flags]
    public enum WindowResizeState
    {
        None = 0,
        Top = 1,
        Left = 2,
        Right = 4,
        Bottom = 8
    }
    public class WaveWindow : Process
    {
        public Bitmap Icon;

        public int windID;
        public string title;
        public bool Active = false;
        public int WindowX, WindowY, SavedX, SavedY, IX, IY;
        WindowState _state = WindowState.Normal;
        public WindowState savedState = WindowState.Normal;

        public bool StayOnTop = false;

        public WindowState State { get { return _state; } set { 
                if(value == WindowState.Maximized)
                {
                    if (_state == WindowState.Normal)
                    {
                        SavedX = WindowX; SavedY = WindowY;

                        WindowX = 0; WindowY = 0;

                        SavedWidth = Width; SavedHeight = Height;

                        Width = Aura_OS.Kernel.canvas.Mode.Columns; Height = (Aura_OS.Kernel.canvas.Mode.Rows - titleBarHeight - Host.restrictedTaskbarSize);

                        CloseButton.Y = 2;
                        MaximizeButton.Y = 2;
                        MinimizeButton.Y = 2;
                    }
                }else if(value == WindowState.Normal)
                {
                    if (_state == WindowState.Maximized)
                    {
                        WindowX = SavedX; WindowY = SavedY;

                        Width = SavedWidth; Height = SavedHeight;

                        CloseButton.Y = 5;
                        MaximizeButton.Y = 5;
                        MinimizeButton.Y = 5;
                    }
                }else if(value == WindowState.Minimized)
                {
                    savedState = _state;
                }
                _state = value;
            } }

        WindowResizeState ResizeState = WindowResizeState.None;

        public int Width, Height, SavedWidth, SavedHeight;
        public int MinWidth = 100, MinHeight = 100;

        public int titleBarHeight { get { if (!borderless) return 21; else return 0; } }

        public int resizeBorder = 10;
        public int borderSize = 3;

        public bool borderless = false;
        public bool controlbox = true;

        public bool Moving;
        public Canvas Canv { get { return Aura_OS.Kernel.canvas; } }

        public List<WaveElement> children = new List<WaveElement>();

        public WindowManager Host;

        WaveButton CloseButton;
        WaveButton MaximizeButton;
        WaveButton MinimizeButton;
        List<Pen> titleBarGradients;
        List<Pen> inactiveBarGradients;
        Pen Background;

        public static readonly Pen DeepBlue = new Pen(System.Drawing.Color.FromArgb(255, 51, 47, 208));
        public static readonly Pen Red = new Pen(System.Drawing.Color.FromArgb(255, 255, 0, 0));
        public static readonly Pen LightGray = new Pen(System.Drawing.Color.FromArgb(255, 125, 125, 125));
        public static readonly Pen DeepGray = new Pen(System.Drawing.Color.FromArgb(255, 25, 25, 25));

        public WaveWindow(string title, int x, int y, int width, int height, WindowManager host, Pen background) : base(title, ProcessType.Program)
        {
            Background = background;

            Icon = Aura_OS.Kernel.programIco;

            this.title = title;
            this.WindowX = x; this.WindowY = y;
            Width = width; Height = height;

            Host = host;

            CloseButton = new WaveButton() { Text = "X", parent = this, X = 21, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Aura_OS.Kernel.BlackPen,
                onClick = () => { Close(); }, 
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            MaximizeButton = new WaveButton() { Text = "o", parent = this, X = CloseButton.X + 18, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Aura_OS.Kernel.BlackPen,
                onClick = () => { if (State == WindowState.Normal) State = WindowState.Maximized; else State = WindowState.Normal; },
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            MinimizeButton = new WaveButton() { Text = "_", parent = this, X = MaximizeButton.X + 18, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Aura_OS.Kernel.BlackPen,
                onClick = () => { State = WindowState.Minimized; SetInActive(); },
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            titleBarGradients = GetGradients(DeepBlue, Red, 50);
            inactiveBarGradients = GetGradients(LightGray, DeepGray, 50);
        }

        public static List<Pen> GetGradients(Pen start, Pen end, int steps)
        {
            List<Pen> list = new List<Pen>();

            int stepA = ((end.Color.A - start.Color.A) / (steps - 1));
            int stepR = ((end.Color.R - start.Color.R) / (steps - 1));
            int stepG = ((end.Color.G - start.Color.G) / (steps - 1));
            int stepB = ((end.Color.B - start.Color.B) / (steps - 1));

            for (int i = 0; i < steps; i++)
            {
                list.Add( new Pen(System.Drawing.Color.FromArgb((byte)(start.Color.A + (stepA * i)),
                                            (byte)(start.Color.A + (stepR * i)),
                                            (byte)(start.Color.G + (stepG * i)),
                                            (byte)(start.Color.B + (stepB * i)))));
            }

            return list;
        }

        public virtual void UpdateWindow() { }

        public virtual void Draw()
        {
            if (State == WindowState.Minimized) return;

            //Draw window
            if (State == WindowState.Normal)
            {
                if (!borderless)
                {
                    Aura_OS.Kernel.GUI.Draw3DBorder(WindowX, WindowY, Width, Height + titleBarHeight);

                    ////Draw 3D border
                    ////Top and left white lines
                    //Canv.DrawFilledRectangle(X, Y, Width - 1, 1, 0, Color.White);
                    //Canv.DrawFilledRectangle(X, Y, 1, (Height + titleBarHeight) - 1, 0, Color.White);

                    ////Inner shadow Top
                    //Canv.DrawFilledRectangle(X + 1, Y + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                    //Canv.DrawFilledRectangle(X + 1, Y + 1, 1, (Height + titleBarHeight) - 1, 0, new Color(223, 223, 223));

                    ////Inner shadow Bottom
                    //Canv.DrawFilledRectangle(X + 1, (Y + Height + titleBarHeight) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                    //Canv.DrawFilledRectangle((X + Width) - 2, Y + 1, 1, (Height + titleBarHeight) - 1, 0, new Color(127, 127, 127));

                    ////Bottom and right black lines
                    //Canv.DrawFilledRectangle(X, (Y + Height + titleBarHeight) - 1, Width, 1, 0, Color.Black);
                    //Canv.DrawFilledRectangle((X + Width) - 1, Y, 1, Height + titleBarHeight, 0, Color.Black);
                }

                //Window bg
                if(!borderless)
                    Aura_OS.Kernel.GUI.DrawFilledRectangle(WindowX + 2, WindowY + 2, Width - 4, (Height + titleBarHeight) - 4, 0, Background);
                else
                    Aura_OS.Kernel.GUI.DrawFilledRectangle(WindowX, WindowY, Width, Height, 0, Background);

                if (!borderless)
                {
                    //Titlebar
                    //Gradient
                    for (int i = 0; i < Width - 6; i++)
                    {
                        float e = (float)i / (Width - 6);
                        e = e * titleBarGradients.Count;

                        Aura_OS.Kernel.GUI.DrawLine((WindowX + 3) + i, WindowY + 3, (WindowX + 3) + i, (WindowY + 3) + 18, Active ? titleBarGradients[(int)e] : inactiveBarGradients[(int)e]);
                    }
                }
            }
            else if(State == WindowState.Maximized)
            {
                //Window bg
                Aura_OS.Kernel.GUI.DrawFilledRectangle(WindowX, WindowY + 2, Width, (Height + titleBarHeight) - 2, 0, Background);

                if (!borderless)
                {
                    //Titlebar
                    //Gradient
                    for (int i = 0; i < Width; i++)
                    {
                        float e = (float)i / (Width);
                        e = e * titleBarGradients.Count;

                        Aura_OS.Kernel.GUI.DrawLine(WindowX + i, WindowY, WindowX + i, WindowY + 18, Active ? titleBarGradients[(int)e] : inactiveBarGradients[(int)e]);
                    }
                }
            }

            if(State == WindowState.Normal && !borderless)
            {
                Aura_OS.Kernel.canvas.DrawString(title, Aura_OS.Kernel.font, Aura_OS.Kernel.WhitePen, WindowX + 5, WindowY + 4);
            }
            else if(State == WindowState.Maximized && !borderless)
            {
                Aura_OS.Kernel.canvas.DrawString(title, Aura_OS.Kernel.font, Aura_OS.Kernel.WhitePen, WindowX + 5, WindowY + 2);
            }

            if (controlbox)
            {
                CloseButton.Draw();
                MaximizeButton.Draw();
                MinimizeButton.Draw();

                Aura_OS.Kernel.canvas.DrawImage(Icon, (int)(WindowX + 2), (int)(WindowY + 2));
            }

            //Draw children
            foreach (var item in children)
            {
                item.Draw();
            }
        }

        public virtual void Update()
        {
            bool activeHit = false;

            if (!WaveInput.MouseHit && WaveInput.WasLMBPressed() && WaveInput.IsMouseWithin(WindowX, WindowY, Width, Height + titleBarHeight))
            {
                activeHit = true;
                if (!Active)
                    SetActive();
            }

            if (controlbox)
            {
                UpdateElement(CloseButton);
                UpdateElement(MaximizeButton);
                UpdateElement(MinimizeButton);
            }

            //Resizing window
            if (!borderless && !WaveInput.MouseHit && State != WindowState.Maximized && WaveInput.WasLMBPressed() && !Moving && ResizeState == WindowResizeState.None)
            {
                if(WaveInput.IsMouseWithin(WindowX, WindowY, Width, resizeBorder))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Top;
                    IY = (int)Mouse.Y;
                }

                if(WaveInput.IsMouseWithin(WindowX, (WindowY + Height + titleBarHeight) - resizeBorder, Width, resizeBorder))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Bottom;
                    IY = (int)Mouse.Y;
                }

                if(WaveInput.IsMouseWithin(WindowX, WindowY, resizeBorder, Height + titleBarHeight))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Left;
                    IX = (int)Mouse.X;
                }

                if(WaveInput.IsMouseWithin((WindowX + Width) - resizeBorder, WindowY, resizeBorder, Height + titleBarHeight))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Right;
                    IX = (int)Mouse.X;
                }
            }

            if (WaveInput.mState == MouseState.Left && ResizeState != WindowResizeState.None)
            {

                if (WaveInput.mState == MouseState.Left && EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Top))
                {
                    if (Height - ((int)Mouse.Y - IY) > MinHeight)
                    {
                        Height -= (int)Mouse.Y - IY;
                        IY = (int)Mouse.Y;
                        WindowY = (int)Mouse.Y;
                    }
                }
                else if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Bottom))
                {
                    if (Height + ((int)Mouse.Y - IY) > MinHeight)
                    {
                        Height += (int)Mouse.Y - IY;
                        IY = (int)Mouse.Y;
                    }
                }

                if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Left))
                {
                    if (Width - ((int)Mouse.X - IX) > MinWidth)
                    {
                        Width -= (int)Mouse.X - IX;
                        IX = (int)Mouse.X;
                        WindowX = (int)Mouse.X;
                    }
                }
                else if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Right))
                {
                    if (Width + ((int)Mouse.X - IX) > MinWidth)
                    {
                        Width += (int)Mouse.X - IX;
                        IX = (int)Mouse.X;
                    }
                }
            }
            else
            {
                ResizeState = WindowResizeState.None;
            }

            //Moving window
            if (!borderless && State != WindowState.Maximized && !WaveInput.MouseHit && WaveInput.WasLMBPressed()
                && !Moving && ResizeState == WindowResizeState.None
                && WaveInput.IsMouseWithin(WindowX, WindowY, Width, titleBarHeight) 
                && !WaveInput.IsMouseWithin(CloseButton.relativeX, CloseButton.relativeY, CloseButton.Width, CloseButton.Height)
                && !WaveInput.IsMouseWithin(MaximizeButton.relativeX, MaximizeButton.relativeY, MaximizeButton.Width, MaximizeButton.Height)
                && !WaveInput.IsMouseWithin(MinimizeButton.relativeX, MinimizeButton.relativeY, MinimizeButton.Width, MinimizeButton.Height))
            {
                Moving = true;
                IX = (int)Mouse.X - WindowX;
                IY = (int)Mouse.Y - WindowY;
                WaveInput.MouseHit = true;
            }

            if(Mouse.MouseState == Cosmos.System.MouseState.Left && Moving)
            {
                WindowX = (int)Mouse.X - IX;
                WindowY = (int)Mouse.Y - IY;
            }
            else
            {
                Moving = false;
            }

            foreach (var item in children)
            {
                UpdateElement(item);
            }

            if (activeHit)
                WaveInput.MouseHit = true;

            UpdateWindow();
        }

        public void UpdateElement(WaveElement item)
        {
            item.Hovering = WaveInput.IsMouseWithin(item.relativeX, item.relativeY, item.Width, item.Height);

            if (item.Clicked)
            {
                if (WaveInput.mState != MouseState.Left)
                {
                    if (item.Hovering)
                    {
                        item.onClick?.Invoke();
                    }

                    item.Clicked = false;
                }
            }
            else
            {
                if(!WaveInput.MouseHit)
                item.Clicked = item.Hovering && WaveInput.WasLMBPressed();

                if (item.Clicked && item.HitTest)
                    WaveInput.MouseHit = true;
            }

            item.Update();
        }

        public void Close()
        {
            Host.CloseWindow(this);
        }

        public void SetActive()
        {
            Host.SetActiveWindow(this);
        }

        public void SetInActive()
        {
            Host.SetInActiveWindow(this);
        }
    }
}
