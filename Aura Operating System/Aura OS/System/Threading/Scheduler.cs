using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public static unsafe class Scheduler
    {
        public static List<Thread> ActiveThreads = new List<Thread>(1024);
        public static Thread CurrentThread = null;
        public static int LastPriority;
        private static int schedulerInterrupts;
        public static List<uint> TaskSwitchEvents = new List<uint>();

        public static void Init()
        {
            CurrentThread = new Thread(Init, "INIT");
            CurrentThread.name = "INIT";
            CurrentThread.Start();
            ISR.SetIntHandler(0, TaskSleep);
            ISR.SetIntHandler(8, TaskSwitch);
            IOPort.outb(0x70, 0x0C);
            IOPort.inb(0x71);
        }

        public static void TaskSwitch()
        {
            schedulerInterrupts++;
            if (schedulerInterrupts >= 8)
            {
                schedulerInterrupts = 0;
                CurrentThread.userStack = ISR.old_esp;
                CurrentThread.realPriority = CurrentThread.basePriority;
                foreach (Thread t in ActiveThreads)
                {
                    t.realPriority++;
                    if (t.state == Thread.THREAD_STATES.RUNNABLE)
                    {
                        if (CurrentThread != t)
                        {
                            if (t.realPriority >= CurrentThread.realPriority)
                            {
                                CurrentThread = t;
                            }
                        }
                    }
                }
                LastPriority = CurrentThread.realPriority;
                CurrentThread.realPriority = 1;
                ISR.old_esp = CurrentThread.userStack;
                GDT.SetKernelStack(CurrentThread.kernelStack);
            }
            IOPort.outb(0x70, 0x0C);
            IOPort.inb(0x71);
        }

        public static void TaskSleep()
        {
            Thread.upTime += 1000 / PIT.IPS;
            foreach (Thread t in ActiveThreads)
            {
                if (t.state == Thread.THREAD_STATES.WAITING)
                {
                    if (t.waitingFor == Thread.THREAD_WAITING.TIME)
                    {
                        if (Thread.upTime >= t.param - 3)
                        {
                            t.state = Thread.THREAD_STATES.RUNNABLE;
                            t.waitingFor = Thread.THREAD_WAITING.NOTHING;
                            t.realPriority = LastPriority;
                        }
                    }
                }
            }
            for (int i = 0; i < TaskSwitchEvents.Count; i++)
            {
                Caller.CallCode(TaskSwitchEvents[i], null);
            }
        }

        public static void Idle()
        {
            while (true)
            {
                CPU.Halt();
            }
        }
    }
}
