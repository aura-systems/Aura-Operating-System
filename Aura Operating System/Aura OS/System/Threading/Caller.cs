using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
namespace Aura_OS.System.Threading
{
    public static unsafe class Caller
    {
        [PlugMethod(Assembler = typeof(CallerPlugStatics))]
        private static void CallCodeStatic(uint address) { }
        private static void CallCodeStatic(aMethod aMethod)
        {
            CallCodeStatic(Utils.getMethodHandler(aMethod));
        }

        [PlugMethod(Assembler = typeof(CallerPlugStaticsParam))]
        private static void CallCodeStaticParam(uint address, object param) { }
        private static void CallCodeStaticParam(oMethod aMethod, object param)
        {
            CallCodeStaticParam((uint)aMethod.GetHashCode(), param);
        }

        [PlugMethod(Assembler = typeof(CallerPlugInst))]
        private static void CallCodeInstance(uint address, object obj) { }
        private static void CallCodeInstance(aMethod aMethod, object obj)
        {
            CallCodeInstance(Utils.getMethodHandler(aMethod), obj);
        }

        [PlugMethod(Assembler = typeof(CallerPlugInstParam))]
        private static void CallCodeInstanceParam(uint address, object obj, object param) { }
        private static void CallCodeInstanceParam(oMethod aMethod, object obj, object param)
        {
            CallCodeInstanceParam((uint)aMethod.GetHashCode(), obj, param);
        }

        public static void CallCode(uint address, object obj)
        {
            if (obj == null)
            {
                CallCodeStatic(address);
            }
            else
            {
                CallCodeInstance(address, obj);
            }
        }
        public static void CallCode(aMethod aMethod, object obj)
        {
            CallCode(Utils.getMethodHandler(aMethod), obj);
        }

        public static void CallCode(uint aMethod, object obj, object param)
        {
            if (obj == null)
            {
                CallCodeStaticParam(aMethod, param);
            }
            else
            {
                CallCodeInstanceParam(aMethod, obj, param);
            }
        }
        public static void CallCode(oMethod aMethod, object obj, object param)
        {
            CallCode((uint)aMethod.GetHashCode(), obj, param);
        }

        [Plug(Target = typeof(Caller))]
        public class CallerPlugInst : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 16, sourceIsIndirect: true);
                XS.Call(XSRegisters.EAX);
            }
        }

        [Plug(Target = typeof(Caller))]
        public class CallerPlugInstParam : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 20, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 16, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 24, sourceIsIndirect: true);
                XS.Call(XSRegisters.EAX);
                XS.Add(XSRegisters.ESP, 8);
            }
        }

        [Plug(Target = typeof(Caller))]
        public class CallerPlugStatics : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8, sourceIsIndirect: true);
                XS.Call(XSRegisters.EAX);
            }
        }

        [Plug(Target = typeof(Caller))]
        public class CallerPlugStaticsParam : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8, sourceIsIndirect: true);
                XS.Push(XSRegisters.EAX);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 16, sourceIsIndirect: true);
                XS.Call(XSRegisters.EAX);
                XS.Pop(XSRegisters.EAX);
            }
        }
    }
}
