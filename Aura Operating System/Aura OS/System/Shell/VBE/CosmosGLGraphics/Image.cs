using Aura_OS.System.Shell.VBE.CosmosGLGraphics.Formats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE.CosmosGLGraphics
{
    public class Image
    {

        public static Image FromBytes(byte[] data, string type)
        {
            switch (type)
            {
                case "ppm":
                    return new PPM().Read(data);
            }

            return null;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        private uint[] _buffer;

        public Image SetBuffer(uint[] pixels)
        {
            _buffer = pixels;
            return this;
        }

        public Image(int width, int height)
        {
            Width = width;
            Height = height;

            _buffer = new uint[Width * Height * 4];
        }

        public void SetPixel(int x, int y, Color c)
        {
            SetPixel(x, y, (uint)c.ToHex());
        }

        public void SetPixel(int x, int y, uint c)
        {
            _buffer[x + (y * Width)] = c;
        }

        public Color GetPixel(int x, int y)
        {
            return new Color((int)_buffer[x + (y * Width)]);
        }

        public void Clear(Color c)
        {
            Clear((uint)c.ToHex());
        }

        public void Clear(uint c)
        {
            for (int i = 0; i < Width * Height * 4; i++)
            {
                _buffer[i] = c;
            }
        }

        public Image ResizeImage(int w1, int h1)
        {
            return new Image(w1, h1).SetBuffer(resizePixels(_buffer, Width, Height, w1, h1));
        }

        private uint[] resizePixels(uint[] pixels, int w1, int h1, int w2, int h2)
        {
            uint[] temp = new uint[w2 * h2];
            // EDIT: added +1 to account for an early rounding problem
            int x_ratio = (int)((w1 << 16) / w2) + 1;
            int y_ratio = (int)((h1 << 16) / h2) + 1;
            //int x_ratio = (int)((w1<<16)/w2) ;
            //int y_ratio = (int)((h1<<16)/h2) ;
            int x2, y2;
            for (int i = 0; i < h2; i++)
            {
                for (int j = 0; j < w2; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * w2) + j] = pixels[(y2 * w1) + x2];
                }
            }
            return temp;
        }
    }
}
