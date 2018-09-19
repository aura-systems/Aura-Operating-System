/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;

namespace Aura_OS.HAL.Drivers.Video
{

    class ManagedVBE
    {
        public static MemoryBlock LinearFrameBuffer;

        int width;
        int height;
        public static int len;

        uint mScrollSize;
        uint mRow2Addr;

        public static uint OffScreenSize;

        public ManagedVBE(int xres, int yres, uint pointer, bool lfb)
        {
            width = xres;
            height = yres;
            len = width * height;

            xBytePerPixel = 32 / 8;
            stride = 32 / 8;

            if (lfb)
            {
                mScrollSize = (uint)(len * 4);
                mRow2Addr = (uint)(width * 4 * 16);
                pitch = (uint)(width * xBytePerPixel);
                LinearFrameBuffer = new MemoryBlock(pointer, (uint)(width * height * 4));
            }
            else
            {
                OffScreenSize = (uint)((System.Graphics.VBE.Graphics.ModeInfo.pitch / 4) - System.Graphics.VBE.Graphics.ModeInfo.width);
                mScrollSize = (uint)(len * 4 + (OffScreenSize * 4));
                mRow2Addr = (uint)((width + OffScreenSize) * 4 * 16);
                pitch = (uint)((width + OffScreenSize) * xBytePerPixel);
                LinearFrameBuffer = new MemoryBlock(pointer, (uint)((width * height * 4) + (OffScreenSize * height * 4)));
            }

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
                    SetVRAM(GetPointOffset(x, y), c);
                }
            }
            else
            {
                SetVRAM(GetPointOffset(x, y), c);
            }            
        }

        uint xBytePerPixel;
        uint stride;
        uint pitch;

        private uint GetPointOffset(int x, int y)
        {
            return (uint)((x * stride) + (y * pitch));
        }

        public void ScrollUp()
        {
            MoveDown(0, mRow2Addr, mScrollSize);
            LinearFrameBuffer.Fill(mScrollSize, mRow2Addr, 0x00);
        }

        public unsafe void MoveDown(uint aDest, uint aSrc, uint aCount)
        {
            byte* xDest = (byte*)(System.Graphics.VBE.Graphics.ModeInfo.framebuffer + aDest);
            byte* xSrc = (byte*)(System.Graphics.VBE.Graphics.ModeInfo.framebuffer + aSrc);
            MemoryOperations.Copy(xDest, xSrc, (int)aCount);
        }
    }

}
