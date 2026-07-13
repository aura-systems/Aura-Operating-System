/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory Informations
* PROGRAMMER(S):    Arawn Davies <arawn.davies@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.Core.Memory;

namespace Aura_OS.Core
{
    public class Memory
    {
        public static uint TotalMemory = (uint)(PageAllocator.RamSize / div);
        public uint FreePercentage;
        public uint UsedPercentage = (GetUsedMemory() * 100) / TotalMemory;
        public uint FreeMemory = TotalMemory - GetUsedMemory();
        private const uint div = 1048576;

        public Memory()
        {
            this.Monitor();
        }

        public static void GetTotalMemory()
        {
            TotalMemory = (uint)(PageAllocator.RamSize / div) + 1;
        }

        public void Monitor()
        {
            GetTotalMemory();
            FreeMemory = TotalMemory - GetUsedMemory();
            UsedPercentage = (GetUsedMemory() * 100) / TotalMemory;
            FreePercentage = 100 - UsedPercentage;
        }

        public static uint GetFreeMemory()
        {
            return TotalMemory - GetUsedMemory();
        }

        public static uint GetUsedMemory()
        {
            // gen2 used CPU.GetEndOfKernel() as a heuristic; gen3 exposes real page accounting.
            ulong usedBytes = (PageAllocator.TotalPageCount - PageAllocator.FreePageCount) * PageAllocator.PageSize;
            return (uint)(usedBytes / div);
        }
    }
}
