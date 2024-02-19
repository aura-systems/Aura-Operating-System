/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

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
        public TextAlign TextAlignStyle = TextAlign.Center;

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
            if (Frame != null)
            {
                base.Draw();

                if (Text != null && Image != null)
                {
                    DrawString(Text, Kernel.font, TextColor, 4 + (int)Image.Width + 4, (Height / 2 - Kernel.font.Height / 2));

                    int imageX = 4;
                    int imageY = Height / 2 - (int)Image.Height / 2;
                    DrawImageAlpha(Image, imageX, imageY);
                }
                else if (Text != null)
                {
                    int textX;
                    int textY = (Height / 2 - Kernel.font.Height / 2);

                    if (TextAlignStyle == TextAlign.Center)
                    {
                        int textWidth = Text.Length * Kernel.font.Width;
                        textX = (Width / 2) - (textWidth / 2);
                    }
                    else
                    {
                        textX = 4;
                    }

                    DrawString(Text, Kernel.font, TextColor, textX, textY);
                }

                return;
            }

            if (!NoBorder && Light)
            {
                if (!NoBackground)
                {
                    Clear(BackColor);
                }

                DrawLine(Kernel.DarkGray, 0, 0, 0 + Width, 0);
                DrawLine(Kernel.DarkGray, 0, 0, 0, 0 + Height);
                DrawLine(Kernel.WhiteColor, 0, 0 + Height, 0 + Width + 1, 0 + Height);
                DrawLine(Kernel.WhiteColor, 0 + Width, 0, 0 + Width, 0 + Height);
            }
            else if (!NoBorder && !Light)
            {
                if (Focused)
                {
                    if (!NoBackground)
                    {
                        DrawGradient(Kernel.Gray, Kernel.Pink, 0 + 2, 0 + 2, Width - 3, Height - 3);
                    }

                    DrawLine(Kernel.BlackColor, 0, 0, 0 + Width, 0);
                    DrawLine(Kernel.Gray, 0, 0 + 1, 0 + Width, 0 + 1);
                    DrawLine(Kernel.BlackColor, 0, 0, 0, 0 + Height);
                    DrawLine(Kernel.Gray, 0 + 1, 0 + 1, 0 + 1, 0 + Height);
                    DrawLine(Kernel.DarkGray, 0 + 1, 0 + Height - 1, 0 + Width, 0 + Height - 1);
                    DrawLine(Kernel.WhiteColor, 0, 0 + Height, 0 + Width + 1, 0 + Height);
                    DrawLine(Kernel.DarkGray, 0 + Width - 1, 0 + 1, 0 + Width - 1, 0 + Height);
                    DrawLine(Kernel.WhiteColor, 0 + Width, 0, 0 + Width, 0 + Height);
                }
                else
                {
                    if (!NoBackground)
                    {
                        Clear(BackColor);
                        // DrawFilledRectangle(BackColor, 0 + 2, 0 + 2, Width - 3, Height - 3);
                    }

                    DrawLine(Kernel.WhiteColor, 0, 0, 0 + Width, 0);
                    DrawLine(Kernel.Gray, 0, 0 + 1, 0 + Width, 0 + 1);
                    DrawLine(Kernel.WhiteColor, 0, 0, 0, 0 + Height);
                    DrawLine(Kernel.Gray, 0 + 1, 0 + 1, 0 + 1, 0 + Height);
                    DrawLine(Kernel.DarkGray, 0 + 1, 0 + Height - 1, 0 + Width, 0 + Height - 1);
                    DrawLine(Kernel.BlackColor, 0, 0 + Height, 0 + Width + 1, 0 + Height);
                    DrawLine(Kernel.DarkGray, 0 + Width - 1, 0 + 1, 0 + Width - 1, 0 + Height);
                    DrawLine(Kernel.BlackColor, 0 + Width, 0, 0 + Width, 0 + Height);
                }
            }
            else
            {
                if (!NoBackground)
                {
                    Clear(BackColor);
                }
            }

            if (Text != null && Image != null)
            {
                DrawString(Text, Kernel.font, TextColor, 0 + 4 + (int)Image.Width + 4, 0 + (Height / 2 - Kernel.font.Height / 2));

                int imageX = 0 + 4;
                int imageY = 0 + (Height / 2 - (int)Image.Height / 2);
                DrawImageAlpha(Image, imageX, imageY);
            }
            else if (Text != null)
            {
                int textX;
                int textY = 0 + (Height / 2 - Kernel.font.Height / 2);

                if (TextAlignStyle == TextAlign.Center)
                {
                    int textWidth = Text.Length * Kernel.font.Width;
                    textX = 0 + (Width / 2) - (textWidth / 2);
                }
                else
                {
                    textX = 0 + 4;
                }

                DrawString(Text, Kernel.font, TextColor, textX, textY);
            }
            else if (Image != null)
            {
                int imageX = 0 + (Width / 2 - (int)Image.Width / 2);
                int imageY = 0 + (Height / 2 - (int)Image.Height / 2);
                DrawImageAlpha(Image, imageX, imageY);
            }
        }
    }
}