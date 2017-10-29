using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public static unsafe class PageManager
    {
        public static byte[] pageStates;
        public static uint currentPage;

        public static void Init()
        {
            pageStates = new byte[((Multiboot.GetTotalMemory() * 1024 * 1024) / 4096) / 8];

            for (uint i = 0; i < Multiboot.GetTotalMemory() * 1024 * 1024; i += 4096)
            {
                if (i <= Heap.heapPointer)
                {
                    Set(i);
                }
                else
                {
                    Clear(i);
                }
            }
        }

        private static long AddressToEntry(long address)
        {
            return address / 4096;
        }

        private static void Set(long entry)
        {
            entry = AddressToEntry(entry);
            pageStates[entry / 8] = (byte)(pageStates[entry / 8] | (1 << ((int)entry % 8)));
        }

        private static void Clear(long entry)
        {
            entry = AddressToEntry(entry);
            pageStates[entry / 8] = (byte)(pageStates[entry / 8] & ~(1 << ((int)entry % 8)));
        }

        private static bool IsSet(long entry)
        {
            entry = AddressToEntry(entry);
            return (pageStates[entry / 8] & (1 << ((int)entry % 8))) != 0;
        }

        public static uint GetFreePage()
        {
            return GetFreePage(1);
        }

        public static uint GetFreePage(int count)
        {
            uint startedAt = currentPage;
            bool skip = true;
            while (true)
            {
                for (; currentPage < Multiboot.GetTotalMemory() * 1024 * 1024; currentPage += 4096)
                {
                    if (startedAt == currentPage)
                    {
                        if (skip)
                        {
                            skip = false;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    if (!IsSet(currentPage))
                    {
                        int f = 0;
                        for (f = 0; f < count; f++)
                        {
                            if (IsSet(currentPage + (f * 4096)))
                            {
                                break;
                            }
                        }
                        if (f >= count)
                        {
                            for (f = 0; f < count; f++)
                            {
                                Set(currentPage + (f * 4096));
                            }
                            Utils.memset((byte*)(currentPage), 0, (uint)(count * 4096));
                            return currentPage;
                        }
                    }
                }
                currentPage = 0;
            }
        }

        public static void FreePage(uint address)
        {
            currentPage = 0;
            Clear(address);
        }
    }
}
