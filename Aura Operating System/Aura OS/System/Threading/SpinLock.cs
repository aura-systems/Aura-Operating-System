using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public class SpinLock
    {
        public int mutex;

        public void Lock()
        {
            Lock(ref mutex);
        }

        public void Unlock()
        {
            Unlock(ref mutex);
        }

        public static void Lock(ref int value)
        {
            if (value == Scheduler.CurrentThread.TID)
            {
                return;
            }
            while (value != 0) { }
            value = Scheduler.CurrentThread.TID;
        }

        public static void Unlock(ref int value)
        {
            value = 0;
        }
    }
}
