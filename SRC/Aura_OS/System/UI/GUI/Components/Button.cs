/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;

namespace Aura_OS.System.UI.GUI.Components
{
    public class Button : Component
    {
        public string Text;
        public Bitmap Image;
        public bool NoBorder = false;
        public bool Light = false;
        public bool Focused = false;

        public Button(string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Text = text;
        }

        public Button(string text, int x, int y, int width, int height, bool light = false) : base(x, y, width, height)
        {
            Text = text;
            Light = light;
        }

        public Button(Bitmap image, int x, int y, int width, int height, bool noBorder = false) : base(x, y, width, height)
        {
            Image = image;
            NoBorder = noBorder;
        }

        public Button(Bitmap image, int x, int y, bool noBorder = false) : base(x, y, (int)image.Width, (int)image.Height)
        {
            Image = image;
            NoBorder = noBorder;
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

        public override void Update()
        {
            if (!NoBorder && Light)
            {
                Kernel.canvas.DrawFilledRectangle(Kernel.Gray, X + 2, Y + 2, Width - 3, Height - 3);

                Kernel.canvas.DrawLine(Kernel.DarkGray, X, Y, X + Width, Y);
                Kernel.canvas.DrawLine(Kernel.DarkGray, X, Y, X, Y + Height);
                Kernel.canvas.DrawLine(Kernel.WhiteColor, X, Y + Height, X + Width + 1, Y + Height);
                Kernel.canvas.DrawLine(Kernel.WhiteColor, X + Width, Y, X + Width, Y + Height);
            }
            else if (!NoBorder && !Light)
            {
                if (Focused)
                {
                    Utils.DrawGradient(Kernel.Gray, Kernel.Pink, X + 2, Y + 2, Width - 3, Height - 3);

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
                    Kernel.canvas.DrawFilledRectangle(Kernel.DarkGrayLight, X + 2, Y + 2, Width - 3, Height - 3);

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

            if (Text != null && Image != null)
            {
                Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4 + (int)Image.Width + 4, Y + (Height / 2 - Kernel.font.Height / 2));

                int imageX = X + 4;
                int imageY = Y + (Height / 2 - (int)Image.Height / 2);
                Kernel.canvas.DrawImageAlpha(Image, imageX, imageY);
            }
            else if (Text != null)
            {
                Kernel.canvas.DrawString(Text, Kernel.font, Kernel.BlackColor, X + 4, Y + (Height / 2 - Kernel.font.Height / 2));
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