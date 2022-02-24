using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp;
using XSharp.Assembler;

namespace Aura_OS.Processing
{
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
