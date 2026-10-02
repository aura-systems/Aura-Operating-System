/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.System.Processing.Applications
{
    public class MemoryInfoApp : Application
    {
        public static string ApplicationName = "MemoryInfo";

        // Last-collection figures from GC.GetGCMemoryInfo(), refreshed only when a collection
        // happened (the snapshot does not change in between, and each call allocates).
        private int _lastCollection = -1;
        private long _gcHeapSize;
        private long _gcCommitted;
        private long _gcFragmented;
        private long _gcPinnedObjects;

        public MemoryInfoApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            ForceDirty = true;
        }

        public override void Draw()
        {
            base.Draw();

            ulong totalPages = MemoryInfo.TotalPages;
            ulong freePages = MemoryInfo.FreePages;
            ulong pageSize = MemoryInfo.PageSizeBytes;

            int collections = MemoryInfo.TotalCollections;
            if (collections != _lastCollection)
            {
                GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
                _gcHeapSize = gcInfo.HeapSizeBytes;
                _gcCommitted = gcInfo.TotalCommittedBytes;
                _gcFragmented = gcInfo.FragmentedBytes;
                _gcPinnedObjects = gcInfo.PinnedObjectsCount;
                _lastCollection = collections;
            }

            // GEN3-GAP(meminfo): the page allocator's pool is the largest usable memory-map region only.
            DrawString("Memory pool                  = " + ((totalPages * pageSize) >> 20) + "MB", 0, 0);
            DrawString("Used memory                  = " + (((totalPages - freePages) * pageSize) >> 20) + "MB", 0, (0 + Kernel.font.Height));
            DrawString("Free memory                  = " + ((freePages * pageSize) >> 20) + "MB", 0, (0 + 2 * Kernel.font.Height));
            DrawString("Total Page Count             = " + totalPages + " (" + pageSize + "B)", 0, (0 + 3 * Kernel.font.Height));
            DrawString("Free Page Count              = " + freePages, 0, (0 + 4 * Kernel.font.Height));
            DrawString("Live heap                    = " + GC.GetTotalMemory(false) + "B", 0, (0 + 5 * Kernel.font.Height));
            DrawString("Heap size (last GC)          = " + _gcHeapSize + "B", 0, (0 + 6 * Kernel.font.Height));
            DrawString("Committed (last GC)          = " + _gcCommitted + "B", 0, (0 + 7 * Kernel.font.Height));
            DrawString("Fragmented (last GC)         = " + _gcFragmented + "B", 0, (0 + 8 * Kernel.font.Height));
            DrawString("Pinned objects (last GC)     = " + _gcPinnedObjects, 0, (0 + 9 * Kernel.font.Height));
            DrawString("Collections                  = " + collections, 0, (0 + 10 * Kernel.font.Height));
            DrawString("Objects freed                = " + MemoryInfo.TotalObjectsFreed, 0, (0 + 11 * Kernel.font.Height));
            DrawString("GC time                      = " + MemoryInfo.GcTimePercent + "%", 0, (0 + 12 * Kernel.font.Height));
            DrawString("Free Count                   = " + Kernel.FreeCount, 0, (0 + 13 * Kernel.font.Height));
        }
    }
}
