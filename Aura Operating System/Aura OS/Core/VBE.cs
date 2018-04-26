using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.Core
{
    public static class VBE
    {
        
        public struct ModeInfo
        {
            public ushort attributes;
            public byte windowA, windowB;
            public ushort granularity;
            public ushort windowSize;
            public ushort segmentA, segmentB;
            public uint winFuncPtr; // ptr to INT 0x10 Function 0x4F05 /
            public ushort pitch; // bytes per scan line /
            public ushort resolutionX, resolutionY;        /* resolution */
            public byte wChar, yChar, planes, bpp, banks; /* number of banks */
            public byte memoryModel, bankSize, imagePages;
            public byte reserved0;

            public byte readMask, redPosition;            /* color masks */
            public byte greenMask, greenPosition;
            public byte blueMask, bluePosition;
            public byte reservedMask, reservedPosition;
            public byte directColorAttributes;

            public uint physbase;                        /* pointer to LFB in LFB modes */
            public uint offScreenMemOff;
            public ushort offScreenMemSize;
            //public byte reserved1[206];
        }
    }

}
