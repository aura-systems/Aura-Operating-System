using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class Timer
    {
        /// <summary>
        /// Waiting during a limited time that the condtion change.
        /// </summary>
        /// <param name="secondsToWait">Time required before break the loop.</param>
        /// <param name="whileCondition">If it's false, it will wait.</param>
        public void Wait(int secondsToWait, bool whileCondition)
        {
            int _deltaT = 0;
            int second = 0;
            while (!whileCondition)
            {
                if (_deltaT != Cosmos.HAL.RTC.Second)
                {
                    second++;
                    _deltaT = Cosmos.HAL.RTC.Second;
                }

                if (second >= secondsToWait)
                {
                    Apps.System.Debugger.debugger.Send("No response in " + secondsToWait + " secondes...");
                    break;
                }
            }
        }
    }
}
