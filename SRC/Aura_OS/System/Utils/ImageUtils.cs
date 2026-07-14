/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Bitmap scaling helpers
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Utils
{
    public static class ImageUtils
    {
        /// <summary>
        /// Returns the source image scaled with nearest-neighbour sampling
        /// (same fixed-point math as gen3's Canvas.ScaleImage, but producing a
        /// reusable Bitmap so the scaling cost is paid once instead of per frame).
        /// </summary>
        public static Bitmap Scale(Image source, uint width, uint height)
        {
            Bitmap scaled = new Bitmap(width, height, ColorDepth.ColorDepth32);

            int w1 = (int)source.Width;
            int h1 = (int)source.Height;
            int w2 = (int)width;
            int h2 = (int)height;
            int xRatio = (w1 << 16) / w2 + 1;
            int yRatio = (h1 << 16) / h2 + 1;

            int[] src = source.RawData;
            int[] dst = scaled.RawData;

            for (int y = 0; y < h2; y++)
            {
                int srcY = (y * yRatio) >> 16;
                int srcRow = srcY * w1;
                int dstRow = y * w2;

                for (int x = 0; x < w2; x++)
                {
                    int srcX = (x * xRatio) >> 16;
                    dst[dstRow + x] = src[srcRow + srcX];
                }
            }

            return scaled;
        }

        /// <summary>
        /// Scales an image to the current screen size. gen2 required wallpapers
        /// to match the screen exactly and blanked to black otherwise; gen3 runs
        /// at the real framebuffer size (e.g. 1280x800), so the 1920x1080
        /// wallpapers must be adapted at load time instead.
        /// </summary>
        public static Bitmap ScaleToScreen(Image source)
        {
            if (source.Width == Kernel.ScreenWidth && source.Height == Kernel.ScreenHeight && source is Bitmap bitmap)
            {
                return bitmap;
            }

            return Scale(source, Kernel.ScreenWidth, Kernel.ScreenHeight);
        }
    }
}
