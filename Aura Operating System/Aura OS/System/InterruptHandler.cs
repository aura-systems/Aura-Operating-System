using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System
{
    public class InterruptHandler
    {
        public byte intNum;
        public Cosmos.Core.INTs.InterruptDelegate handler;
        public static List<InterruptHandler> interruptHandlers = new List<InterruptHandler>();
    }
}
