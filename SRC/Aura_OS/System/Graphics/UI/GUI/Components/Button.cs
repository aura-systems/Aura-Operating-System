/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Input;
using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Button : Component
    {
        public enum TextAlign
        {
            Center,
            Left
        }

        public string Text;
        public Bitmap Image;
        public Action Click;

        public Color BackColor = Color.LightGray;
        public Color TextColor = Color.Black;
        public bool NoBackground = false;
        public bool NoBorder = false;
        public bool Light = false;
        public bool Focused = false;
        public TextAlign TextAlignStyle { get; set; } = TextAlign.Center;

        public Button(string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Text = text;
        }

        public Button(Bitmap image, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Image = image;
        }

        public Button(Bitmap image, int x, int y) : base(x, y, (int)image.Width, (int)image.Height)
        {
            Image = image;
        }

        public Button(Bitmap image, string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Text = text;
            Image = image;
        }

        public Button(Bitmap image, string text, int x, int y) : base(x, y, (int)image.Width, (int)image.Height)
        {
            Text = text;
            Image = image;
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            if (Click != null)
            {
                if (IsInside((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y))
                {
                    Click();
                }
            }
        }

        public override void Draw()
        {
            if (!NoBorder && Light)
            {
                if (!NoBackground)
                {
                    Kernel.canvas.DrawFilledRectangle(BackColor, X + 2, Y + 2, Width - 3, Height - 3);
                }

                Kernel.canvas.DrawLine(Kernel.DarkGray, X, Y, X + Width, Y);
                Kernel.canvas.DrawLine(Kernel.DarkGray, X, Y, X, Y + Height);
                Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
                Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);
            }
            else if (!NoBorder && !Light)
            {
                if (Focused)
                {
                    if (!NoBackground)
                    {
                        Utils.DrawGradient(Kernel.Gray, Kernel.Pink, X + 2, Y + 2, Width - 3, Height - 3);
                    }

                    Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X + Width, Y);
                    Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
                    Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y, X, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
                    Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);
                }
                else
                {
                    if (!NoBackground)
                    {
                        Kernel.canvas.DrawFilledRectangle(BackColor, X + 2, Y + 2, Width - 3, Height - 3);
                    }

                    Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y, X + Width, Y);
                    Kernel.canvas.DrawLine(Kernel.Gray, X, Y + 1, X + Width, Y + 1);
                    Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y, X, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.Gray, X + 1, Y + 1, X + 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.DarkGray, X + 1, Y + Height - 1, X + Width, Y + Height - 1);
                    Kernel.canvas.DrawLine(Kernel.BlackColor, X, Y + Height, X + Width + 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.DarkGray, X + Width - 1, Y + 1, X + Width - 1, Y + Height);
                    Kernel.canvas.DrawLine(Kernel.BlackColor, X + Width, Y, X + Width, Y + Height);
                }
            }
            else
            {
                if (!NoBackground)
                {
                    Kernel.canvas.DrawFilledRectangle(BackColor, X + 2, Y + 2, Width - 3, Height - 3);
                }
            }

            if (Text != null && Image != null)
            {
                Kernel.canvas.DrawString(Text, Kernel.font, TextColor, X + 4 + (int)Image.Width + 4, Y + (Height / 2 - Kernel.font.Height / 2));

                int imageX = X + 4;
                int imageY = Y + (Height / 2 - (int)Image.Height / 2);
                Kernel.canvas.DrawImageAlpha(Image, imageX, imageY);
            }
            else if (Text != null)
            {
                int textX;
                int textY = Y + (Height / 2 - Kernel.font.Height / 2);

                if (TextAlignStyle == TextAlign.Center)
                {
                    int textWidth = Text.Length * Kernel.font.Width;
                    textX = X + (Width / 2) - (textWidth / 2);
                }
                else
                {
                    textX = X + 4;
                }

                Kernel.canvas.DrawString(Text, Kernel.font, TextColor, textX, textY);
            }
            else if (Image != null)
            {
                int imageX = X + (Width / 2 - (int)Image.Width / 2);
                int imageY = Y + (Height / 2 - (int)Image.Height / 2);
                Kernel.canvas.DrawImageAlpha(Image, imageX, imageY);
            }
        }
    }
}