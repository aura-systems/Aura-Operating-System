using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Threading
{
    public static unsafe class Label
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct GDTEntry
        {
            [FieldOffset(0)]
            public ushort limit_low;
            [FieldOffset(2)]
            public ushort base_low;
            [FieldOffset(4)]
            public char base_middle;
            [FieldOffset(5)]
            public char access;
            [FieldOffset(6)]
            public char granularity;
            [FieldOffset(7)]
            public char base_high;
        }

        [StructLayout(LayoutKind.Explicit, Size = 6)]
        public struct GDTTable
        {
            [FieldOffset(0)]
            public ushort limit;
            [FieldOffset(2)]
            public uint _base;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct IDTEntry
        {
            [FieldOffset(0)]
            public ushort base_low;
            [FieldOffset(2)]
            public ushort sel;
            [FieldOffset(4)]
            public byte zero;
            [FieldOffset(5)]
            public byte flags;
            [FieldOffset(6)]
            public ushort base_high;
        }

        [StructLayout(LayoutKind.Explicit, Size = 6)]
        public struct IDTTable
        {
            [FieldOffset(0)]
            public ushort limit;
            [FieldOffset(2)]
            public uint _base;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x68)]
        public struct TSSEntry
        {
            [FieldOffset(0)]
            public ushort Link;
            [FieldOffset(4)]
            public uint ESP0;
            [FieldOffset(8)]
            public ushort SS0;
            [FieldOffset(12)]
            public uint ESP1;
            [FieldOffset(16)]
            public ushort SS1;
            [FieldOffset(20)]
            public uint ESP2;
            [FieldOffset(24)]
            public ushort SS2;
            [FieldOffset(28)]
            public uint CR3;
            [FieldOffset(32)]
            public uint EIP;
            [FieldOffset(36)]
            public uint EFlags;
            [FieldOffset(40)]
            public uint EAX;
            [FieldOffset(44)]
            public uint ECX;
            [FieldOffset(48)]
            public uint EDX;
            [FieldOffset(52)]
            public uint EBX;
            [FieldOffset(56)]
            public uint ESP;
            [FieldOffset(60)]
            public uint EBP;
            [FieldOffset(64)]
            public uint ESI;
            [FieldOffset(68)]
            public uint EDI;
            [FieldOffset(72)]
            public ushort ES;
            [FieldOffset(76)]
            public ushort CS;
            [FieldOffset(80)]
            public ushort SS;
            [FieldOffset(84)]
            public ushort DS;
            [FieldOffset(88)]
            public ushort FS;
            [FieldOffset(92)]
            public ushort GS;
            [FieldOffset(96)]
            public ushort LDTR;
            [FieldOffset(102)]
            public ushort IOPBOffset;
        }

        public struct IRQRegs
        {
            public uint gs, fs, es, ds;
            public uint edi, esi, ebp, esp, ebx, edx, ecx, eax;
            public uint int_no, err_code;
            public uint eip, cs, eflags, useresp, ss;
        }

        [PlugMethod(PlugRequired = true)]
        public static GDTTable* getGDTPointer() { return (GDTTable*)0; }

        [PlugMethod(PlugRequired = true)]
        public static IDTTable* getIDTPointer() { return (IDTTable*)0; }

        [PlugMethod(PlugRequired = true)]
        public static GDTEntry* getGDT() { return (GDTEntry*)0; }

        [PlugMethod(PlugRequired = true)]
        public static IDTEntry* getIDT() { return (IDTEntry*)0; }

        [PlugMethod(PlugRequired = true)]
        public static TSSEntry* getTSS() { return (TSSEntry*)0; }

        [PlugMethod(PlugRequired = true)]
        public static uint getEndOfKernel() { return 0; }

        [PlugMethod(PlugRequired = true)]
        public static uint getAmountOfRam() { return 0; }

        [PlugMethod(PlugRequired = true)]
        public static uint getExperiment() { return 0; }

        [Plug(Target = typeof(Label))]
        public class LabelsPlug
        {
            [PlugMethod(Assembler = typeof(asmGetEndOfRam))]
            public static uint getEndOfKernel() { return 0; }

            public class asmGetEndOfRam : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("_end_code");
                }
            }

            [PlugMethod(Assembler = typeof(asmGetAmountOfRam))]
            public static uint getAmountOfRam() { return 0; }

            public class asmGetAmountOfRam : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, "MultiBootInfo_Memory_High", sourceIsIndirect: true);
                    XS.Xor(XSRegisters.EDX, XSRegisters.EDX);
                    XS.Set(XSRegisters.ECX, 1024);
                    XS.Divide(XSRegisters.ECX);
                    XS.Add(XSRegisters.EAX, 1);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmGetExperiment))]
            public static uint getExperiment() { return 0; }

            public class asmGetExperiment : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.CR0);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmGetGDTPTR))]
            public static GDTTable* getGDTPointer() { return (GDTTable*)0; }

            public class asmGetGDTPTR : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("_NATIVE_GDT_Pointer");
                }
            }

            [PlugMethod(Assembler = typeof(asmGetIDTPTR))]
            public static IDTTable* getIDTPointer() { return (IDTTable*)0; }

            public class asmGetIDTPTR : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("[_NATIVE_IDT_Pointer]");
                }
            }

            [PlugMethod(Assembler = typeof(asmGetGDT))]
            public static GDTEntry* getGDT() { return (GDTEntry*)0; }

            public class asmGetGDT : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("_NATIVE_GDT_Contents");
                }
            }

            [PlugMethod(Assembler = typeof(asmGetIDT))]
            public static IDTEntry* getIDT() { return (IDTEntry*)0; }

            public class asmGetIDT : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("_NATIVE_IDT_Contents");
                }
            }

            [PlugMethod(Assembler = typeof(asmGetTSS))]
            public static TSSEntry* getTSS() { return (TSSEntry*)0; }

            public class asmGetTSS : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("_NATIVE_TSS_Contents");
                }
            }
        }
    }
}