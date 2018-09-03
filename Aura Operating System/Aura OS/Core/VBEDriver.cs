using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.Core
{
    public class VbeDriver
    {
       
        public Cosmos.Core.IOGroup.VBE Io = Cosmos.Core.Global.BaseIOGroups.VBE;

        private void vbe_write(ushort index, ushort value)
        {
            Io.VbeIndex.Word =  index;
            Io.VbeData.Word = value;
        }        

        public void vbe_set(ushort xres, ushort yres, ushort bpp)
        {
            //Disable Display
            vbe_write(0x4, 0x00);
            //Set Display Xres
            vbe_write(0x1, xres);
            //SetDisplay Yres
            vbe_write(0x2, yres);
            //SetDisplay bpp
            vbe_write(0x3, bpp);
            //Enable Display and LFB           
            vbe_write(0x4, (ushort)(0x01 | 0x40));
        }

        public void vbe_disable()
        {
            vbe_write(0x4, 0x00);
        }

        public void set_vram(byte[] value)
        {
            for (uint i = 0; i < value.Length; i++)
            {
                Io.LinearFrameBuffer[i] = value[i];
            }
        }

        public void set_vram(uint index, byte value)
        {
            Io.LinearFrameBuffer[index] = value;
        }

        public byte get_vram(uint index)
        {
            return (byte)Io.LinearFrameBuffer[index];
        }

    }
}
