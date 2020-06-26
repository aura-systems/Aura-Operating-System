/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory Informations
* PROGRAMMER(S):    Arawn Davies <arawn.davies@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;

namespace Aura_OS.Core
{
    public class MemoryManager
    {
        public static uint GetUsedMemory()
        {
            uint UsedRAM = CPU.GetEndOfKernel() + 1024;
            return UsedRAM / div;
        }
        public static uint TotalMemory = CPU.GetAmountOfRAM();
        public uint FreePercentage;
        public uint UsedPercentage = (GetUsedMemory() * 100) / TotalMemory;
        public uint FreeMemory = TotalMemory - GetUsedMemory();
        private const uint div = 1048576;

        public static void GetTotalMemory()
        {
            TotalMemory = CPU.GetAmountOfRAM() + 1;
        }
        public void Monitor()
        {
            GetTotalMemory();
            FreeMemory = TotalMemory - GetUsedMemory();
            UsedPercentage = (GetUsedMemory() * 100) / TotalMemory;
            FreePercentage = 100 - UsedPercentage;
        }
        public MemoryManager()
        {
            this.Monitor();
        }

        public static uint GetFreeMemory()
        {
            return TotalMemory - GetUsedMemory();
        }
    }
}
