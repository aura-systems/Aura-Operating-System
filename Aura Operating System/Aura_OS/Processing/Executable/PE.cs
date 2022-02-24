using Aura_OS.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura_OS.Processing.Executable
{
    public unsafe class PE32
    {
        public byte[] data;
        public byte[] text;

        private static List<Section> sections = new List<Section>();

        public PE32(byte[] file)
        {
            int p = 0;
            uint address = 0;
            uint data_addr = 0;
            uint ib = 0;

            for (int i = 0; i < file.Length; i++)
            {
                p = i;
                if (file[i] == (byte)'P' && file[i + 1] == (byte)'E')
                    break;
            }

            if (p == file.Length - 1)
            {
                Console.WriteLine("Not a Portable Executable.");
            }
            else
            {
                Console.WriteLine("Start: " + p.ToString());
            } 
        }

        #region Nested Types

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        unsafe struct Pe32OptionalHeader
        {
            [FieldOffset(0)]
            public ushort mMagic; // 0x010b - PE32, 0x020b - PE32+ (64 bit)
            [FieldOffset(2)]
            public byte mMajorLinkerVersion;
            [FieldOffset(3)]
            public byte mMinorLinkerVersion;
            [FieldOffset(4)]
            public uint mSizeOfCode;
            [FieldOffset(8)]
            public uint mSizeOfInitializedData;
            [FieldOffset(12)]
            public uint mSizeOfUninitializedData;
            [FieldOffset(16)]
            public uint mAddressOfEntryPoint;
            [FieldOffset(20)]
            public uint mBaseOfCode;
            [FieldOffset(24)]
            public uint mBaseOfData;
            [FieldOffset(28)]
            public uint mImageBase;
            [FieldOffset(32)]
            public uint mSectionAlignment;
            [FieldOffset(36)]
            public uint mFileAlignment;
            [FieldOffset(40)]
            public ushort mMajorOperatingSystemVersion;
            [FieldOffset(42)]
            public ushort mMinorOperatingSystemVersion;
            [FieldOffset(44)]
            public ushort mMajorImageVersion;
            [FieldOffset(46)]
            public ushort mMinorImageVersion;
            [FieldOffset(48)]
            public ushort mMajorSubsystemVersion;
            [FieldOffset(50)]
            public ushort mMinorSubsystemVersion;
            [FieldOffset(52)]
            public uint mWin32VersionValue;
            [FieldOffset(56)]
            public uint mSizeOfImage;
            [FieldOffset(60)]
            public uint mSizeOfHeaders;
            [FieldOffset(64)]
            public uint mCheckSum;
            [FieldOffset(68)]
            public ushort mSubsystem;
            [FieldOffset(70)]
            public ushort mDllCharacteristics;
            [FieldOffset(72)]
            public uint mSizeOfStackReserve;
            [FieldOffset(76)]
            public uint mSizeOfStackCommit;
            [FieldOffset(80)]
            public uint mSizeOfHeapReserve;
            [FieldOffset(84)]
            public uint mSizeOfHeapCommit;
            [FieldOffset(88)]
            public uint mLoaderFlags;
            [FieldOffset(92)]
            public uint mNumberOfRvaAndSizes;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        unsafe struct PeHeader
        {
            [FieldOffset(0)]
            public uint mMagic;
            [FieldOffset(4)]
            public ushort mMachine;
            [FieldOffset(6)]
            public ushort mNumberOfSections;
            [FieldOffset(8)]
            public uint mTimeDateStamp;
            [FieldOffset(12)]
            public uint mPointerToSymbolTable;
            [FieldOffset(16)]
            public uint mNumberOfSymbols;
            [FieldOffset(20)]
            public ushort mSizeOfOptionalHeader;
            [FieldOffset(22)]
            public ushort mCharacteristics;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        struct SectionHeader
        {
            [FieldOffset(0)]
            public fixed byte Name[8];
            [FieldOffset(8)]
            public uint PhysicalAddress;
            [FieldOffset(12)]
            public uint VirtualAddress;
            [FieldOffset(16)]
            public uint SizeOfRawData;
            [FieldOffset(20)]
            public uint PointerToRawData;
            [FieldOffset(24)]
            public uint PointerToRelocations;
            [FieldOffset(28)]
            public uint PointerToLinenumbers;
            [FieldOffset(32)]
            public ushort NumberOfRelocations;
            [FieldOffset(34)]
            public ushort NumberOfLinenumbers;
            [FieldOffset(36)]
            public uint Characteristics;
        }

        public class Section
        {
            #region Fields

            public uint Address, Size, RelocationPtr, RelocationCount;
            public string Name;

            #endregion Fields
        }

        #endregion Nested Types
    }
}