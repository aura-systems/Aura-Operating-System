using Aura_OS.Core;
using Cosmos.Core.Memory.Old;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE.CosmosGLGraphics
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

            _buffer = (uint*)Heap.MemAlloc((uint)(Width * Height * 4));
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
            Memory.Memcpy((uint*)0xE0000000, _buffer, Width * Height);
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
