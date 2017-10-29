using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;

namespace Aura_OS.System.Threading
{
    public static class CPU
    {
        public enum EFlagsEnum : uint
        {
            Carry = 1,
            Parity = 1 << 2,
            AuxilliaryCarry = 1 << 4,
            Zero = 1 << 6,
            Sign = 1 << 7,
            Trap = 1 << 8,
            InterruptEnable = 1 << 9,
            Direction = 1 << 10,
            Overflow = 1 << 11,
            NestedTag = 1 << 14,
            Resume = 1 << 16,
            Virtual8086Mode = 1 << 17,
            AlignmentCheck = 1 << 18,
            VirtualInterrupt = 1 << 19,
            VirtualInterruptPending = 1 << 20,
            ID = 1 << 21
        }

        [PlugMethod(PlugRequired = true)]
        public static void Initialize() { }
        [PlugMethod(PlugRequired = true)]
        public static void DisableInts() { }
        [PlugMethod(PlugRequired = true)]
        public static void EnableInts() { }
        [PlugMethod(PlugRequired = true)]
        public static void Halt() { }
        [PlugMethod(PlugRequired = true)]
        public static uint ReadCR0() { return 0; }
        [PlugMethod(PlugRequired = true)]
        public static void WriteCR0(uint val) { }
        [PlugMethod(PlugRequired = true)]
        public static uint ReadCR2() { return 0; }
        [PlugMethod(PlugRequired = true)]
        public static uint ReadCR3() { return 0; }
        [PlugMethod(PlugRequired = true)]
        public static void WriteCR3(uint val) { }

        [Plug(Target = typeof(CPU))]
        public class CPUPlug
        {
            [PlugMethod(Assembler = typeof(asmInit))]
            public static void Initialize() { }

            public class asmInit : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.SSE.SSEInit();
                    XS.FPU.FloatInit();
                }
            }

            [PlugMethod(Assembler = typeof(asmDisInts))]
            public static void DisableInts() { }

            public class asmDisInts : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.DisableInterrupts();
                }
            }

            [PlugMethod(Assembler = typeof(asmEnInts))]
            public static void EnableInts() { }

            public class asmEnInts : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.EnableInterrupts();
                }
            }

            [PlugMethod(Assembler = typeof(asmHalt))]
            public static void Halt() { }

            public class asmHalt : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Halt();
                }
            }

            [PlugMethod(Assembler = typeof(asmReadCR0))]
            public static uint ReadCR0() { return 0; }

            public class asmReadCR0 : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.CR0);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmWriteCR0))]
            public static void WriteCR0(uint val) { }

            public class asmWriteCR0 : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceIsIndirect: true, sourceDisplacement: 8);
                    XS.Set(XSRegisters.CR0, XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmReadCR2))]
            public static uint ReadCR2() { return 0; }

            public class asmReadCR2 : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.CR2);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmReadCR3))]
            public static uint ReadCR3() { return 0; }

            public class asmReadCR3 : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.CR3);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmWriteCR3))]
            public static void WriteCR3(uint val) { }

            public class asmWriteCR3 : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8, sourceIsIndirect: true);
                    XS.Set(XSRegisters.CR3, XSRegisters.EAX);
                }
            }
        }
    }
}
