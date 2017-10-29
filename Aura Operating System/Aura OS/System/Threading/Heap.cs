using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Threading
{
    public static unsafe class Heap
    {
        public const int totalPagesForDirectory = 20;

        public static SpinLock mutex = new SpinLock();
        public static DirectoryHeader* directory = null;
        public static uint entryCount;
        public static uint heapPointer = 0;
        public static uint pageSpace = 0;
        public static int currentEntry = 0;

        public static int totalAllocations = 0;

        public static void Init()
        {
            directory = (DirectoryHeader*)PageManager.GetFreePage(totalPagesForDirectory);
            directory->location = (uint)directory;
            entryCount = (uint)((4096 * totalPagesForDirectory) - sizeof(DirectoryHeader));
            for (int i = 0; i < entryCount; i++)
            {
                var entry = directory->GetEntry(i);
                entry->used = false;
            }
            Console.Write("Total System Memory ");
            Console.Write((int)Multiboot.GetTotalMemory());
            Console.NewLine();

            Console.Write("Useable Memory ");
            Console.Write(Multiboot.GetTotalMemory() - (heapPointer / 1024 / 1024));
            Console.NewLine();

        }

        public static uint alloc(uint size)
        {
            return alloc(size, false);
        }

        public static uint alloc(uint size, bool allign)
        {
            return internalAllocation(size, allign);
        }

        private static uint internalAllocation(uint size, bool allign)
        {
            totalAllocations++;
            tryAgain:;
            uint returnable = 0;
            if (Settings.HeapUseBasic || entryCount == 0)
            {
                if (heapPointer == 0)
                {
                    heapPointer = Utils.allign4K(Multiboot.GetEndOfKernel()) + 4096;
                }
                if (allign)
                {
                    heapPointer = Utils.allign4K(heapPointer) + 4096;
                }
                uint tmp = heapPointer;
                heapPointer += size;
                returnable = tmp;
                goto allocationEnd;
            }
            mutex.Lock();
            if (currentEntry >= entryCount)
            {
                currentEntry = 0;
            }
            var entry = directory->GetEntry(currentEntry);
            if (entry->used)
            {
                currentEntry++;
                goto tryAgain;
            }
            if (!allign && size < 4096)
            {
                if (entry->size <= size && entry->location != 0)
                {
                    entry->used = true;
                    returnable = entry->location;
                    goto allocationEnd;
                }
                if (size > pageSpace)
                {
                    pageSpace = 4096;
                    heapPointer = PageManager.GetFreePage();
                    if (heapPointer == 0)
                    {
                        // need to to a GC scan and free some pages
                        Console.WriteLine("RAN OUT OF RAM");
                       // ACPI.Shutdown(); Ask later I may have a better driver.
                        Cosmos.HAL.Power.ACPIShutdown();
                        while (true) { }
                    }
                }
                pageSpace -= size;
                entry->size = size;
                entry->location = heapPointer;
                heapPointer += size;
                entry->used = true;
                returnable = entry->location;
                goto allocationEnd;
            }
            else
            {
                int num = (int)(size / 4096);
                int remainder = (int)(size % 4096);
                if (remainder > 0)
                {
                    num++;
                }
                entry->used = true;
                entry->location = PageManager.GetFreePage(num);
                entry->size = (uint)num * 4096;
                returnable = entry->location;
                goto allocationEnd;
            }
            allocationEnd:;
            Utils.memset((byte*)returnable, 0, size);
            mutex.Unlock();
            return returnable;
        }

        public static void free(object obj)
        {
            free((uint*)Utils.getPointer(obj));
        }

        public static void free(void* ptr)
        {
            return;
            if (!Settings.HeapUseBasic)
            {
                for (int i = 0; i < entryCount; i++)
                {
                    var entry = directory->GetEntry(i);
                    if (entry->used)
                    {
                        if (entry->location == (uint)ptr)
                        {
                            int remainder = (int)(entry->size % 4096);
                            if (remainder == 0)
                            {
                                currentEntry = 0;
                                for (uint a = 0; a < entry->size; a += 4096)
                                {
                                    PageManager.FreePage(entry->location + a);
                                }
                                entry->location = 0;
                                entry->size = 0;
                            }
                            entry->used = false;
                        }
                    }
                }
            }
        }

        public struct DirectoryHeader
        {
            public uint location;
            public EntryHeader* GetEntry(int index)
            {
                byte* ptr = (byte*)location;
                return (EntryHeader*)(ptr + (sizeof(EntryHeader) * index));

            }
        }

        public struct EntryHeader
        {
            public bool used;
            public uint location;
            public uint size;
        }

       // [Plug(TargetName = "Cosmos.IL2CPU.GCImplementation, Cosmos.IL2CPU")]
       // public static class GCImplementionImpl
      //  {
        //    public static uint AllocNewObject(uint aSize)
        //    {
        //        return alloc(aSize);
        //    }
      //  }
    }
}