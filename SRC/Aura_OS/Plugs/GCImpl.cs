/*
* PROJECT:          Aura Operating System Development
* CONTENT:          System.GC plug (gen3 runtime without a finalizer queue).
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Build.API.Attributes;

namespace Aura_OS.Plugs
{
    // GEN3-GAP(finalizers): Cosmos runs no finalizers (RhSuppressFinalize/RhReRegisterForFinalize are
    // no-ops) and Cosmos.Kernel.Core does not export RhWaitForPendingFinalizers (left commented out in
    // Runtime/Stdllib.cs). GC.GetTotalMemory, the only public live-heap reading (MemoryInfoApp,
    // GEN3-GAP(meminfo)), calls GC.WaitForPendingFinalizers in its body, so the kernel failed to link with
    // "undefined symbol: RhWaitForPendingFinalizers". With no finalizer queue there is never anything to
    // wait for, so the wait returns at once. Remove this plug once Cosmos exports the symbol.
    [Plug(typeof(GC))]
    public static class GCImpl
    {
        [PlugMember]
        public static void WaitForPendingFinalizers()
        {
        }
    }
}
