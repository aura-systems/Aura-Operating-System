/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory class
* PROGRAMMERS:      Myvar
*/

using System.Collections.Generic;
using Cosmos.Core.Memory.Old;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace Aura_OS.Core
{
    public static unsafe class Memory
    {
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