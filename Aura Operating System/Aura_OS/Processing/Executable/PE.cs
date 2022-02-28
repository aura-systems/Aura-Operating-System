using Aura_OS.Processing;
using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura_OS.Processing.Executable
{
    public unsafe class PE32
    {
        public static byte* ProgramAdress = null;

        public byte[] data;
        public byte[] text;
        public byte[] managed;

        int p = 0;
        uint address = 0;
        uint managed_addr = 0;
        uint data_addr = 0;
        uint ib = 0;

        uint address_entrypoint = 0;

        private static List<Section> sections = new List<Section>();

        public PE32(byte[] file)
        {
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

                byte[] hdr = new byte[(sizeof(PeHeader))];
                int baseP = p;
                for (int i = 0; i < sizeof(PeHeader); i++)
                {
                    hdr[i] = file[baseP + i];
                    p++;
                }

                fixed (byte* ptr = hdr)
                {
                    var header = (PeHeader*)ptr;

                    Console.WriteLine("Arch=" + GetArchitecture(header->mMachine));
                    Console.WriteLine("Number of sections=" + header->mNumberOfSections);
                    Console.WriteLine("Number of symbols=" + header->mNumberOfSymbols);

                    byte[] ohdr = new byte[header->mSizeOfOptionalHeader];

                    baseP = p;
                    for (int i = 0; i < header->mSizeOfOptionalHeader; i++)
                    {
                        ohdr[i] = file[baseP + i];
                        p++;
                    }
                    fixed (byte* ptr2 = ohdr)
                    {
                        Pe32OptionalHeader* opt = (Pe32OptionalHeader*)ptr2;

                        byte[] tmp = new byte[40];
                        address = opt->mBaseOfCode;
                        data_addr = opt->mBaseOfData;
                        ib = opt->mImageBase;

                        address_entrypoint = opt->mAddressOfEntryPoint;

                        Console.WriteLine("Base of code=0x" + address.ToString("X"));
                        Console.WriteLine("Base of data=0x" + data_addr.ToString("X"));
                        Console.WriteLine("Image base=0x" + ib.ToString("X"));
                        Console.WriteLine("Address Entry Point=0x" + address_entrypoint.ToString("X"));

                        for (int s = 0; s < header->mNumberOfSections; s++)
                        {
                            fixed (byte* ptr3 = tmp)
                            {
                                baseP = p;
                                for (int i = 0; i < 40; i++)
                                {
                                    tmp[i] = file[baseP + i];
                                    p++;
                                }

                                SectionHeader* sec = (SectionHeader*)ptr3;

                                string name = "";
                                for (int c = 0; sec->Name[c] != 0; c++)
                                    name += ((char)sec->Name[c]).ToString();

                                Section section = new Section();
                                section.Name = name;
                                section.Address = (uint)sec->PointerToRawData;
                                section.VirtualAddress = (uint)sec->VirtualAddress;
                                section.RelocationCount = (uint)sec->NumberOfRelocations;
                                section.RelocationPtr = (uint)sec->PointerToRelocations;
                                section.Size = (uint)sec->SizeOfRawData;
                                sections.Add(section);

                                Console.WriteLine(name + "=0x" + ((int)sec->VirtualAddress).ToString("X"));
                            }
                        }

                        for (int i = 0; i < sections.Count; i++)
                        {
                            if (sections[i].Name == ".text")
                            {
                                text = new byte[sections[i].Size];
                                for (int b = 0; b < (int)sections[i].Size; b++)
                                {
                                    text[b] = file[sections[i].Address + b];
                                }
                            }
                            else if (sections[i].Name == ".data")
                            {
                                data = new byte[sections[i].Size];
                                for (int b = 0; b < (int)sections[i].Size; b++)
                                {
                                    data[b] = file[sections[i].Address + b];
                                }
                            }
                            else if (sections[i].Name.StartsWith(".managed"))
                            {
                                managed_addr = sections[i].VirtualAddress;
                                managed = new byte[sections[i].Size];
                                for (int b = 0; b < (int)sections[i].Size; b++)
                                {
                                    managed[b] = file[sections[i].Address + b];
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Start()
        {
            ProgramAdress = Heap.Alloc((uint)(ib + data_addr + data.Length));

            //relocate sections to virtual addresses
            var textBlock = new MemoryBlock((uint)(ProgramAdress + ib + address), (uint)text.Length);
            textBlock.Copy(text);
            var managedBlock = new MemoryBlock((uint)(ProgramAdress + ib + managed_addr), (uint)managed.Length);
            managedBlock.Copy(managed);
            var dataBlock = new MemoryBlock((uint)(ProgramAdress + ib + data_addr), (uint)data.Length);
            dataBlock.Copy(data);

            //call address entry point (should be Main in .managed section)
            Caller cl = new Caller();
            cl.CallCode((uint)(ProgramAdress + ib + address_entrypoint));
        }

        public string GetArchitecture(ushort arch)
        {
            switch (arch)
            {
                case 0x014c:
                    return "x86";
                case 0x0200:
                    return "Intel Itanium";
                case 0x8664:
                    return "x64";
                default:
                    return "Unknown architecture";
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

            public uint Address, VirtualAddress, Size, RelocationPtr, RelocationCount;
            public string Name;

            #endregion Fields
        }

        #endregion Nested Types
    }
}