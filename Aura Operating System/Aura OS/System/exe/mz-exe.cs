using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cosmos.IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using System.IO;
namespace Aura_OS.System.exe
{

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    unsafe struct MZ_Header
    {
        [FieldOffset(0)]
        public ushort Magic; //signature
        [FieldOffset(2)]
        public ushort ExtraBytes;
        [FieldOffset(4)]
        public ushort Pages;
        [FieldOffset(6)]
        public ushort RelocationItems;
        [FieldOffset(8)]
        public ushort HeaderSize;
        [FieldOffset(10)]
        public ushort MinAlloc;
        [FieldOffset(12)]
        public ushort MaxAlloc;
        [FieldOffset(14)]
        public ushort InitialSS;
        [FieldOffset(16)]
        public ushort InitialSP;
        [FieldOffset(18)]
        public ushort Checksum;
        [FieldOffset(20)]
        public ushort Entry; //inital cp
        [FieldOffset(22)]
        public ushort IntialCP;
        [FieldOffset(24)]
        public ushort RelocationTab;
        [FieldOffset(26)]
        public ushort Overlay;
        [FieldOffset(28)]
        public ushort OverlayInfo;
    }

    class RelocateInfo
    {
        public uint Offset;
        public uint Segment;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct RelocationStruct
    {
        //orginal is 0, 4
        [FieldOffset(2)]
        public uint Offset;
        [FieldOffset(2)]
        public uint Segment;
    }

    public unsafe class mz_exe
    {
        private byte[] code;
        private MZ_Header* header;
        private byte* rel_ptr;
        private List<RelocateInfo> symbolsToRelocate = new List<RelocateInfo>();
        public mz_exe(string file)
        {
            code = File.ReadAllBytes(file);

            fixed (byte* ptr = code)
            {
                header = (MZ_Header*)ptr;
                rel_ptr = (byte*)((uint)ptr + (uint)header->RelocationTab);
            }

            for (int i = 0; i < header->RelocationItems; i++)
            {
                RelocationStruct* rs = (RelocationStruct*)(i * 8);
                RelocateInfo ri = new RelocateInfo();
                ri.Offset = rs->Offset;
                ri.Segment = rs->Segment;
                symbolsToRelocate.Add(ri);
            }
        }

        private void Relocate(uint offset)
        {
            BinaryWriter bw = new BinaryWriter(MemoryStream(code));
            BinaryReader br = new BinaryReader(MemoryStream(code));
            for (int i = 0; i < symbolsToRelocate.Count; i++)
            {
                br.BaseStream.Position = (int)symbolsToRelocate[i].Offset;
                ushort val = BitConverter.ToUInt16(new byte[]
                {
                    br.ReadByte(),
                    br.ReadByte()
                });
            }

        }
        public void Execute()
        {
            Caller c = new Caller();
            c.CallCode(header->Entry);
        }

        public class Caller
        {
            [PlugMethod(Assembler = typeof(CallerPlug))]
            public void CallCode(uint address) { }
        }
        [Plug(Target = typeof(Caller))]
        public class CallerPlug : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
                XS.Call(XSRegisters.EAX);
            }
        }
    }
}
