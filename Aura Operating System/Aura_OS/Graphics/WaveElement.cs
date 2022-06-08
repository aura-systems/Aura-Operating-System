using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.GUI
{
    [Flags]
    public enum AnchorStyles
    {
        Bottom = 2,
        Left = 4,
        None = 0,
        Right = 8,
        Top = 1
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public class WaveElement
    {
        public WaveWindow parent;
        public WaveElement parent2;
        public AnchorStyles Anchor = AnchorStyles.Left | AnchorStyles.Top;

        public WaveContextMenu contextMenu;

        public bool ignoreTitleBar = false;

        public int X = 0, Y = 0, Width = 0, Height = 0;
        public object tag;
        public int relativeX { get 
            {
                if (EnumHelper.IsAnchorSet(Anchor, AnchorStyles.Left))
                    if (parent2 == null) //3 is the border size, make border size customizable in the future?
                        return (parent.WindowX + X) + 3;
                    else
                        return parent2.relativeX + X;
                else
                    if(parent2 == null)
                        return (parent.WindowX + parent.Width) - X;
                    else
                        return (parent2.relativeX + parent2.Width) - X;
            }
        }
        public int relativeY { get 
            {
                if (EnumHelper.IsAnchorSet(Anchor, AnchorStyles.Top))
                    if(parent2 == null)
                        return (!ignoreTitleBar) ? (parent.WindowY + ((!parent.borderless) ? parent.titleBarHeight : 0)) + Y : parent.WindowY + Y;
                    else
                        return (parent2.relativeY) + Y;
                else
                    if(parent2 == null)
                        return (parent.WindowY + parent.Height) - Y;
                    else
                        return (parent2.relativeY + parent2.Height) - Y;
            }
        }

        public bool IsFlagSet<T>(T value, T flag) where T : struct
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public bool Clicked, Hovering;
        public bool HitTest = true;
        public Canvas Canv { get { return Aura_OS.Kernel.canvas; } }

        public Action onClick;

        public virtual void Update()
        {
            //foreach (var item in children)
            //{
            //    item.Update();
            //} 
        }
        public virtual void Draw() 
        {
            //foreach (var item in children)
            //{
            //    item.Draw();
            //}
        }
    }

    public class WaveContextMenu : WaveStackPanel
    {
        public WaveContextMenu() { }
        public WaveContextMenu(List<StartMenuItem> items)
        {
            foreach (var item in items)
            {
                item.parent2 = this;
                children.Add(item);
            }

            Height = Height = (children.Count * 20) + 6;
            UpdateView();
        }


    }

    public class WavePanel : WaveElement
    {
        public Pen pen = new Pen(System.Drawing.Color.FromArgb(0, 0, 128));
        public Pen pen2 = new Pen(System.Drawing.Color.FromArgb(191, 191, 191));
        public Pen pen3 = new Pen(System.Drawing.Color.FromArgb(127, 127, 127));
        public Pen pen4 = new Pen(System.Drawing.Color.FromArgb(223, 223, 223));

        public List<WaveElement> children = new List<WaveElement>();
        public bool DrawBorder = true;

        public WavePanel()
        {
            HitTest = false;
        }
        public override void Update()
        {
            foreach (var item in children)
            {
                UpdateElement(item);
            }
        }
        public override void Draw()
        {
            Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, pen2);

            if (DrawBorder)
            {
                //Draw 3D border
                //Top and left white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, (Height) - 1, 0, Aura_OS.Kernel.WhitePen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, (Height) - 1, 0, pen4);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, (Height) - 1, 0, pen3);

                //Bottom and right black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.BlackPen);
            }

            foreach (var item in children)
            {
                item.Draw();
            }
        }

        public void UpdateElement(WaveElement item)
        {
            item.Hovering = IsMouseWithin(item.relativeX, item.relativeY, item.Width, item.Height);
            //item.Clicked = item.Hovering && Mouse.MouseState == MouseState.Left;

            if (item.Clicked)
            {
                if (WaveInput.mState != MouseState.Left)
                {
                    //Trigger mouse click on element
                    if (item.Hovering)
                    {
                        item.onClick?.Invoke();
                    }

                    item.Clicked = false;
                }
                //item.clicked = Mouse.MouseState == MouseState.Left
            }
            else
            {
                if (!WaveInput.MouseHit)
                    item.Clicked = item.Hovering && WaveInput.WasLMBPressed();

                if (item.Clicked && item.HitTest)
                    WaveInput.MouseHit = true;
            }

            item.Update();
        }

        public static bool IsMouseWithin(int X, int Y, int Width, int Height)
        {
            return Mouse.X >= X && Mouse.Y >= Y && Mouse.X <= X + Width && Mouse.Y <= Y + Height;
        }
    }

    public class WaveStackPanel : WavePanel
    {
        public void UpdateView()
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (i > 0)
                    children[i].Y = children[i - 1].Y + children[i - 1].Height;
                else
                    children[i].Y = 0;
            }
        }
    }

    public class WaveLabel : WaveElement
    {
        public string Text = "";
        public Pen Color = Aura_OS.Kernel.WhitePen;

        public override void Draw()
        {
            if (parent2 == null)
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color, relativeX, relativeY);
            //else
            //Canv.DrawString(relativeX, relativeY, parent2, Text, Color);
        }
    }

    public class WaveButton : WaveElement
    {
        public string Text = "";
        public Pen Color = Aura_OS.Kernel.WhitePen;
        public TextAlignment TextAlignment = TextAlignment.Center;
        public bool forcePressed = false;

        public Pen pen = new Pen(System.Drawing.Color.FromArgb(0, 0, 128));
        public Pen pen2 = new Pen(System.Drawing.Color.FromArgb(191, 191, 191));
        public Pen pen3 = new Pen(System.Drawing.Color.FromArgb(127, 127, 127));
        public Pen pen4 = new Pen(System.Drawing.Color.FromArgb(223, 223, 223));

        public override void Draw()
        {
            if (forcePressed)
            {
                //Top and left black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Aura_OS.Kernel.BlackPen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, pen3);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, pen4);

                //Bottom and right white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.WhitePen);

                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, pen2);


                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(20, 20, 20));
            }
            else if (Hovering && !Clicked) //Hover
            {
                //Top and left white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Aura_OS.Kernel.WhitePen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, pen4);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, pen3);

                //Bottom and right black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.BlackPen);

                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, pen2);

                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(54, 54, 54));
            }
            else if (!Clicked) //Normal
            {
                //Top and left white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Aura_OS.Kernel.WhitePen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, pen4);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, pen3);

                //Bottom and right black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.BlackPen);

                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, pen2);

                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(36, 36, 36));
            }
            else if (Clicked && Hovering) //Clicked and Hovering
            {
                //Top and left black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Aura_OS.Kernel.BlackPen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, pen3);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, pen4);

                //Bottom and right white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.WhitePen);

                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, pen2);


                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(20, 20, 20));
            }
            else if (Clicked && !Hovering)
            {
                //Top and left white lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Aura_OS.Kernel.WhitePen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Aura_OS.Kernel.WhitePen);

                //Inner shadow Top
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, pen4);
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, pen4);

                //Inner shadow Bottom
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, pen3);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, pen3);

                //Bottom and right black lines
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Aura_OS.Kernel.BlackPen);
                Aura_OS.Kernel.GUI.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Aura_OS.Kernel.BlackPen);

                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, pen2);
            }
            if (TextAlignment == TextAlignment.Left)
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color, relativeX + 3, relativeY + (Height / 2) - 7 - 2);
            else if (TextAlignment == TextAlignment.Center)
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color, (relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7 - 2);
            else if (TextAlignment == TextAlignment.Right) //Not implemented, uses Center setting
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color, (relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7 - 2);
        }
    }

    public class StartMenuItem : WaveElement
    {
        public string Text = "";
        //public Color Color = Aura_OS.Kernel.WhitePen;
        public TextAlignment TextAlignment = TextAlignment.Center;
        public bool forcePressed = false;

        WaveContextMenu itemList;

        public StartMenuItem(int Width, List<StartMenuItem> items = null)
        {
            this.Width = Width;

            if (items != null)
            {
                itemList = new WaveContextMenu(items)
                {
                    X = Width,
                    Y = 0,
                    parent = parent,
                    parent2 = this,
                    Width = 166
                };
            }

            if (items != null)
            {
                //itemList.UpdateView();
                //itemList.Height = Height = (itemList.children.Count * 20) + 6;
            }
        }

        //public void UpdateView()
        //{
        //    itemList.Height = (Items.Count * 20) + 6;

        //    if(Items.Count > 0)
        //    {
        //        itemList.Update();
        //    }
        //}

        public Pen pen = new Pen(System.Drawing.Color.FromArgb(0, 0, 128));
        public Pen pen2 = new Pen(System.Drawing.Color.FromArgb(0, 0, 128));

        public override void Draw()
        {
            Pen Color = Aura_OS.Kernel.BlackPen;

            if (Hovering && !Clicked) //Hover
            {
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, pen);
                Color = Aura_OS.Kernel.WhitePen;
            }
            else if (!Clicked) //Normal
            {
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, pen2);
            }
            else if (Clicked && Hovering) //Clicked and Hovering
            {
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, pen);
            }
            else if (Clicked && !Hovering)
            {
                Aura_OS.Kernel.GUI.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, pen);
            }
            if (TextAlignment == TextAlignment.Left)
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color, relativeX + 3, relativeY + (Height / 2) - 7);
            else if (TextAlignment == TextAlignment.Center)
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color,  (relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7);
            else if (TextAlignment == TextAlignment.Right) //Not implemented, uses Center setting
                Aura_OS.Kernel.canvas.DrawString(Text, Aura_OS.Kernel.font, Color,  (relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7);

            if(itemList.children.Count > 0)
            {
                itemList.Draw();
            }
        }

        public override void Update()
        {
            if(itemList.children.Count > 0)
            {
                itemList.Update();
            }
        }
    }
}
