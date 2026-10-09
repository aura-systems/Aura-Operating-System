/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Names of the kernel threads Aura starts
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Threading;
using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.System.Processing
{
    // GEN3-GAP(thread-names): KernelThreadInfo has no name.
    /// <summary>
    /// The names of the threads Aura starts (the FTP and HTTP servers), which the scheduler does not
    /// keep: the Task Manager shows them. Each thread names itself first thing. No lock: a thread
    /// writes only its own entry and the UI thread only reads, so a name being written reads as none.
    /// </summary>
    public static class ThreadNames
    {
        // Thread stacks are never freed: past this many named threads, the kernel is short of memory anyway.
        private const int Capacity = 32;

        private static readonly uint[] s_ids = new uint[Capacity];
        private static readonly string[] s_names = new string[Capacity];
        private static int s_count;

        /// <summary>
        /// Names the calling thread.
        /// </summary>
        public static void NameCurrent(string name)
        {
            KernelThreadInfo thread;

            if (!SchedulerDiagnostics.TryGetCurrentThread(0, out thread))
            {
                return;
            }

            int slot = Interlocked.Increment(ref s_count) - 1;

            if (slot >= Capacity)
            {
                return;
            }

            // The name first: the reader finds the entry by its id.
            s_names[slot] = name;
            Volatile.Write(ref s_ids[slot], thread.Id);
        }

        /// <summary>
        /// The name the thread with that id gave itself, null for none.
        /// </summary>
        public static string Of(uint id)
        {
            int count = Volatile.Read(ref s_count);

            // An entry being written still has id 0, the main loop's, which names no thread.
            for (int i = 0; i < count && i < Capacity; i++)
            {
                if (Volatile.Read(ref s_ids[i]) == id && id != 0)
                {
                    return s_names[i];
                }
            }

            return null;
        }
    }
}
