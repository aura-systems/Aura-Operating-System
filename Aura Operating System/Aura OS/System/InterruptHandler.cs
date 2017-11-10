using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System
{
    class InterruptHandler
    {
        public delegate void InterruptDelegate(ref Cosmos.Core.INTs.IRQContext aContext);
        public byte intNum;
        public InterruptDelegate handler;
        public static List<InterruptHandler> interruptHandlers = new List<InterruptHandler>();
    }
}
