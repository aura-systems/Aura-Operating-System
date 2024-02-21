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
            if (Text != null && Image != null)
            {
                if (!NoBackground)
                {
                    base.Draw();
                }

                DrawString(Text, Kernel.font, TextColor, 4 + (int)Image.Width + 4, (Height / 2 - Kernel.font.Height / 2));

                int imageX = 4;
                int imageY = Height / 2 - (int)Image.Height / 2;
                DrawImageAlpha(Image, imageX, imageY);
            }
            else if (Text != null)
            {
                if (!NoBackground)
                {
                    base.Draw();
                }

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
            else if (Image != null)
            {
                int imageX = 0 + (Width / 2 - (int)Image.Width / 2);
                int imageY = 0 + (Height / 2 - (int)Image.Height / 2);
                DrawImageAlpha(Image, imageX, imageY);
            }
        }
    }
}