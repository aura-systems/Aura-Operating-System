using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.Core
{
    public static unsafe class Blend
    {
        // [PlugMethod(Assembler = typeof(AsmBlend))]
        public static void BlendSurfaceInplace(uint* background, uint* foreground, int w, int h)
        {
            for (int i = 0; i < w * h; i++)
            {
                uint src = foreground[i];
                uint bg = background[i];

                uint a = src >> 24;
                if (0 == a)
                    continue;

                uint rb = (((src & 0x00ff00ff) * a) +
                           ((bg & 0x00ff00ff) * (0xff - a))) & 0xff00ff00;
                uint g = (((src & 0x0000ff00) * a) +
                          ((bg & 0x0000ff00) * (0xff - a))) & 0x00ff0000;
                background[i] = (src & 0xff000000) | ((rb | g) >> 8);
            }
        }

        /* public class AsmBlend : AssemblerMethod
         {
             public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
             {
                 new LiteralAssemblerCode("sub esp, 32");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp+16]");
                 new LiteralAssemblerCode("imul eax, DWORD [ebp+20]");
                 new LiteralAssemblerCode("mov DWORD [ebp-4], eax");
                 new LiteralAssemblerCode("cmp DWORD [ebp-4], 0");
                 new LiteralAssemblerCode("je L1");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-4]");
                 new LiteralAssemblerCode("lea edx, [0+eax*4]");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp+12]");
                 new LiteralAssemblerCode("add eax, edx");
                 new LiteralAssemblerCode("mov eax, DWORD [eax]");
                 new LiteralAssemblerCode("mov DWORD [ebp-8], eax");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-4]");
                 new LiteralAssemblerCode("lea edx, [0+eax*4]");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp+8]");
                 new LiteralAssemblerCode("add eax, edx");
                 new LiteralAssemblerCode("mov eax, DWORD [eax]");
                 new LiteralAssemblerCode("mov DWORD [ebp-12], eax");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-8]");
                 new LiteralAssemblerCode("shr eax, 24");
                 new LiteralAssemblerCode("mov DWORD [ebp-16], eax");
                 new LiteralAssemblerCode("cmp DWORD [ebp-16], 0");
                 new LiteralAssemblerCode("je L4");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-8]");
                 new LiteralAssemblerCode("and eax, 16711935");
                 new LiteralAssemblerCode("imul eax, DWORD [ebp-16]");
                 new LiteralAssemblerCode("mov edx, eax");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-12]");
                 new LiteralAssemblerCode("and eax, 16711935");
                 new LiteralAssemblerCode("mov ecx, eax");
                 new LiteralAssemblerCode("mov eax, 255");
                 new LiteralAssemblerCode("sub eax, DWORD [ebp-16]");
                 new LiteralAssemblerCode("imul eax, ecx");
                 new LiteralAssemblerCode("add eax, edx");
                 new LiteralAssemblerCode("and eax, -16711936");
                 new LiteralAssemblerCode("mov DWORD [ebp-20], eax");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-8]");
                 new LiteralAssemblerCode("and eax, 65280");
                 new LiteralAssemblerCode("imul eax, DWORD [ebp-16]");
                 new LiteralAssemblerCode("mov edx, eax");
                 new LiteralAssemblerCode("mov eax, DWORD [ebp-12]");
                 new LiteralAssemblerCode("and eax, 65280");
                 new LiteralAssemblerCode("mov ecx, eax");
                 new LiteralAssemblerCode("mov eax, 255");
                 new LiteralAssemblerCode("sub eax, DWORD [ebp-16]");
                 new LiteralAssemblerCode("imul eax, ecx");
                 new LiteralAssemblerCode("add eax, edx");
                 new LiteralAssemblerCode("and eax, 16711680");
                 new LiteralAssemblerCode("mov DWORD [ebp-24], eax");
                 new LiteralAssemblerCode("nop");
                 new LiteralAssemblerCode("jmp L1");
                 new LiteralAssemblerCode("L4:");
                 new LiteralAssemblerCode("nop");
                 new LiteralAssemblerCode("L1:");
             }
         }*/
    }
}
