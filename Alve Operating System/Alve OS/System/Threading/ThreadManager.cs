using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public static class ThreadManager
    {
        private static List<Thread> threads;
        private static Queue<ThreadSignal> signals;

        public static void PrepareThreading()
        {
            Console.WriteLine("Initializing thread list...");
            threads = new List<Thread>();
            Console.WriteLine("Initalizing signal queue...");
            signals = new Queue<ThreadSignal>();
        }

        public static void Register(Thread Thrd)
        {
            Thrd.TID = threads.Count + 1;
            //log
            threads.Add(Thrd);
        }

        public static void ProcessSignals()
        {
            for (int i = 0; i < signals.Count; i++)
            {
                ThreadSignal ThrdSgnl = signals.Dequeue();
                ThrdSgnl.thread.HandleSignal(ThrdSgnl.signal);
            }
        }

        public static void SendSignal(int tid, string signal)
        {
            for (int i = 0; i < threads.Count; i++)
            {
                if (threads[i].TID == tid)
                {
                    signals.Enqueue(new ThreadSignal(threads[i], signal));
                    ProcessSignals();
                    return;
                }
            }
        }
        public static void SendSignal(string threadName, string signal)
        {
            for (int i = 0; i < threads.Count; i++)
            {
                if (threads[i].name == threadName)
                {
                    SendSignal(threads[i].TID, signal);
                    return;
                }
            }
        }

        public static void HandleRequest(string[] args)
        {

        }
    }
}
