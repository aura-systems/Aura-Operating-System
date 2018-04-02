using Cosmos.HAL.Drivers.PCI.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.SVGAII
{
    class Graphics
    {
        private static byte[] font;
        static VMWareSVGAII svga;
        private uint[] pallete = new uint[16];

        public void Init()
        {
            pallete[1] = 0x1F3FAB;
            pallete[2] = 0x027D11; // Green
            pallete[3] = 0x1C9AE8; // Aqua
            pallete[4] = 0x941010; // Red 941010
            pallete[5] = 0x000000; // Purple 807E7F
            pallete[6] = 0x000000; // Gray
            pallete[7] = 0x807E7F; // Gray
            pallete[8] = 0x525225; // Dark Gray 525252
            pallete[9] = 0x0E55ED; // Light blue 0E55ED
            pallete[10] = 0x21CC73; // Light green 21CC73
            pallete[11] = 0x91E0EB; // Light aqua 91E0EB
            pallete[12] = 0xF00000; // Light red
            pallete[13] = 0xF788DC; // Light purple F788DC
            pallete[14] = 0xE8E546; // Yellow E8E546
            pallete[15] = 0xEBEBEB;
            font = Terminal.read_font();
            svga = new VMWareSVGA();
            svga.SetMode(800, 600);
            svga.Clear(0);
        }
        public void Clear()
        {
            svga.Clear(0x000000);
            svga.Update(0, 0, 800, 600);
        }

    }
}
