/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE Canvas
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Cosmos.Core.Memory.Old;

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
        public void ScrollUp()
        {

            for (int i = 0; i < Height; i++)
            {
                for (int m = 0; m < Width; m++)
                {
                    _buffer[i * Width + m] = _buffer[(i + 16) * Width + m];

                }
            }

            for (int i = Height - 16; i < Height; i++)
            {
                for (int m = 0; m < Width; m++)
                {
                    _buffer[i * Width + m] = 0x00;

                }
            }

            
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
