/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/


using Aura_OS.Core;
using Cosmos.Core.Memory.Old;

namespace Aura_OS.System.Shell.VESAVBE.Driver
{
    unsafe class VESACanvas
    {

        //public static void SetPixel(int x, int y, int RGB)
        //{
        //    int offset = x * (Graphics.depthVESA / 8) + y * (Graphics.widthVESA * (Graphics.depthVESA / 8));
        //
        //    Graphics.vga_mem[offset + 0] = (byte)(RGB & 0xff);
        //     Graphics.vga_mem[offset + 1] = (byte)((RGB >> 8) & 0xff);
        //    Graphics.vga_mem[offset + 2] = (byte)((RGB >> 16) & 0xff);
        //
        //}

        public static int Width { get; set; }
        public static int Height { get; set; }

        public VESACanvas(int width, int height)
        {
            Width = width;
            Height = height;

            _buffer = (uint*)Heap.MemAlloc((uint)(Width * Height * 4));
        }

        private static uint* _buffer;

        public void SetPixel(int x, int y, uint c)
        {
            _buffer[x + (y * Width)] = c;
        }

        internal void Clear(uint c)
        {
            Memory.Memset(_buffer, c, (uint)(Width * Height));
        }

        internal static void WriteToScreen()
        {
            Memory.Memcpy((uint*)Graphics.vga_mem, _buffer, Width * Height);
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

    }
}
