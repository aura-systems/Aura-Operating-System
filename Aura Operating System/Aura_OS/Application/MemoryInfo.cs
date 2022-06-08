using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System;
using System.Drawing;

namespace Aura_OS
{
    public class MemoryInfo : WaveOS.GUI.WaveWindow
    {
        public MemoryInfo(uint width, uint height, uint x = 0, uint y = 0) : base("MemoryInfo", (int)x, (int)y, (int)width, (int)height, Kernel.WindowManager, new Cosmos.System.Graphics.Pen(Color.FromArgb(191, 191, 191)))
        {

        }

        public override void UpdateWindow()
        {
            /*
            Kernel.canvas.DrawString("Available RAM                = " + GCImplementation.GetAvailableRAM(), Kernel.font, Kernel.BlackPen, (int)X, (int)Y + titleBarHeight);
            Kernel.canvas.DrawString("Used RAM                     = " + GCImplementation.GetUsedRAM(), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("Small Allocated Object Count = " + HeapSmall.GetAllocatedObjectCount(), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 2 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("Small Page Count             = " + RAT.GetPageCount(RAT.PageType.HeapSmall), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 3 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("Medium Page Count            = " + RAT.GetPageCount(RAT.PageType.HeapMedium), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 4 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("Large Page Count             = " + RAT.GetPageCount(RAT.PageType.HeapLarge), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 5 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("RAT Page Count               = " + RAT.GetPageCount(RAT.PageType.RAT), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 6 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("SMT Page Count               = " + RAT.GetPageCount(RAT.PageType.SMT), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 7 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("GC Managed Page Count        = " + RAT.GetPageCount(RAT.PageType.SMT), Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 8 * Kernel.font.Height) + titleBarHeight);
            Kernel.canvas.DrawString("Free Count                   = " + Kernel.FreeCount, Kernel.font, Kernel.BlackPen, (int)X, (int)(Y + 9 * Kernel.font.Height) + titleBarHeight);
        */
        }
    }
}
