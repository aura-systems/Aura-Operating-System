/*
* PROJECT:          Aura Operating System Development
* CONTENT:          CPU Exceptions.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.IL2CPU.API.Attribs;
using static Cosmos.Core.INTs;

namespace Aura_OS.Core
{
    [Plug(Target = typeof(Cosmos.Core.INTs))]

    public class INTs
    {
        private static void HandleException(uint aEIP, string aDescription, string aName, ref IRQContext ctx, uint lastKnownAddressValue = 0)
        {
            // At this point we are in a very unstable state.
            // Try not to use any Cosmos routines, just
            // report a crash dump.
            const string xHex = "0123456789ABCDEF";
            uint xPtr = ctx.EIP;

            // we're printing exception info to the screen now:
            // 0/0: x
            // 1/0: exception number in hex
            unsafe
            {
                byte* xAddress = (byte*)0xB8000;
                System.Crash.StopKernel("TEST");
                

                if (lastKnownAddressValue != 0)
                {
                    //PutErrorString(1, 0, "Last known address: 0x");

                    //PutErrorChar(1, 22, xHex[(int)((lastKnownAddressValue >> 28) & 0xF)]);
                    //PutErrorChar(1, 23, xHex[(int)((lastKnownAddressValue >> 24) & 0xF)]);
                    //PutErrorChar(1, 24, xHex[(int)((lastKnownAddressValue >> 20) & 0xF)]);
                   // PutErrorChar(1, 25, xHex[(int)((lastKnownAddressValue >> 16) & 0xF)]);
                   // PutErrorChar(1, 26, xHex[(int)((lastKnownAddressValue >> 12) & 0xF)]);
                   // PutErrorChar(1, 27, xHex[(int)((lastKnownAddressValue >> 8) & 0xF)]);
                   // PutErrorChar(1, 28, xHex[(int)((lastKnownAddressValue >> 4) & 0xF)]);
                   // PutErrorChar(1, 29, xHex[(int)(lastKnownAddressValue & 0xF)]);
                }

            }

            // lock up
            while (true)
            {
            }
        }
    }
}
