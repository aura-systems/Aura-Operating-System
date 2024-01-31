/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Core;
using Cosmos.Core.Memory;

namespace Aura_OS.System.Processing.Applications
{
    public class MemoryInfoApp : Application
    {
        public static string ApplicationName = "MemoryInfo";

        public MemoryInfoApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {

        }

        public override void Draw()
        {
            if (Dirty)
            {
                base.Draw();

                Canvas.DrawString("Available RAM                = " + GCImplementation.GetAvailableRAM() + "MB", Kernel.font, Kernel.BlackColor, (int)X, (int)Y);
                Canvas.DrawString("Used RAM                     = " + GCImplementation.GetUsedRAM() + "B", Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + Kernel.font.Height));
                Canvas.DrawString("Small Allocated Object Count = " + HeapSmall.GetAllocatedObjectCount(), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 2 * Kernel.font.Height));
                Canvas.DrawString("Small Page Count             = " + RAT.GetPageCount((byte)RAT.PageType.HeapSmall), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 3 * Kernel.font.Height));
                Canvas.DrawString("Medium Page Count            = " + RAT.GetPageCount((byte)RAT.PageType.HeapMedium), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 4 * Kernel.font.Height));
                Canvas.DrawString("Large Page Count             = " + RAT.GetPageCount((byte)RAT.PageType.HeapLarge), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 5 * Kernel.font.Height));
                Canvas.DrawString("RAT Page Count               = " + RAT.GetPageCount((byte)RAT.PageType.RAT), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 6 * Kernel.font.Height));
                Canvas.DrawString("SMT Page Count               = " + RAT.GetPageCount((byte)RAT.PageType.SMT), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 7 * Kernel.font.Height));
                Canvas.DrawString("GC Managed Page Count        = " + RAT.GetPageCount((byte)RAT.PageType.SMT), Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 8 * Kernel.font.Height));
                Canvas.DrawString("Free Count                   = " + Kernel.FreeCount, Kernel.font, Kernel.BlackColor, (int)X, (int)(Y + 9 * Kernel.font.Height));
            }
        }
    }
}
