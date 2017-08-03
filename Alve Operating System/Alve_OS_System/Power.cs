using System;
using System.Collections.Generic;
using System.Text;

namespace Alve_OS_System
{
    public class Power
    {
        public static uint getRam() { return Alve_OS_HAL.CPU.getRam(); }
    }
}
