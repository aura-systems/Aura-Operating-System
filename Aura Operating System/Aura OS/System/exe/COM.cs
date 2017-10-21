using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.API;
using System.Runtime.InteropServices;
using XSharp;
using Cosmos.IL2CPU.API.Attribs;
using XSharp.Assembler;
namespace  Aura_OS.System.exe
{
    public unsafe class COM
    {
        private byte[] code;

        public COM(string file)
        {
           // code = { 0;//readfile
        }

        public void Execute()
        {
            /* This might be overwritting something, but since we do not have paging working
            * there is really no 'better' alternative. I have test this though and I have
            * not noticed any bad side effects so I will assume this is somewhat safe...
            */
            byte* ptr = (byte*)0x100;
            for (int i = 0; i < code.Length; i++)
                ptr[i] = code[i];
            Caller c = new Caller();
            c.CallCode(0x100);
            //caller c = new caller();
           // c.callercode0x100
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
    }
}
