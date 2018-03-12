using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace Aura_OS.System.GUI.Drivers
{
    public unsafe class Memory
    {
        [PlugMethod(Assembler = typeof(AsmCopyBytes))]
        public static void Memcpy(byte* dst, byte* src, int len) { }

        public class AsmCopyBytes : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov esi, [esp+12]");
                new LiteralAssemblerCode("mov edi, [esp+16]");
                new LiteralAssemblerCode("cld");
                new LiteralAssemblerCode("mov ecx, [esp+8]");
                new LiteralAssemblerCode("rep movsb");
            }
        }

        [PlugMethod(Assembler = typeof(AsmCopyUint))]
        public static void Memcpy(uint* dst, uint* src, int len) { }

        public class AsmCopyUint : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov esi, [esp+12]");
                new LiteralAssemblerCode("mov edi, [esp+16]");
                new LiteralAssemblerCode("cld");
                new LiteralAssemblerCode("mov ecx, [esp+8]");
                new LiteralAssemblerCode("rep movsd");
            }
        }

        [PlugMethod(Assembler = typeof(AsmSetByte))]
        public static void Memset(byte* dst, byte value, uint len) { }

        public class AsmSetByte : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov al, [esp+12]");
                new LiteralAssemblerCode("mov edi, [esp+16]");
                new LiteralAssemblerCode("cld");
                new LiteralAssemblerCode("mov ecx, [esp+8]");
                new LiteralAssemblerCode("rep stosb");
            }
        }

        [PlugMethod(Assembler = typeof(AsmSetUint))]
        public static void Memset(uint* dst, uint value, uint len) { }

        public class AsmSetUint : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov eax, [esp+12]");
                new LiteralAssemblerCode("mov edi, [esp+16]");
                new LiteralAssemblerCode("cld");
                new LiteralAssemblerCode("mov ecx, [esp+8]");
                new LiteralAssemblerCode("rep stosd");
            }
        }
    }
}
