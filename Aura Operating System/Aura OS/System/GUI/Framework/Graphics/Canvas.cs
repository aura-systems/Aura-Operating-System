using Cosmos.Core.Memory.Old;
using Aura_OS.System.GUI.Drivers;

namespace Aura_OS.System.GUI.Graphics
{
    public unsafe class Canvas : ICanvas
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private uint* _buffer;

        public Canvas(int width, int height)
        {
            Width = width;
            Height = height;

            _buffer = (uint*) Heap.MemAlloc((uint) (Width * Height * 4));
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

        public void WriteToScreen()
        {
            Memory.Memcpy((uint*) 0xE0000000, _buffer, Width * Height);
        }

        public uint Blit(int x0, int y0, int w, int h)
        {
            int y;
            uint target = 0;
            for (y = 0; y < h; y++)
            {
                int yoff = ((y + y0) * w * 4);
                Memory.Memcpy((byte*)target, (byte*)_buffer + (yoff) + (x0 * 4), w);
            }
            return target;
        }

        public void SetScanLine(int offset, int length, uint color)
        {
            Memory.Memset(offset + _buffer, color, (uint)length);
        }

        public void Clear(uint c)
        {
            Memory.Memset(_buffer, c, (uint)(Width * Height));
        }
    }
}