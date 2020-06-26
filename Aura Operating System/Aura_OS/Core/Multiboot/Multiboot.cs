/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Multiboot struct
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   https://www.gnu.org/software/grub/manual/multiboot/multiboot.html
*/

using System;
using System.Runtime.InteropServices;

namespace Aura_OS.Core
{

    public static class MultiBoot
    {
        [StructLayout(LayoutKind.Explicit, Size = 88)]
        public unsafe struct Header
        {
            [FieldOffset(0)]
            public uint Flags;
            [FieldOffset(4)]
            public uint mem_lower;
            [FieldOffset(8)]
            public uint mem_upper;
            [FieldOffset(12)]
            public uint boot_device;
            [FieldOffset(16)]
            public uint cmdline;
            [FieldOffset(20)]
            public uint mods_count;
            [FieldOffset(24)]
            public uint mods_addr;
            [FieldOffset(28)]
            public fixed uint syms[4];
            [FieldOffset(44)]
            public uint memMapLength;
            [FieldOffset(48)]
            public uint memMapAddress;
            [FieldOffset(52)]
            public uint drivesLength;
            [FieldOffset(56)]
            public uint drivesAddress;
            [FieldOffset(60)]
            public uint configTable;
            [FieldOffset(68)]
            public uint apmTable;
            [FieldOffset(72)]
            public uint vbeControlInfo;
            [FieldOffset(76)]
            public uint vbeModeInfo;
            [FieldOffset(80)]
            public uint vbeMode;
            [FieldOffset(82)]
            public uint vbeInterfaceSeg;
            [FieldOffset(84)]
            public uint vbeInterfaceOff;
            [FieldOffset(86)]
            public uint vbeInterfaceLength;
        }
    }

}