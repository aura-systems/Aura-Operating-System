/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application. The window is Resources/UI/Layouts/MemoryInfo.xml.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.System.Processing.Applications
{
    public class MemoryInfoApp : Application
    {
        // Each refresh builds the value strings: not every frame, the app would grow the heap it measures.
        private const int RefreshIntervalMs = 250;

        private DateTime _lastRefresh;

        // Last-collection figures from GC.GetGCMemoryInfo(), refreshed only when a collection
        // happened (the snapshot does not change in between, and each call allocates).
        private int _lastCollection = -1;
        private long _gcHeapSize;
        private long _gcCommitted;
        private long _gcFragmented;
        private long _gcPinnedObjects;

        public MemoryInfoApp(int x = 0, int y = 0) : base(AppLayout.Load("MemoryInfo"), x, y)
        {
            Refresh();
        }

        public override void Update()
        {
            // Before the base update, which places the labels again when a value got longer or shorter.
            if ((DateTime.Now - _lastRefresh).TotalMilliseconds >= RefreshIntervalMs)
            {
                Refresh();
            }

            base.Update();
        }

        private void Refresh()
        {
            _lastRefresh = DateTime.Now;

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
            SetValue("memoryPool", ((totalPages * pageSize) >> 20) + "MB");
            SetValue("usedMemory", (((totalPages - freePages) * pageSize) >> 20) + "MB");
            SetValue("freeMemory", ((freePages * pageSize) >> 20) + "MB");
            SetValue("totalPages", totalPages + " (" + pageSize + "B)");
            SetValue("freePages", freePages.ToString());
            SetValue("liveHeap", GC.GetTotalMemory(false) + "B");
            SetValue("gcHeapSize", _gcHeapSize + "B");
            SetValue("gcCommitted", _gcCommitted + "B");
            SetValue("gcFragmented", _gcFragmented + "B");
            SetValue("gcPinnedObjects", _gcPinnedObjects.ToString());
            SetValue("collections", collections.ToString());
            SetValue("objectsFreed", MemoryInfo.TotalObjectsFreed.ToString());
            SetValue("gcTime", MemoryInfo.GcTimePercent + "%");
            SetValue("freeCount", Kernel.FreeCount.ToString());
        }

        private void SetValue(string id, string value)
        {
            Find<Label>(id).Text = value;
        }
    }
}
