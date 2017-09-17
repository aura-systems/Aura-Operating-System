using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public class ThreadSignal
    {
        public Thread thread;
        public string signal;
        public ThreadSignal(Thread thrd, string sgnl)
        {
            this.thread = thrd;
            this.signal = sgnl;
        }
    }
}
