using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public unsafe class Thread
    {
        public static int NewThreadID = 0;

        public static ulong upTime;

        public const byte SIG_KILL = 0xA;
        public const byte SIG_WAIT = 0xB;
        public const byte SIG_READY = 0xC;
        public const byte SIG_MSG = 0xD;

        public enum THREAD_WAITING
        {
            NOTHING,
            TIME
        }
        public enum THREAD_STATES
        {
            RUNNABLE,
            WAITING,
            DEAD
        }
        public enum THREAD_PRIORITY
        {
            LOW = 1,
            NORMAL = 3,
            HIGH = 5,
            REALTIME = 7
        }

        public int TID;
        public uint userStack;
        public uint userStackTop;
        public uint kernelStack;
        public uint kernelStackTop;
        public int basePriority;
        public int realPriority;
        public THREAD_STATES state;
        public THREAD_WAITING waitingFor;
        public ulong param;
        public string name;

        public Thread(aMethod entry, string title)
        {
            name = title;
            uint ss = 0x10;
            uint cs = 0x08;
            TID = NewThreadID++;
            basePriority = (int)THREAD_PRIORITY.NORMAL;
            realPriority = basePriority;
            state = THREAD_STATES.RUNNABLE;
            kernelStackTop = Heap.alloc(1024);
            kernelStack = kernelStackTop + 1024;
            userStack = Heap.alloc(4096);
            uint* stack = (uint*)(userStack + 4096);
            userStackTop = userStack;

            *--stack = ss; // ss ?
            *--stack = 0x00000202; // eflags
            *--stack = cs; // cs
            *--stack = (uint)GetHashCode(entry);//entry.GetHashCode(); // eip

            *--stack = 0;
            *--stack = 0;

            *--stack = 0; // EDI
            *--stack = 0; // ESI
            *--stack = 0; // EBP
            *--stack = 0; // No value
            *--stack = 0; // EBX
            *--stack = 0; // EDX
            *--stack = 0; // ECX
            *--stack = 0; // EAX

            userStack = (uint)stack;
            name = title;
        }

        public void Start()
        {
            bool found = false;
            for (int i = 0; i < Scheduler.ActiveThreads.Count; i++)
            {
                if (Scheduler.ActiveThreads[i] == this)
                {
                    found = true;
                }
            }
            if (!found)
            {
                Scheduler.ActiveThreads.Add(this);
            }
        }

        public void Stop()
        {
            for (int i = 0; i < Scheduler.ActiveThreads.Count; i++)
            {
                if (Scheduler.ActiveThreads[i] == this)
                {
                    Scheduler.ActiveThreads.RemoveAt(i);
                }
            }
        }

        public static void Sleep(ulong ms)
        {
            Thread t = Scheduler.CurrentThread;
            t.param = upTime + ms;
            t.state = THREAD_STATES.WAITING;
            t.waitingFor = THREAD_WAITING.TIME;
            while (t.state != THREAD_STATES.RUNNABLE) { }
        }

        public static bool IsThreadAlive(int id)
        {
            foreach (var thread in Scheduler.ActiveThreads)
            {
                if (thread.TID == id)
                {
                    return thread.state != THREAD_STATES.DEAD;
                }
            }
            return false;
        }

        public static uint GetHashCode(string src)
        {
            if (src.Length == 0) return 0;
            uint hash = src[0];
            for (int i = 1; src[i] != 0; i++)
                hash = (hash << 4) + src[i];
            return (uint)(hash % src.Length);
        }

        public static uint GetHashCode(aMethod src)
        { 
            //  if (src.Length == 0) return 0;
            //int hash = src[0];
            //   for (int i = 1; src[i] != 0; i++)
            //     hash = (hash << 4) + src[i];
            //  return (uint)(hash % src.Length);
            return GetHashCode(src.ToString());
        }
    }
}
