/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Direct bitmap (used for compositing)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Aura_OS.System.Graphics.UI.GUI
{
    /// <summary>
    /// Aura's compositor surface: raw ARGB ints, either owned by a Bitmap or aliasing the canvas
    /// back buffer. Every operation clips to the surface, never throws for off-screen coordinates
    /// (windows and the cursor go off-screen) and never allocates (except ExtractImage).
    /// GEN3-GAP(blit): Canvas.DrawImageAlpha is per-pixel and forces opaque output, and the BCL
    /// replaces the gen2 kernel memory helpers, so blending and copies are done here in plain C#.
    /// </summary>
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

        /// <summary>
        /// Raw ARGB pixels, row-major, Width * Height. This is Bitmap.RawData (same array, no copy),
        /// or the canvas back buffer for a canvas-backed DirectBitmap.
        /// </summary>
        public readonly int[] Pixels;

        /// <summary>
        /// Backing bitmap. Null only when this DirectBitmap aliases the canvas back buffer.
        /// </summary>
        public Bitmap Bitmap { get; private set; }
        public int Height = 144;
        public int Width = 160;

        public DirectBitmap()
        {
            Bitmap = new Bitmap(Width, Height, ColorDepth.ColorDepth32);
            Pixels = Bitmap.RawData;
            Stride = (int)32 / 8;
            Pitch = (int)Width * Stride;
        }

        public DirectBitmap(int width, int height)
        {
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
            Bitmap = new Bitmap(Width, Height, ColorDepth.ColorDepth32);
            Pixels = Bitmap.RawData;
            Stride = (int)32 / 8;
            Pitch = (int)Width * Stride;
        }

        /// <summary>
        /// Zero-copy surface over the canvas back buffer: whatever is drawn here is shown by the
        /// next Canvas.Display(). Bitmap stays null. The canvas reallocates its buffer on a mode
        /// change, so create a new DirectBitmap after any Canvas.GetFullScreen(mode).
        /// </summary>
        public DirectBitmap(Canvas canvas)
        {
            int[] buffer = canvas != null ? canvas.GetBuffer() : null;

            if (buffer != null)
            {
                Width = canvas.Width;
                Height = canvas.Height;
                Pixels = buffer;
            }
            else
            {
                Width = 0;
                Height = 0;
                Pixels = Array.Empty<int>();
            }

            Stride = (int)32 / 8;
            Pitch = (int)Width * Stride;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, int colour)
        {
            if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            {
                Pixels[x + y * Width] = colour | (0xFF << 24);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixelAlpha(int x, int y, int colour)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return;
            }

            int index = x + y * Width;

            if (((uint)colour >> 24) == 0xFF)
            {
                Pixels[index] = colour;
                return;
            }

            int bgColour = Pixels[index];
            int alpha = (colour >> 24) & 0xff;
            int invAlpha = 255 - alpha;
            int newRed = (((colour >> 16) & 0xff) * alpha + ((bgColour >> 16) & 0xff) * invAlpha) >> 8;
            int newGreen = (((colour >> 8) & 0xff) * alpha + ((bgColour >> 8) & 0xff) * invAlpha) >> 8;
            int newBlue = ((colour & 0xff) * alpha + (bgColour & 0xff) * invAlpha) >> 8;

            Pixels[index] = (alpha << 24) | (newRed << 16) | (newGreen << 8) | newBlue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPixel(int x, int y)
        {
            if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            {
                return Pixels[x + y * Width];
            }

            return 0;
        }

        public void Clear(int colour)
        {
            Pixels.AsSpan().Fill(colour);
        }

        public void DrawString(string str, Font font, int color, int x, int y)
        {
            if (str == null || font == null)
            {
                return;
            }

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
            if (font == null)
            {
                return;
            }

            int height = font.Height;
            int width = font.Width;
            byte[] data = font.Data;

            // PSF glyph rows are (Width + 7) / 8 bytes wide (zap is 6 px = 1 byte, DefaultFont 16 px = 2 bytes).
            int bytesPerRow = (width + 7) / 8;
            int p = height * bytesPerRow * (byte)c;

            if (data == null || p + height * bytesPerRow > data.Length)
            {
                return;
            }

            if (x >= Width || y >= Height || x + width <= 0 || y + height <= 0)
            {
                return;
            }

            for (int cy = 0; cy < height; cy++)
            {
                int py = y + cy;
                if ((uint)py >= (uint)Height)
                {
                    continue;
                }

                int row = p + cy * bytesPerRow;
                for (int cx = 0; cx < width; cx++)
                {
                    if (font.ConvertByteToBitAddress(data[row + cx / 8], (cx % 8) + 1))
                    {
                        SetPixelAlpha(x + cx, py, color);
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

        /// <summary>
        /// Clips a width x height rectangle placed at (x, y) against this bitmap.
        /// On success (x, y) is the first visible destination pixel, (srcX, srcY) the matching
        /// offset inside the rectangle and w x h the visible size.
        /// </summary>
        private bool ClipRect(ref int x, ref int y, int width, int height, out int srcX, out int srcY, out int w, out int h)
        {
            srcX = 0;
            srcY = 0;
            w = width;
            h = height;

            if (width <= 0 || height <= 0 || x >= Width || y >= Height || x <= -width || y <= -height)
            {
                return false;
            }

            if (x < 0)
            {
                srcX = -x;
                w += x;
                x = 0;
            }

            if (y < 0)
            {
                srcY = -y;
                h += y;
                y = 0;
            }

            if (w > Width - x)
            {
                w = Width - x;
            }

            if (h > Height - y)
            {
                h = Height - y;
            }

            return w > 0 && h > 0;
        }

        /// <summary>
        /// Validates an image and clips it against this bitmap (see ClipRect).
        /// </summary>
        private bool ClipImage(Image image, ref int x, ref int y, out int[] src, out int srcX, out int srcY, out int w, out int h)
        {
            src = image != null ? image.RawData : null;

            if (src == null || src.Length < image.Width * image.Height)
            {
                srcX = 0;
                srcY = 0;
                w = 0;
                h = 0;
                return false;
            }

            return ClipRect(ref x, ref y, image.Width, image.Height, out srcX, out srcY, out w, out h);
        }

        public void DrawImage(Image image, int x, int y)
        {
            int[] src;
            int srcX, srcY, w, h;

            if (!ClipImage(image, ref x, ref y, out src, out srcX, out srcY, out w, out h))
            {
                return;
            }

            int srcWidth = image.Width;

            for (int yi = 0; yi < h; yi++)
            {
                int destOffset = (y + yi) * Width + x;
                int srcOffset = (srcY + yi) * srcWidth + srcX;

                // The gen2 copy helper took the destination first; Span.CopyTo is source.CopyTo(destination).
                src.AsSpan(srcOffset, w).CopyTo(Pixels.AsSpan(destOffset, w));
            }
        }

        public Bitmap ExtractImage(int srcX, int srcY, int width, int height)
        {
            width = Math.Max(0, width);
            height = Math.Max(0, height);

            Bitmap bmp = new(width, height, ColorDepth.ColorDepth32);

            int x = srcX;
            int y = srcY;
            int offsetX, offsetY, w, h;

            if (ClipRect(ref x, ref y, width, height, out offsetX, out offsetY, out w, out h))
            {
                int[] dest = bmp.RawData;

                for (int yi = 0; yi < h; yi++)
                {
                    int destOffset = (offsetY + yi) * width + offsetX;
                    int srcOffset = (y + yi) * Width + x;

                    Pixels.AsSpan(srcOffset, w).CopyTo(dest.AsSpan(destOffset, w));
                }
            }

            return bmp;
        }

        /// <summary>
        /// Blends straight-alpha ARGB source rows over destination rows, in place
        /// (bpl = bytes per line). Formerly the X# SSE2 plug AlphaBltSSE2ASM, now plain C#.
        /// </summary>
        public static void AlphaBlendSSE(uint *dest, int dbpl, uint* src, int sbpl, int width, int height)
        {
            if (dest == null || src == null || width <= 0 || height <= 0)
            {
                return;
            }

            for (int row = 0; row < height; row++)
            {
                BlendRow(new Span<int>((byte*)dest + (long)row * dbpl, width), new ReadOnlySpan<int>((byte*)src + (long)row * sbpl, width));
            }
        }

        /// <summary>
        /// Sets the alpha byte of every non fully transparent pixel to a (bpl = bytes per line).
        /// Formerly the X# plug BrightnessASM, now plain C#.
        /// </summary>
        public static void OpacitySSE(uint* pixelPtr, int w, int h, int bpl, uint a)
        {
            if (pixelPtr == null || w <= 0 || h <= 0)
            {
                return;
            }

            for (int row = 0; row < h; row++)
            {
                ApplyOpacity(new Span<int>((byte*)pixelPtr + (long)row * bpl, w), (byte)a);
            }
        }

        /// <summary>
        /// Blends one row of straight-alpha ARGB source pixels over the destination, in place.
        /// a == 255 copies, a == 0 skips, otherwise colour = (src * a + dst * (255 - a)) >> 8
        /// and alpha = min(255, dstA + a * a / 255) (gen2 AlphaBltSSE2ASM alpha word: the result
        /// stays opaque over opaque buffers and keeps transparency over Color.Transparent ones).
        /// </summary>
        private static void BlendRow(Span<int> dest, ReadOnlySpan<int> src)
        {
            int count = Math.Min(dest.Length, src.Length);

            for (int i = 0; i < count; i++)
            {
                uint sp = (uint)src[i];
                uint a = sp >> 24;

                if (a == 0xFF)
                {
                    dest[i] = (int)sp;
                    continue;
                }

                if (a == 0)
                {
                    continue;
                }

                uint dp = (uint)dest[i];
                uint ia = 255 - a;
                uint rb = (((sp & 0x00FF00FF) * a + (dp & 0x00FF00FF) * ia) >> 8) & 0x00FF00FF;
                uint g = (((sp & 0x0000FF00) * a + (dp & 0x0000FF00) * ia) >> 8) & 0x0000FF00;
                uint oa = Math.Min(255u, (dp >> 24) + (a * a + 127) / 255);

                dest[i] = (int)((oa << 24) | rb | g);
            }
        }

        /// <summary>
        /// gen2 OpacitySSE semantics: rewrites in place the alpha byte of every pixel whose alpha is not 0.
        /// </summary>
        private static void ApplyOpacity(Span<int> pixels, byte alpha)
        {
            int a = alpha << 24;

            for (int i = 0; i < pixels.Length; i++)
            {
                int p = pixels[i];

                if (((uint)p >> 24) != 0)
                {
                    pixels[i] = (p & 0x00FFFFFF) | a;
                }
            }
        }

        public void DrawImageAlpha(Image image, int x, int y, byte alpha = 0xFF)
        {
            int[] src;
            int srcX, srcY, w, h;

            if (!ClipImage(image, ref x, ref y, out src, out srcX, out srcY, out w, out h))
            {
                return;
            }

            if (alpha < 0xFF)
            {
                // As in gen2, this permanently mutates the source image.
                ApplyOpacity(src, alpha);
            }

            int srcWidth = image.Width;

            for (int yi = 0; yi < h; yi++)
            {
                int destOffset = (y + yi) * Width + x;
                int srcOffset = (srcY + yi) * srcWidth + srcX;

                BlendRow(Pixels.AsSpan(destOffset, w), src.AsSpan(srcOffset, w));
            }
        }

        public void DrawImageStretchAlpha(Image image, Rectangle sourceRect, Rectangle destRect)
        {
            if (image == null || sourceRect == null || destRect == null)
            {
                return;
            }

            int[] src = image.RawData;
            int srcWidth = image.Width;
            int srcHeight = image.Height;

            if (src == null || src.Length < srcWidth * srcHeight || destRect.Width <= 0 || destRect.Height <= 0)
            {
                return;
            }

            float scaleX = (float)sourceRect.Width / destRect.Width;
            float scaleY = (float)sourceRect.Height / destRect.Height;

            // Only walk the destination pixels that land inside this bitmap.
            int xStart = Math.Max(0, -destRect.Left);
            int xEnd = Math.Min(destRect.Width, Width - destRect.Left);
            int yStart = Math.Max(0, -destRect.Top);
            int yEnd = Math.Min(destRect.Height, Height - destRect.Top);

            for (int yi = yStart; yi < yEnd; yi++)
            {
                int srcY = (int)(yi * scaleY) + sourceRect.Top;
                srcY = Math.Min(srcY, sourceRect.Bottom - 1);

                if ((uint)srcY >= (uint)srcHeight)
                {
                    continue;
                }

                int destY = destRect.Top + yi;

                for (int xi = xStart; xi < xEnd; xi++)
                {
                    int srcX = (int)(xi * scaleX) + sourceRect.Left;
                    srcX = Math.Min(srcX, sourceRect.Right - 1);

                    if ((uint)srcX < (uint)srcWidth)
                    {
                        SetPixelAlpha(destRect.Left + xi, destY, src[srcX + srcY * srcWidth]);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a filled circle at the given coordinates with the given radius.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x0">The X center coordinate.</param>
        /// <param name="y0">The Y center coordinate.</param>
        /// <param name="radius">The radius of the circle to draw.</param>
        public void DrawFilledCircle(int color, int x0, int y0, int radius)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int drawX = x0 + x;
                        int drawY = y0 + y;
                        if ((uint)drawX < (uint)Width && (uint)drawY < (uint)Height)
                        {
                            SetPixelAlpha(drawX, drawY, color);
                        }
                    }
                }
            }
        }
    }
}