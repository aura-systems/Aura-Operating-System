using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;

namespace Aura_OS.System.Threading
{
    public static unsafe class API
    {
        public static void Handle(ref ISR.IRQContext r)
        {

        }

        [PlugMethod(PlugRequired = true)]
        public static void DEMOCALL() { }

        [Plug(Target = typeof(API))]
        public class APIPlug
        {
            [PlugMethod(Assembler = typeof(asmAPIGen))]
            public static void DEMOCALL() { }

            public class asmAPIGen : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.LiteralCode("int 0x80");
                }
            }
        }
    }
}
