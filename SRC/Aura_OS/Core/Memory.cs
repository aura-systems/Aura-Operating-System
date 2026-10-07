/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory Informations
* PROGRAMMER(S):    Arawn Davies <arawn.davies@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.Core
{
    public class Memory
    {
        // GEN3-GAP(meminfo): MemoryDiagnostics.TotalPages/RamSizeBytes only cover the largest usable
        // Limine memory-map region (the page allocator's pool), so the total reads below the
        // machine's installed RAM. All figures are in MB.

        public static uint TotalMemory
        {
            get
            {
                return (uint)((MemoryDiagnostics.TotalPages * MemoryDiagnostics.PageSizeBytes) >> 20);
            }
        }

        public uint FreePercentage;
        public uint UsedPercentage;
        public uint FreeMemory;

        public Memory()
        {
            this.Monitor();
        }

        public void Monitor()
        {
            uint total = TotalMemory;
            FreeMemory = GetFreeMemory();
            UsedPercentage = total == 0 ? 0 : (GetUsedMemory() * 100) / total;
            FreePercentage = 100 - UsedPercentage;
        } 

        public static uint GetFreeMemory()
        {
            return (uint)((MemoryDiagnostics.FreePages * MemoryDiagnostics.PageSizeBytes) >> 20);
        }

        public static uint GetUsedMemory()
        {
            return (uint)(((MemoryDiagnostics.TotalPages - MemoryDiagnostics.FreePages) * MemoryDiagnostics.PageSizeBytes) >> 20);
        }
    }
}