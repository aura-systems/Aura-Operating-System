/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.Heap;
using GarbageCollector = Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;

namespace Aura_OS.System.Processing.Applications
{
    public class MemoryInfoApp : Application
    {
        public static string ApplicationName = "MemoryInfo";

        public MemoryInfoApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            ForceDirty = true;
        }

        public override void Draw()
        {
            base.Draw();

            PageAllocator.GetPageCountsByType(out _, out ulong gcHeap, out ulong heapSmall,
                out ulong heapMedium, out ulong heapLarge, out _, out _,
                out ulong pageAllocator, out ulong smt, out _, out _);

            ulong availableRam = PageAllocator.FreePageCount * PageAllocator.PageSize / (1024 * 1024);
            ulong usedRam = GarbageCollector.GetTotalCommittedBytes();
            GarbageCollector.GetStats(out int totalCollections, out int totalObjectsFreed);

            DrawString("Available RAM                = " + availableRam + "MB", 0, 0);
            DrawString("Used RAM                     = " + usedRam + "B", 0, (0 + Kernel.font.Height));
            DrawString("Small Allocated Object Count = " + SmallHeap.GetAllocatedObjectCount(), 0, (0 + 2 * Kernel.font.Height));
            DrawString("Small Page Count             = " + heapSmall, 0, (0 + 3 * Kernel.font.Height));
            DrawString("Medium Page Count            = " + heapMedium, 0, (0 + 4 * Kernel.font.Height));
            DrawString("Large Page Count             = " + heapLarge, 0, (0 + 5 * Kernel.font.Height));
            DrawString("Page Allocator Page Count    = " + pageAllocator, 0, (0 + 6 * Kernel.font.Height));
            DrawString("SMT Page Count               = " + smt, 0, (0 + 7 * Kernel.font.Height));
            DrawString("GC Managed Page Count        = " + gcHeap, 0, (0 + 8 * Kernel.font.Height));
            DrawString("GC Collections               = " + totalCollections + " (" + totalObjectsFreed + " objects freed)", 0, (0 + 9 * Kernel.font.Height));
            DrawString("Free Count                   = " + Kernel.FreeCount, 0, (0 + 10 * Kernel.font.Height));
        }
    }
}
