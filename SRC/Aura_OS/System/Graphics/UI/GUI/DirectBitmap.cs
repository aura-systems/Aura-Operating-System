﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Direct bitmap (used for compositing)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public unsafe class DirectBitmap
    {
        /// <summary>
        /// Stride.
        /// </summary>
        internal int Stride;

        /// <summary>
        /// Pitch.
        /// </summary>
        internal int Pitch;

        public Bitmap Bitmap { get; private set; }
        public int Height = 144;
        public int Width = 160;

        public DirectBitmap()
        {
            Bitmap = new Bitmap((uint)Width, (uint)Height, ColorDepth.ColorDepth32);
            Stride = (int)32 / 8;
            Pitch = (int)Width * Stride;
        }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bitmap = new Bitmap((uint)Width, (uint)Height, ColorDepth.ColorDepth32);
            Stride = (int)32 / 8;
            Pitch = (int)Width * Stride;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, int colour)
        {
            int index = x + y * Width;
            Bitmap.RawData[index] = colour | (0xFF << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixelAlpha(int x, int y, int colour)
        {
            int index = x + y * Width;

            if (index < Bitmap.RawData.Length)
            {
                if ((colour >> 24) == 0xFF)
                {
                    Bitmap.RawData[index] = colour;
                    return;
                }

                int bgColour = Bitmap.RawData[index];
                int alpha = (colour >> 24) & 0xff;
                int invAlpha = 255 - alpha;
                int newRed = (((colour >> 16) & 0xff) * alpha + ((bgColour >> 16) & 0xff) * invAlpha) >> 8;
                int newGreen = (((colour >> 8) & 0xff) * alpha + ((bgColour >> 8) & 0xff) * invAlpha) >> 8;
                int newBlue = ((colour & 0xff) * alpha + (bgColour & 0xff) * invAlpha) >> 8;

                Bitmap.RawData[index] = (alpha << 24) | (newRed << 16) | (newGreen << 8) | newBlue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPixel(int x, int y)
        {
            int index = x + y * Width;
            return Bitmap.RawData[index];
        }

        public void Clear(int colour)
        {
            fixed (int* destPtr = Bitmap.RawData)
            {
                MemoryOperations.Fill(destPtr, colour, Bitmap.RawData.Length);
            }
        }

        public void DrawString(string str, Font font, int color, int x, int y)
        {
            int length = str.Length;
            byte width = font.Width;
            for (int i = 0; i < length; i++)
            {
                DrawChar(str[i], font, color, x, y);
                x += width;
            }
        }

        public void DrawChar(char c, Font font, int color, int x, int y)
        {
            byte height = font.Height;
            byte width = font.Width;
            byte[] data = font.Data;
            int num = height * (byte)c;
            for (int i = 0; i < height; i++)
            {
                for (byte b = 0; b < width; b = (byte)(b + 1))
                {
                    if (font.ConvertByteToBitAddress(data[num + i], b + 1))
                    {
                        SetPixelAlpha((ushort)(x + b), (ushort)(y + i), color);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public virtual void DrawRectangle(int color, int x, int y, int width, int height)
        {
            /*
             * we must draw four lines connecting any vertex of our rectangle to do this we first obtain the position of these
             * vertex (we call these vertexes A, B, C, D as for geometric convention)
             */

            /* The check of the validity of x and y are done in DrawLine() */

            /* The vertex A is where x,y are */
            int xa = x;
            int ya = y;

            /* The vertex B has the same y coordinate of A but x is moved of width pixels */
            int xb = x + width;
            int yb = y;

            /* The vertex C has the same x coordiate of A but this time is y that is moved of height pixels */
            int xc = x;
            int yc = y + height;

            /* The Vertex D has x moved of width pixels and y moved of height pixels */
            int xd = x + width;
            int yd = y + height;

            /* Draw a line betwen A and B */
            DrawLine(color, xa, ya, xb, yb);

            /* Draw a line between A and C */
            DrawLine(color, xa, ya, xc, yc);

            /* Draw a line between B and D */
            DrawLine(color, xb, yb, xd, yd);

            /* Draw a line between C and D */
            DrawLine(color, xc, yc, xd, yd);
        }

        public void DrawFilledRectangle(int color, int xStart, int yStart, int width, int height)
        {
            if (height == -1)
            {
                height = width;
            }

            for (int i = yStart; i < yStart + height; i++)
            {
                DrawLine(color, xStart, i, xStart + width - 1, i);
            }
        }

        public void DrawLine(int color, int x1, int y1, int x2, int y2)
        {
            TrimLine(ref x1, ref y1, ref x2, ref y2);
            int num = x2 - x1;
            int num2 = y2 - y1;
            if (num2 == 0)
            {
                DrawHorizontalLine(color, num, x1, y1);
            }
            else if (num == 0)
            {
                DrawVerticalLine(color, num2, x1, y1);
            }
            else
            {
                DrawDiagonalLine(color, num, num2, x1, y1);
            }
        }

        internal void DrawDiagonalLine(int color, int dx, int dy, int x1, int y1)
        {
            int num = Math.Abs(dx);
            int num2 = Math.Abs(dy);
            int num3 = Math.Sign(dx);
            int num4 = Math.Sign(dy);
            int num5 = num2 >> 1;
            int num6 = num >> 1;
            int num7 = x1;
            int num8 = y1;
            if (num >= num2)
            {
                for (int i = 0; i < num; i++)
                {
                    num6 += num2;
                    if (num6 >= num)
                    {
                        num6 -= num;
                        num8 += num4;
                    }

                    num7 += num3;
                    SetPixelAlpha(num7, num8, color);
                }

                return;
            }

            for (int i = 0; i < num2; i++)
            {
                num5 += num;
                if (num5 >= num2)
                {
                    num5 -= num2;
                    num7 += num3;
                }

                num8 += num4;
                SetPixelAlpha(num7, num8, color);
            }
        }

        internal void DrawVerticalLine(int color, int dy, int x1, int y1)
        {
            for (int i = 0; i < dy; i++)
            {
                SetPixelAlpha(x1, y1 + i, color);
            }
        }

        internal void DrawHorizontalLine(int color, int dx, int x1, int y1)
        {
            for (int i = 0; i < dx; i++)
            {
                SetPixelAlpha(x1 + i, y1, color);
            }
        }

        protected void TrimLine(ref int x1, ref int y1, ref int x2, ref int y2)
        {
            if (x1 == x2)
            {
                x1 = Math.Min((int)(Width - 1), Math.Max(0, x1));
                x2 = x1;
                y1 = Math.Min((int)(Height - 1), Math.Max(0, y1));
                y2 = Math.Min((int)(Height - 1), Math.Max(0, y2));
                return;
            }

            float num = x1;
            float num2 = y1;
            float num3 = x2;
            float num4 = y2;
            float num5 = (num4 - num2) / (num3 - num);
            float num6 = num2 - num5 * num;
            if (num < 0f)
            {
                num = 0f;
                num2 = num6;
            }
            else if (num >= (float)Width)
            {
                num = Width - 1;
                num2 = (float)(Width - 1) * num5 + num6;
            }

            if (num3 < 0f)
            {
                num3 = 0f;
                num4 = num6;
            }
            else if (num3 >= (float)Width)
            {
                num3 = Width - 1;
                num4 = (float)(Width - 1) * num5 + num6;
            }

            if (num2 < 0f)
            {
                num = (0f - num6) / num5;
                num2 = 0f;
            }
            else if (num2 >= (float)Height)
            {
                num = ((float)(Height - 1) - num6) / num5;
                num2 = Height - 1;
            }

            if (num4 < 0f)
            {
                num3 = (0f - num6) / num5;
                num4 = 0f;
            }
            else if (num4 >= (float)Height)
            {
                num3 = ((float)(Height - 1) - num6) / num5;
                num4 = Height - 1;
            }

            if (num < 0f || num >= (float)Width || num2 < 0f || num2 >= (float)Height)
            {
                num = 0f;
                num3 = 0f;
                num2 = 0f;
                num4 = 0f;
            }

            if (num3 < 0f || num3 >= (float)Width || num4 < 0f || num4 >= (float)Height)
            {
                num = 0f;
                num3 = 0f;
                num2 = 0f;
                num4 = 0f;
            }

            x1 = (int)num;
            y1 = (int)num2;
            x2 = (int)num3;
            y2 = (int)num4;
        }

        public void DrawImage(Image image, int x, int y)
        {
            for (int yi = 0; yi < image.Height; yi++)
            {
                int destOffset = ((y + yi) * (int)Bitmap.Width + x);
                int srcOffset = yi * (int)image.Width;
                int count = (int)image.Width;

                MemoryOperations.Copy(Bitmap.RawData, destOffset, image.RawData, srcOffset, count);
            }
        }

        public Bitmap ExtractImage(int srcX, int srcY, int width, int height)
        {
            Bitmap bmp = new((uint)width, (uint)height, ColorDepth.ColorDepth32);

            for (int yi = 0; yi < height; yi++)
            {
                int destOffset = yi * width;
                int srcOffset = ((srcY + yi) * (int)Bitmap.Width + srcX);
                int count = width;

                MemoryOperations.Copy(bmp.RawData, destOffset, Bitmap.RawData, srcOffset, count);
            }

            return bmp;
        }

        public static void AlphaBlendSSE(uint *dest, int dbpl, uint* src, int sbpl, int width, int height)
        {
            // PLUGGED
        }

        public static void AlphaBltSSE2(byte* dst, byte* src, int w, int h, int wmul4)
        {
        }

        public static void OpacitySSE(uint* pixelPtr, int w, int h, int bpl, uint a)
        {
            // PLUGGED
        }

        public void DrawImageAlpha(Image image, int x, int y, byte alpha = 0xFF)
        {
            if (image.RawData.Length > Bitmap.RawData.Length)
            {
                return;
            }

            if ((y + image.Height > Bitmap.Height || y < 0) && (x + image.Width < Bitmap.Width || x > 0))
            {
                return;
            }

            Bitmap tmp = ExtractImage(x, y, (int)image.Width, (int)image.Height);
            if (tmp.Width == 0) return;

            fixed (int* bgBitmap = tmp.RawData)
            fixed (int* fgBitmap = image.RawData)
            {
                if (alpha < 0xFF)
                {
                    OpacitySSE((uint*)fgBitmap, (int)image.Width, (int)image.Height, (int)image.Width * 4, alpha);
                }

                // AlphaBltSSE2((byte*)bgBitmap, (byte*)fgBitmap, w, (int)image.Height, wmul4);
                AlphaBlendSSE((uint*)bgBitmap, (int)image.Width * 4, (uint*)fgBitmap, (int)image.Width * 4, (int)image.Width, (int)image.Height);
            }

            DrawImage(tmp, x, y);
        }

        public void DrawImageStretchAlpha(Bitmap image, Rectangle sourceRect, Rectangle destRect)
        {
            float scaleX = (float)sourceRect.Width / destRect.Width;
            float scaleY = (float)sourceRect.Height / destRect.Height;

            for (int xi = 0; xi < destRect.Width; xi++)
            {
                for (int yi = 0; yi < destRect.Height; yi++)
                {
                    int srcX = (int)(xi * scaleX) + sourceRect.Left;
                    int srcY = (int)(yi * scaleY) + sourceRect.Top;

                    srcX = Math.Min(srcX, sourceRect.Right - 1);
                    srcY = Math.Min(srcY, sourceRect.Bottom - 1);

                    int color = image.RawData[srcX + srcY * image.Width];
                    SetPixelAlpha(destRect.Left + xi, destRect.Top + yi, color);
                }
            }
        }
    }
}