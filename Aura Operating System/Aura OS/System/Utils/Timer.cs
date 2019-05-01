using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils
{
    class Timer
    {
        private static TimeSpan timeatstart;
        private static TimeSpan timeatend;
        private static TimeSpan now;
        private static int second;

        public void Start(int secondsToWait)
        {
            timeatstart = DateTime.Now.TimeOfDay;
            timeatend = timeatstart + TimeSpan.FromSeconds(4);            
        }

        public bool AtEnd()
        {
            while (true)
            {
                now = DateTime.Now.TimeOfDay;

                if (timeatend == now)
                {
                    return true;
                }
            }
        }
    }
}
