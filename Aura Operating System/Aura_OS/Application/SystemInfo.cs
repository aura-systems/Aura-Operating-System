using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System;
using System.Drawing;

namespace Aura_OS
{
    public class SystemInfo : App
    {
        public SystemInfo(uint width, uint height, uint x = 0, uint y = 0) : base("MemoryInfo", width, height, x, y)
        {

        }

        public override void UpdateApp()
        {
            Kernel.canvas.DrawString("Available RAM               =" + GCImplementation.GetAvailableRAM(), Kernel.font, Kernel.BlackPen, (int)x, (int)y);
            Kernel.canvas.DrawString("Used RAM                    =" + GCImplementation.GetUsedRAM(), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + Kernel.font.Height));
            Kernel.canvas.DrawString("Small Allocated Object Count=" + HeapSmall.GetAllocatedObjectCount(), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 2 * Kernel.font.Height));
            Kernel.canvas.DrawString("Small Page Count            =" + RAT.GetPageCount(RAT.PageType.HeapSmall), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 3 * Kernel.font.Height));
            Kernel.canvas.DrawString("Medium Page Count           =" + RAT.GetPageCount(RAT.PageType.HeapMedium), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 4 * Kernel.font.Height));
            Kernel.canvas.DrawString("Large Page Count            =" + RAT.GetPageCount(RAT.PageType.HeapLarge), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 5 * Kernel.font.Height));
            Kernel.canvas.DrawString("RAT Page Count              =" + RAT.GetPageCount(RAT.PageType.RAT), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 6 * Kernel.font.Height));
            Kernel.canvas.DrawString("SMT Page Count              =" + RAT.GetPageCount(RAT.PageType.SMT), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 7 * Kernel.font.Height));
            Kernel.canvas.DrawString("GC Managed Page Count       =" + RAT.GetPageCount(RAT.PageType.SMT), Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 8 * Kernel.font.Height));
            Kernel.canvas.DrawString("Free Count                  =" + Kernel.FreeCount, Kernel.font, Kernel.BlackPen, (int)x, (int)(y + 9 * Kernel.font.Height));
        }
    }
}
