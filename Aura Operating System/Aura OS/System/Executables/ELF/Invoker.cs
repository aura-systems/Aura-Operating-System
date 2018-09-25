using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core.Memory.Old;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace CosmosELFCore
{
    public static unsafe class Invoker
    {
        public static uint Offset;
        public static uint eax, ebx, ecx, edx, esi, edi, esp, ebp;
        public static uint* Stack = (uint*)Heap.MemAlloc(1024);

        public static void Dump()
        {
            Console.WriteLine(
                $"eax:{eax}, ebx:{ebx}, ecx:{ecx}, edx:{edx}, esi:{esi}, edi:{edi}, esp: {esp}, ebp: {ebp}");
            for (int i = 0; i < 512; i++)
            {
                Console.Write(((byte*)Stack)[i] + " ");
            }
        }

        [PlugMethod(Assembler = typeof(PlugStoreState))]
        public static void StoreState()
        {
            eax = 0;
            ebx = 0;
            ecx = 0;
            edx = 0;
            esi = 0;
            edi = 0;
            esp = 0;
            ebp = 0;
        }

        [PlugMethod(Assembler = typeof(PlugLoadState))]
        public static void LoadState()
        {
        }

        [PlugMethod(Assembler = typeof(PlugCall))]
        public static void CallCode()
        {
        }
    }

    [Plug(Target = typeof(Invoker))]
    public class PlugStoreState : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_eax], eax");
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_ebx], ebx");
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_ecx], ecx");
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_edx], edx");
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_edi], edi");
        }
    }


    [Plug(Target = typeof(Invoker))]
    public class PlugLoadState : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new LiteralAssemblerCode("mov eax, [static_field__CosmosELFCore_Invoker_eax]");
            new LiteralAssemblerCode("mov ebx, [static_field__CosmosELFCore_Invoker_ebx]");
            new LiteralAssemblerCode("mov ecx, [static_field__CosmosELFCore_Invoker_ecx]");
            new LiteralAssemblerCode("mov edx, [static_field__CosmosELFCore_Invoker_edx]");
            new LiteralAssemblerCode("mov edi, [static_field__CosmosELFCore_Invoker_edi]");
        }
    }


    [Plug(Target = typeof(Invoker))]
    public class PlugCall : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_esp], esp");
            new LiteralAssemblerCode("mov [static_field__CosmosELFCore_Invoker_ebp], ebp");

            new LiteralAssemblerCode("mov eax, [static_field__CosmosELFCore_Invoker_Stack]");
            new LiteralAssemblerCode("add eax, 50");
            new LiteralAssemblerCode("mov esp, eax");
            new LiteralAssemblerCode("mov ebp, eax");
            new LiteralAssemblerCode("mov eax, [static_field__CosmosELFCore_Invoker_Offset]");
            new LiteralAssemblerCode("Call eax");

            new LiteralAssemblerCode("mov ecx, [static_field__CosmosELFCore_Invoker_Stack]");
            new LiteralAssemblerCode("mov dword [ecx], eax");

            new LiteralAssemblerCode("mov esp, [static_field__CosmosELFCore_Invoker_esp]");
            new LiteralAssemblerCode("mov ebp, [static_field__CosmosELFCore_Invoker_ebp]");
        }
    }
}
