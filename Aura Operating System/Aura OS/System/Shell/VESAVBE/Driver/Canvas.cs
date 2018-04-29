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

        public VESACanvas(int width, int height)
        {
            Graphics.widthVESA = width;
            Graphics.heightVESA = height;

            _buffer = (uint*)Heap.MemAlloc((uint)(Graphics.widthVESA * Graphics.heightVESA * 4));
        }

        private static uint* _buffer;

        public void SetPixel(int x, int y, uint c)
        {
            _buffer[x + (y * Graphics.widthVESA)] = c;
        }

        internal void Clear(uint c)
        {
            Memory.Memset(_buffer, c, (uint)(Graphics.widthVESA * Graphics.heightVESA));
        }

        internal static void WriteToScreen()
        {
            Memory.Memcpy((uint*)Graphics.vga_mem, _buffer, Graphics.widthVESA * Graphics.heightVESA);
        }

        public void ScrollUp()
        {

            for (int i = 0; i < Graphics.heightVESA; i++)
            {
                for (int m = 0; m < Graphics.widthVESA; m++)
                {
                    _buffer[i * Graphics.widthVESA + m] = _buffer[(i + 16) * Graphics.widthVESA + m];

                }
            }

            for (int i = Graphics.heightVESA - 16; i < Graphics.heightVESA; i++)
            {
                for (int m = 0; m < Graphics.widthVESA; m++)
                {
                    _buffer[i * Graphics.widthVESA + m] = 0x00;

                }
            }


        }
    }
}
