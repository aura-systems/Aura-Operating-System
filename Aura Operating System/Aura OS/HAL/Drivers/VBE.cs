/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/


using Cosmos.Core;

namespace Aura_OS.HAL.Drivers
{

    class ManagedVBE
    {
        public static MemoryBlock LinearFrameBuffer;

        int width;
        int height;
        public static int len;

        uint mScrollSize;
        uint mRow2Addr;

        public ManagedVBE(int xres, int yres, uint pointer)
        {
            width = xres;
            height = yres;
            len = width * height;

            mScrollSize = (uint)(len * 4);
            mRow2Addr = (uint)(width * 4 * 16);

            LinearFrameBuffer = new MemoryBlock(pointer, (uint)(width * height * 4));
        }

        public void SetVRAM(uint index, byte value)
        {
            LinearFrameBuffer.Bytes[index] = value;
        }

        public void SetVRAM(uint index, ushort value)
        {
            LinearFrameBuffer.Words[index] = value;
        }

        public void SetVRAM(uint index, uint value)
        {
            LinearFrameBuffer.DWords[index] = value;
        }

        public byte GetVRAM(uint index)
        {
            return LinearFrameBuffer.Bytes[index];
        }

        public void ClearVRAM(uint value)
        {
            LinearFrameBuffer.Fill(value);
        }

        public void ClearVRAM(int aStart, int aCount, int value)
        {
            LinearFrameBuffer.Fill(aStart, aCount, value);
        }

        public void CopyVRAM(int aStart, int[] aData, int aIndex, int aCount)
        {
            LinearFrameBuffer.Copy(aStart, aData, aIndex, aCount);
        }

        public void SetPixel(int x, int y, uint c, bool background)
        {
            if (background)
            {
                if (c != 0x00)
                {
                    uint offset;
                    offset = (uint)GetPointOffset(x, y);
                    SetVRAM(offset, c);
                }
            }
            else
            {
                uint offset;
                offset = (uint)GetPointOffset(x, y);
                SetVRAM(offset, c);
            }            
        }

        private int GetPointOffset(int x, int y)
        {
            int xBytePerPixel = 32 / 8;
            int stride = 32 / 8;
            int pitch = width * xBytePerPixel;
            return (x * stride) + (y * pitch);
        }

        public void ScrollUp()
        {
            LinearFrameBuffer.MoveDown(0, mRow2Addr, mScrollSize);
            LinearFrameBuffer.Fill(mScrollSize, mRow2Addr, 0x00);
        }
    }
}
