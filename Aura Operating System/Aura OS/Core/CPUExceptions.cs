using System;
using System.Collections.Generic;
using System.Text;
using static Cosmos.Core.INTs;

namespace Aura_OS.Core
{
    public class CPUExceptions
    {
        static bool already = false;
        private static string[] errs = new string[] { "DIVIDE_BY_ZERO", "SINGLE_STEP", "NON_MASKABLE_INTERRUPT", "BREAK_FLOW", "OVERFLOW", "NULL", "INVALID_OPCODE", "", "DOUBLE_FAULT_EXCEPTION", "INVALID_TSS", "SEGMENT_NOT_PRESENT", "STACK_EXCEPTION", "GENERAL_PROTECTION_FAULT" };

        public unsafe static void SWI(ref IRQContext aContext)
        {

        }
            public static void HandleInterrupt_Default(ref Cosmos.Core.INTs.IRQContext aContext)
        {

            SWI(ref aContext);

        }
        public static void HandleInterrupt_00(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[0]);
        }

        public static void HandleInterrupt_01(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[1]);
        }
        public static void HandleInterrupt_02(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[2]);
        }
        public static void HandleInterrupt_03(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[3]);
        }
        public static void HandleInterrupt_04(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[4]);
        }
        public static void HandleInterrupt_05(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[5]);
        }
        public static void HandleInterrupt_06(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[6]);
        }
        public static void HandleInterrupt_07(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[7]);
        }
        public static void HandleInterrupt_08(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[8]);
        }
        public static void HandleInterrupt_09(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[9]);
        }
        public static void HandleInterrupt_0A(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[10]);
        }
        public static void HandleInterrupt_0B(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[11]);

        }
        public static void HandleInterrupt_0C(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[12]);
        }
        public static void HandleInterrupt_0D(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[13]);
        }
        public static void HandleInterrupt_0E(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[14]);
        }
        public static void HandleInterrupt_0F(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            System.Crash.StopKernel(errs[15]);
        }
    }
}
