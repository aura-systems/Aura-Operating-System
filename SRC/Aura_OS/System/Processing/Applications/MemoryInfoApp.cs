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
            MarkDirty();

            base.Draw();

            DrawString("Available RAM                = " + GCImplementation.GetAvailableRAM() + "MB", 0, 0);
            DrawString("Used RAM                     = " + GCImplementation.GetUsedRAM() + "B", 0, (0 + Kernel.font.Height));
            DrawString("Small Allocated Object Count = " + HeapSmall.GetAllocatedObjectCount(), 0, (0 + 2 * Kernel.font.Height));
            DrawString("Small Page Count             = " + RAT.GetPageCount((byte)RAT.PageType.HeapSmall), 0, (0 + 3 * Kernel.font.Height));
            DrawString("Medium Page Count            = " + RAT.GetPageCount((byte)RAT.PageType.HeapMedium), 0, (0 + 4 * Kernel.font.Height));
            DrawString("Large Page Count             = " + RAT.GetPageCount((byte)RAT.PageType.HeapLarge), 0, (0 + 5 * Kernel.font.Height));
            DrawString("RAT Page Count               = " + RAT.GetPageCount((byte)RAT.PageType.RAT), 0, (0 + 6 * Kernel.font.Height));
            DrawString("SMT Page Count               = " + RAT.GetPageCount((byte)RAT.PageType.SMT), 0, (0 + 7 * Kernel.font.Height));
            DrawString("GC Managed Page Count        = " + RAT.GetPageCount((byte)RAT.PageType.SMT), 0, (0 + 8 * Kernel.font.Height));
            DrawString("Free Count                   = " + Kernel.FreeCount, 0, (0 + 9 * Kernel.font.Height));
        }
    }
}
