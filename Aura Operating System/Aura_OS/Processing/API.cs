using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cosmos.Core.INTs;

namespace Aura_OS.Processing
{
    public unsafe class API : Process
    {
        public API() : base("Aura API", ProcessType.Driver)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            Kernel.ProcessManager.Register(this);

            SetIntHandler(0x48, SWI);
        }

        public unsafe static void SWI(ref IRQContext aContext)
        {
            if (aContext.Interrupt == 0x48) //API interrupt.
            {
                Kernel.console.WriteLine("Aura API called!");

                if (aContext.EAX == 0x01) //Print function.
                {
                    Kernel.console.WriteLine("Print called!");
                }
            }
        }
    }
}
