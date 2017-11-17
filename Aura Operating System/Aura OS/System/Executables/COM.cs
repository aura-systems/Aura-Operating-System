using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.API;
using System.Runtime.InteropServices;
using XSharp;
using Cosmos.IL2CPU.API.Attribs;
using XSharp.Assembler;
using System.IO;
using Cosmos.Core;
using CPUx86 = XSharp.Assembler.x86;

namespace  Aura_OS.System.Executable
{
    public unsafe class COM
    {

        public static uint ProgramAddress;

        public static void LoadAuraProgram(byte[] code)
        {
            byte* data = (byte*)Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)code.Length);
            ProgramAddress = (uint)&data[0];
            for (int i = 0; i < code.Length; i++)
            {
                data[i] = code[i];
            }
            Caller call = new Caller();
            call.CallCode((uint)&data[0]);
        }

        public static void LoadCOMProgram(byte[] code)
        {
            code = Replace(code);
            byte* data = (byte*)Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)code.Length);

            ProgramAddress = (uint)&data[0];
            for (int i = 0; i < code.Length; i++)
            {
                data[i] = code[i];
            }

            Console.WriteLine((uint)&data[0]);

            Caller call = new Caller();

            exitUtils.Vs8086Mode();

            call.CallCode((uint)&data[0]);
        }
		
        public class Caller
        {
            [PlugMethod(Assembler = typeof(CallerPlug))]
            public void CallCode(uint address) { }
        }
        [Plug(Target = typeof(Caller))]
        public class CallerPlug : AssemblerMethod// : PlugMethod // : Method
        {
           // public override void AssembleNew(object aAssembler, object aMethodInfo)
           // {
                //   XS.Set(XSRegisters.EBX, false, false, 8, true, null XSRegisters.RegisterSize.Byte8);
               // XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
              //  XS.Call(XSRegisters.EAX);
           // }

            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {//I asked jp2masa
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
                XS.Call(XSRegisters.EAX);
            }
        }

        private static byte[] Replace(byte[] input)
        {
            List<byte> bytes = new List<byte>();

            foreach (byte byte_ in input)
            {
                if (byte_ == 0x21)
                    bytes.Add(0x48);
                else
                    bytes.Add(byte_);
            }

            byte[] result = bytes.ToArray();

            return result;
        }
    }
}

