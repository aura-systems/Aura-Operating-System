using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Graphics.Fonts;

namespace Aura_OS.System.Graphics
{
    public class SVGAIIGraphics
    {
        public Cosmos.HAL.Drivers.PCI.Video.VMWareSVGAII SVGA;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public SVGAIIGraphics()
        {
            SVGA = new Cosmos.HAL.Drivers.PCI.Video.VMWareSVGAII();
            SetMode(800, 600);
        }

        public void SetMode(int w, int h)
        {
            if (w < 0) { w = 640; }
            if (h < 0) { h = 480; }
            Width = w;
            Height = h;
            SVGA.SetMode((uint)w, (uint)h, 32);
        }

        public void Clear(Color color)
        {
            SVGA.Clear((uint)color.ToArgb());
        }

        public void Clear(uint color)
        {
            SVGA.Clear(color);
        }

        public void Scroll(int pixels)
        {
            SVGA.Copy(0, (uint)pixels, 0, 0, (uint)Width, (uint)(Height - pixels));
        }

        public void Copy(int x, int y, int newX, int newY, int w, int h)
        {
            SVGA.Copy((uint)x, (uint)y, (uint)newX, (uint)newY, (uint)w, (uint)h);
        }

        public void Update() { SVGA.Update(0, 0, (uint)Width, (uint)Height); }

        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return; }
            SVGA.SetPixel((uint)x, (uint)y, (uint)color.ToArgb());
        }

        public void DrawFilledRect(int x, int y, int w, int h, Color color)
        {
            for (int i = 0; i < w * h; i++) { SetPixel(x + (i % w), y + (i / w), color); }
        }

        public void DrawRect(int x, int y, int w, int h, int thickness, Color color)
        {
            DrawFilledRect(x, y, w, thickness, color);
            DrawFilledRect(x, y + h - thickness, w, thickness, color);
            DrawFilledRect(x, y + thickness, thickness, h - (thickness * 2), color);
            DrawFilledRect(x + w - thickness, y + thickness, thickness, h - (thickness * 2), color);
        }

        public void DrawChar(int x, int y, char c, Color fg, Font font)
        {
            uint p = (uint)(font.Height * c);
            for (int cy = 0; cy < font.Height; cy++)
            {
                for (int cx = 0; cx < font.Width; cx++)
                {
                    if (ConvertByteToBitAddress(font.Data[p + cy], cx + 1))
                    { SetPixel(x + (font.Width - cx), y + cy, fg); }
                }
            }
        }

        public void DrawChar(int x, int y, char c, Color fg, Color bg, Font font)
        {
            uint p = (uint)(font.Height * c);
            for (int cy = 0; cy < font.Height; cy++)
            {
                for (int cx = 0; cx < font.Width; cx++)
                {
                    if (ConvertByteToBitAddress(font.Data[p + cy], cx + 1))
                    { SetPixel(x + (font.Width - cx), y + cy, fg); }
                    else { SetPixel(x + (font.Width - cx), y + cy, bg); }
                }
            }
        }

        public void DrawString(int x, int y, string text, Color fg, Font font)
        {
            int dx = x, dy = y;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { dx = x; dy += font.Height; }
                else
                {
                    DrawChar(dx, dy, text[i], fg, font);
                    dx += font.Width;
                }
            }
        }


        public void DrawString(int x, int y, string text, Color fg, Color bg, Font font)
        {
            int dx = x, dy = y;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { dx = x; dy += font.Height; }
                else
                {
                    DrawChar(dx, dy, text[i], fg, bg, font);
                    dx += font.Width;
                }
            }
        }

        private bool ConvertByteToBitAddress(byte to_convert, int to_return)
        {
            int mask = 1 << (to_return - 1);
            return (to_convert & mask) != 0;
        }
    }
}
