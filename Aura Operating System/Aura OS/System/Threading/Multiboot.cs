using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Threading
{
    public static unsafe class Multiboot
    {
        [PlugMethod(PlugRequired = true)]
        public static multiboot* getMultiboot() { return null; }

        [Plug(Target = typeof(Multiboot))]
        public class MultibootPlug
        {
            [PlugMethod(Assembler = typeof(asmGetMB))]
            public static multiboot* getMultiboot() { return null; }

            public class asmGetMB : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Push("MultiBootInfo_Structure");
                }
            }
        }

        [PlugMethod(Assembler = typeof(MultibootGetKernelEnd))]
        public static uint GetEndOfKernel() { return 0; }

        [Plug(Target = typeof(Multiboot))]
        public class MultibootGetKernelEnd : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Push("_end_code");
            }
        }

        [PlugMethod(Assembler = typeof(MultibootGetTotalMemory))]
        public static uint GetTotalMemory() { return 0; }

        [Plug(Target = typeof(Multiboot))]
        public class MultibootGetTotalMemory : AssemblerMethod
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
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct multiboot_memory_map
    {
        [FieldOffset(0)]
        public uint size;
        [FieldOffset(4)]
        public uint base_addr_low;
        [FieldOffset(8)]
        public uint base_addr_high;
        [FieldOffset(12)]
        public uint length_low;
        [FieldOffset(16)]
        public uint length_high;
        [FieldOffset(20)]
        public uint type;
    }

    [StructLayout(LayoutKind.Explicit, Size = 92)]
    public struct multiboot
    {
        [FieldOffset(0)]
        public uint flags;
        [FieldOffset(4)]
        public uint mem_lower;
        [FieldOffset(8)]
        public uint mem_upper;
        [FieldOffset(12)]
        public uint boot_device;
        [FieldOffset(14)]
        public uint cmdline;
        [FieldOffset(16)]
        public uint mods_count;
        [FieldOffset(20)]
        public uint mods_addr;
        [FieldOffset(24)]
        public uint num;
        [FieldOffset(28)]
        public uint size;
        [FieldOffset(32)]
        public uint addr;
        [FieldOffset(36)]
        public uint shndx;
        [FieldOffset(40)]
        public uint mmap_length;
        [FieldOffset(44)]
        public uint mmap_addr;
        [FieldOffset(48)]
        public uint drives_length;
        [FieldOffset(52)]
        public uint drives_addr;
        [FieldOffset(56)]
        public uint config_table;
        [FieldOffset(60)]
        public uint boot_loader_name;
        [FieldOffset(64)]
        public uint apm_table;
        [FieldOffset(68)]
        public uint vbe_control_info;
        [FieldOffset(72)]
        public uint vbe_mode_info;
        [FieldOffset(76)]
        public uint vbe_mode;
        [FieldOffset(80)]
        public uint vbe_interface_seg;
        [FieldOffset(84)]
        public uint vbe_interface_off;
        [FieldOffset(88)]
        public uint vbe_interface_len;
    }
}
