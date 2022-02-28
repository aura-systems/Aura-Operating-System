using Aura_OS.Processing.Executable;
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
                if (aContext.EAX == 0x01) //Print function.
                {
                    byte* dat = PE32.ProgramAdress + aContext.EDI;

                    Console.Write(GetUnicodeString(dat));
                }
                if (aContext.EAX == 0x02) //Clear function.
                {
                    Kernel.console.Clear();
                }
            }
        }

        public static string GetUnicodeString(byte *str)
        {
            string test = "";

            int len = str[4] * 2;

            for (int i = 0; i < len; i++)
            {
                if (str[8 + i] != 0)
                {
                    test += (char)str[8 + i];
                }
            }

            return test;
        }
    }
}
