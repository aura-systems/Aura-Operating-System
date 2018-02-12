/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plain binary loader.
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using XSharp;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace  Aura_OS.System.Executables
{
    public unsafe class PlainBinaryProgram
    {

        public static uint ProgramAddress;

        public static void LoadProgram(byte[] code)
        {
            var address = Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)code.Length);
            byte* ptr = (byte*)address;

            for (int i = 0; i < code.Length; i++)
            {
                ptr[i] = code[i];
            }

            Caller call = new Caller();
            call.CallCode(address);
        }

        public class Caller
        {
            [PlugMethod(Assembler = typeof(CallerPlug))]
            public void CallCode(uint address) { }
        }
        [Plug(Target = typeof(Caller))]
        public class CallerPlug : AssemblerMethod// : PlugMethod // : Method
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {//I asked jp2masa
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
                XS.Call(XSRegisters.EAX);
            }
        }
    }
}

