using System;
using System.Collections.Generic;
using System.Text;

namespace Alve_OS_HAL
{
    public class CPU
    {
        public static uint getRam() { return Cosmos.Core.CPU.GetAmountOfRAM(); }
    }
}
