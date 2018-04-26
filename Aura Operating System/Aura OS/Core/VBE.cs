using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura_OS.Core
{
    public static class VBE
    {

        [StructLayout(LayoutKind.Explicit, Size = 54)]
        public struct ModeInfo
        {

            [FieldOffset(0)]
            public ushort attributes;
            [FieldOffset(2)]
            public byte windowA;
            [FieldOffset(3)]
            public byte windowB;
            [FieldOffset(4)]
            public ushort granularity;
            [FieldOffset(6)]
            public ushort windowSize;
            [FieldOffset(8)]
            public ushort segmentA;
            [FieldOffset(10)]
            public ushort segmentB;
            [FieldOffset(12)]
            public ulong winFuncPtr; // ptr to INT 0x10 Function 0x4F05 /
            [FieldOffset(20)]
            public ushort pitch; // bytes per scan line /
            [FieldOffset(22)]
            public ushort resolutionX;
            [FieldOffset(24)]
            public ushort resolutionY;        /* resolution */
            [FieldOffset(26)]
            public byte wChar;
            [FieldOffset(27)]
            public byte yChar;
            [FieldOffset(28)]
            public byte planes;
            [FieldOffset(29)]
            public byte bpp;
            [FieldOffset(30)]
            public byte banks; /* number of banks */
            [FieldOffset(31)]
            public byte memoryModel;
            [FieldOffset(32)]
            public byte bankSize;
            [FieldOffset(33)]
            public byte imagePages;
            [FieldOffset(34)]
            public byte reserved0;
            [FieldOffset(35)]
            public byte readMask;
            [FieldOffset(36)]
            public byte redPosition;            /* color masks */
            [FieldOffset(37)]
            public byte greenMask;
            [FieldOffset(38)]
            public byte greenPosition;
            [FieldOffset(39)]
            public byte blueMask;
            [FieldOffset(40)]
            public byte bluePosition;
            [FieldOffset(41)]
            public byte reservedMask;
            [FieldOffset(42)]
            public byte reservedPosition;
            [FieldOffset(43)]
            public byte directColorAttributes;
            [FieldOffset(44)]

            public uint physbase;                        /* pointer to LFB in LFB modes */
            [FieldOffset(48)]
            public uint offScreenMemOff;
            [FieldOffset(52)]
            public ushort offScreenMemSize;
            
            //public byte reserved1[206];
        }
    }

}
