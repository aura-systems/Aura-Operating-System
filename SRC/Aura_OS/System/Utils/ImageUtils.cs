/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Image helpers (wallpaper scaling)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Utils
{
    /// <summary>
    /// GEN3-GAP(blit): gen3 has no public image scaling that produces a reusable image
    /// (the stretched Canvas.DrawImage overloads rescale on every call), and gen3 runs at
    /// the real framebuffer size (GEN3-GAP(display-mode)), so the 1920x1080 wallpapers are scaled
    /// here once at load time, never per frame.
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// Returns a copy of the source image scaled to width x height with nearest-neighbour
        /// sampling (16.16 fixed point, the same maths as gen3's private Canvas.ScaleImage).
        /// Returns null for a null/empty source or a non-positive size.
        /// </summary>
        public static Bitmap ScaleTo(Image src, int w, int h)
        {
            if (src == null || w <= 0 || h <= 0)
            {
                return null;
            }

            int srcWidth = src.Width;
            int srcHeight = src.Height;
            int[] s = src.RawData;

            if (srcWidth <= 0 || srcHeight <= 0 || s == null || s.Length < srcWidth * srcHeight)
            {
                return null;
            }

            Bitmap dst = new Bitmap(w, h, ColorDepth.ColorDepth32);
            int[] d = dst.RawData;

            long xRatio = (((long)srcWidth << 16) / w) + 1;
            long yRatio = (((long)srcHeight << 16) / h) + 1;

            // Source column of every destination column, computed once (clamped: the +1 rounding
            // can step past the last column when upscaling).
            int[] columns = new int[w];
            for (int x = 0; x < w; x++)
            {
                int srcX = (int)((x * xRatio) >> 16);
                columns[x] = srcX < srcWidth ? srcX : srcWidth - 1;
            }

            for (int y = 0; y < h; y++)
            {
                int srcY = (int)((y * yRatio) >> 16);
                if (srcY >= srcHeight)
                {
                    srcY = srcHeight - 1;
                }

                int srcRow = srcY * srcWidth;
                int dstRow = y * w;

                for (int x = 0; x < w; x++)
                {
                    d[dstRow + x] = s[srcRow + columns[x]];
                }
            }

            return dst;
        }

        /// <summary>
        /// Scales an image to the screen (Kernel.ScreenWidth x Kernel.ScreenHeight). Returns the
        /// source itself when it already has the screen size (no copy), or when it cannot be scaled.
        /// gen2 required wallpapers to match the screen and blanked to black otherwise.
        /// </summary>
        public static Image ScaleToScreen(Image src)
        {
            if (src == null)
            {
                return null;
            }

            int screenWidth = (int)Kernel.ScreenWidth;
            int screenHeight = (int)Kernel.ScreenHeight;

            if (src.Width == screenWidth && src.Height == screenHeight)
            {
                return src;
            }

            Bitmap scaled = ScaleTo(src, screenWidth, screenHeight);

            return scaled != null ? scaled : src;
        }
    }
}
