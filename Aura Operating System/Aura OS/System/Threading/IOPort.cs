using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;

namespace Aura_OS.System.Threading
{
    public static class IOPort
    {
        public static void outb(int port, byte data)
        {
            outb((ushort)port, data);
        }

        [PlugMethod(PlugRequired = true)]
        public static void outb(ushort port, byte data) { }

        public static byte inb(int port)
        {
            return inb((ushort)port);
        }

        [PlugMethod(PlugRequired = true)]
        public static byte inb(ushort port) { return 0; }

        [PlugMethod(PlugRequired = true)]
        public static void outw(ushort port, ushort data) { }

        public static ushort inw(int port)
        {
            return inw((ushort)port);
        }

        [PlugMethod(PlugRequired = true)]
        public static ushort inw(ushort port) { return 0; }

        [PlugMethod(PlugRequired = true)]
        public static uint ind(ushort port) { return 0; }

        [PlugMethod(PlugRequired = true)]
        public static void outd(ushort port, uint data) { }

        public static void send_eoi(uint irq_no)
        {
            if (irq_no > 8)
            {
                outb(0xA0, 0x20);
            }
            outb(0x20, 0x20);
        }

        [Plug(Target = typeof(IOPort))]
        public class IOPortPlug
        {
            [PlugMethod(Assembler = typeof(asmOUTB))]
            public static void outb(ushort port, byte data) { }

            public class asmOUTB : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.WriteToPortDX(XSRegisters.AL);
                }
            }

            [PlugMethod(Assembler = typeof(asmINB))]
            public static byte inb(ushort port) { return 0; }

            public class asmINB : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.Set(XSRegisters.EAX, 0);
                    XS.ReadFromPortDX(XSRegisters.AL);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmOUTB))]
            public static void outw(ushort port, ushort data) { }

            public class asmOUTW : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.WriteToPortDX(XSRegisters.AX);
                }
            }

            [PlugMethod(Assembler = typeof(asmINW))]
            public static ushort inw(ushort port) { return 0; }

            public class asmINW : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.Set(XSRegisters.EAX, 0);
                    XS.ReadFromPortDX(XSRegisters.AX);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmOUTD))]
            public static void outd(ushort port, uint data) { }

            public class asmOUTD : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.WriteToPortDX(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmIND))]
            public static uint ind(ushort port) { return 0; }

            public class asmIND : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                    XS.Set(XSRegisters.EAX, 0);
                    XS.ReadFromPortDX(XSRegisters.EAX);
                    XS.Push(XSRegisters.EAX);
                }
            }
        }
    }
}
