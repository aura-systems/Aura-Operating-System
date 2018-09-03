/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Get multiboot info structure
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using IL2CPU.API.Attribs;
using XSharp.Assembler;
using CPUx86 = XSharp.Assembler.x86;

namespace Aura_OS.Core
{

    public class GetMBI : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new CPUx86.Push { DestinationRef = ElementReference.New("MultiBootInfo_Structure"), DestinationIsIndirect = true };
        }

        [PlugMethod(Assembler = typeof(GetMBI))]
        public static uint GetMBIAddress() { return 0; }
    }

}
