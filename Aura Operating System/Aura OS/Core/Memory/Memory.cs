/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory class
* PROGRAMMERS:      Myvar (part of CosmosGL)
*/

using System.Collections.Generic;
using Cosmos.Core.Memory.Old;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace Aura_OS.Core
{
    public static unsafe class Memory
    {

        #region SSEMemCpy

        [PlugMethod(Assembler = typeof(AsmSSEEnable))]
        public static void SSEEnable()
        {
        }

        public class AsmSSEEnable : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov eax, 0x600");
                new LiteralAssemblerCode("mov cr4, eax");
                new LiteralAssemblerCode("mov eax, cr0");
                new LiteralAssemblerCode("and eax, 0xFFFFFFFB");
                new LiteralAssemblerCode("or eax, 2");
                new LiteralAssemblerCode("mov cr0, eax");
            }
        }

        [PlugMethod(Assembler = typeof(AsmSSEMemcpy))]
        public static void SSEMemcpy(uint* dst, uint* src, int len)
        {
        }

        public class AsmSSEMemcpy : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov edi, [esp + 4]");
                new LiteralAssemblerCode("mov esi, [esp + 8]");
                new LiteralAssemblerCode("mov ecx, [esp + 12]");
                new LiteralAssemblerCode("mov ecx, [esp+8]");
                new LiteralAssemblerCode(".loop:");
                new LiteralAssemblerCode("movdqu xmm0, [esi]");
                new LiteralAssemblerCode("movdqu xmm1, [esi + 0x10]");
                new LiteralAssemblerCode("movdqu xmm2, [esi + 0x20]");
                new LiteralAssemblerCode("movdqu xmm3, [esi + 0x30]");
                new LiteralAssemblerCode("movdqu xmm4, [esi + 0x40]");
                new LiteralAssemblerCode("movdqu xmm5, [esi + 0x50]");
                new LiteralAssemblerCode("movdqu xmm6, [esi + 0x60]");
                new LiteralAssemblerCode("movdqu xmm7, [esi + 0x70]");
                new LiteralAssemblerCode("movdqu [edi], xmm0");
                new LiteralAssemblerCode("movdqu [edi + 0x10], xmm1");
                new LiteralAssemblerCode("movdqu [edi + 0x20], xmm2");
                new LiteralAssemblerCode("movdqu [edi + 0x30], xmm3");
                new LiteralAssemblerCode("movdqu [edi + 0x40], xmm4");
                new LiteralAssemblerCode("movdqu [edi + 0x50], xmm5");
                new LiteralAssemblerCode("movdqu [edi + 0x60], xmm6");
                new LiteralAssemblerCode("movdqu [edi + 0x70], xmm7");
                new LiteralAssemblerCode("add esi, 128");
                new LiteralAssemblerCode("add edi, 128");
                new LiteralAssemblerCode("loop .loop");
            }
        }
        #endregion

        #region MemCpy

        [PlugMethod(Assembler = typeof(AsmCopyBytes))]
        public static void Memcpy(byte* dst, byte* src, int len)
        {
        }

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
        public static void Memcpy(uint* dst, uint* src, int len)
        {
        }

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
        public static void Memset(byte* dst, byte value, uint len)
        {
        }

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
        public static void Memset(uint* dst, uint value, uint len)
        {
        }

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

        #endregion

        /*
         * Becuse we the building cosmos heap does not have a Free, we need to make our own heap
         * In the hopes of saving time it will be a block style allocation system
         * becuase that is fast and easy to implment
         *
         */
        private class MemoryAllocationObject
        {
            public int TotalBlocks { get; set; }
            public int FirstBlock { get; set; }
        }


        private const int MaxBlocks = 4096;
        private static byte* _memoryBlocks = (byte*)Heap.MemAlloc(512 * MaxBlocks);
        private static bool[] _blockIndex = new bool[MaxBlocks];
        private static List<MemoryAllocationObject> _allocationIndex = new List<MemoryAllocationObject>();

        private static MemoryAllocationObject FindOpenSpace(int size)
        {
            var re = new MemoryAllocationObject();

            for (int i = 0; i < _blockIndex.Length; i++)
            {
                if (_blockIndex[i] == false)
                {
                    if (re.TotalBlocks * 512 >= size) break;
                    re.FirstBlock = i;
                }
                else
                {
                    re.TotalBlocks++;
                }
            }

            return re;
        }

        public static void* MemAlloc(int size)
        {
            /*
             * 1. find a qequnce of blocks the size of the maloc
             * 2. Registor the allocation into the Index and mark blocks as used
             * 3. return the offset
             
            var memalloc = FindOpenSpace(size);

            for (int i = memalloc.FirstBlock; i < memalloc.TotalBlocks; i++)
            {
                _blockIndex[i] = true;
            }

            _allocationIndex.Add(memalloc);

            return (void*) ((uint) _memoryBlocks + (memalloc.FirstBlock * 512));*/

            return (void*)Heap.MemAlloc((uint)size);
        }

        public static void Free(void* memblock)
        {
            /* 
              * 1. mark blocks in allocation as free
              * 2. remove allocation object


             var firstBlock = ((uint) memblock - (uint) _memoryBlocks) / 512;

             var remOffset = 0;

             for (var index = 0; index < _allocationIndex.Count; index++)
             {
                 var allocationObject = _allocationIndex[index];

                 if (allocationObject.FirstBlock == firstBlock)
                 {
                     for (int i = allocationObject.TotalBlocks; i < firstBlock; i++)
                     {
                         _blockIndex[i] = false;
                     }

                     remOffset = index;
                     break;
                 }
             }

             _allocationIndex.RemoveAt(remOffset);*/
        }
    }
}