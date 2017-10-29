using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Threading
{
    public static unsafe class GDT
    {
        public static uint highestGDTEntry;

        public static void Init()
        {
            var gp = Label.getGDTPointer();
            gp->limit = (ushort)((sizeof(Label.GDTEntry) * 6) - 1);
            gp->_base = (uint)Label.getGDT();
            SetGate(0, 0, 0, 0, 0);
            SetGate(1, 0, 0xFFFFFFFF, 0x9A, 0xCF);
            SetGate(2, 0, 0xFFFFFFFF, 0x92, 0xCF);
            SetGate(3, 0, 0xFFFFFFFF, 0xFA, 0xCF);
            SetGate(4, 0, 0xFFFFFFFF, 0xF2, 0xCF);
            UpdateTSS(5, 0x10, 0x0);
            GDTFlush();
            TSSFlush();
            highestGDTEntry++;
        }

        public static void SetGate(uint num, ulong _base, ulong limit, byte access, byte gran)
        {
            if (highestGDTEntry > num)
            {
                highestGDTEntry = num;
            }
            Label.GDTEntry* gdt = Label.getGDT();
            gdt[num].base_low = (ushort)(_base & 0xFFFF);
            gdt[num].base_middle = (char)((_base >> 16) & 0xFF);
            gdt[num].base_high = (char)((_base >> 24) & 0xFF);
            gdt[num].limit_low = (ushort)(limit & 0xFFFF);
            gdt[num].granularity = (char)((limit >> 16) & 0X0F);
            gdt[num].granularity |= (char)(gran & 0xF0);
            gdt[num].access = (char)access;
        }

        private static void UpdateTSS(uint num, ushort ss0, uint esp0)
        {
            uint _base = (uint)Label.getTSS();
            uint limit = _base + (uint)sizeof(Label.TSSEntry);

            SetGate(num, _base, limit, 0xE9, 0x00);

            Utils.memset((byte*)_base, 0x0, (uint)sizeof(Label.TSSEntry));
            Label.TSSEntry* tss_entry = Label.getTSS();
            tss_entry->SS0 = ss0;
            tss_entry->ESP0 = esp0;
            tss_entry->CS = 0x0b;
            tss_entry->SS =
                tss_entry->DS =
                tss_entry->ES =
                tss_entry->FS =
                tss_entry->GS = 0x13;
            tss_entry->IOPBOffset = (ushort)sizeof(Label.TSSEntry);
        }

        public static void SetKernelStack(uint stack)
        {
            Label.getTSS()->ESP0 = stack;
        }

        [PlugMethod(PlugRequired = true)]
        public static void GDTFlush() { }

        [PlugMethod(PlugRequired = true)]
        public static void TSSFlush() { } // add passing params

        [Plug(Target = typeof(GDT))]
        public class GDTPlug
        {
            [PlugMethod(Assembler = typeof(asmGDTFlush))]
            public static void GDTFlush() { }

            public class asmGDTFlush : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.ClearInterruptFlag();
                    XS.Set(XSRegisters.EAX, "_NATIVE_GDT_Pointer");
                    XS.LoadGdt(XSRegisters.EAX, isIndirect: true);
                    XS.Set(XSRegisters.EAX, XSRegisters.CR0);
                    XS.Or(XSRegisters.AL, 1);
                    XS.Set(XSRegisters.CR0, XSRegisters.EAX);
                    XS.JumpToSegment(0x08, "PMode");
                    XS.Label("PMode");
                }
            }

            [PlugMethod(Assembler = typeof(asmTSSFlush))]
            public static void TSSFlush() { }

            public class asmTSSFlush : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov ax, 0x2B");
                    new LiteralAssemblerCode("ltr ax");
                }
            }
        }
    }
}
