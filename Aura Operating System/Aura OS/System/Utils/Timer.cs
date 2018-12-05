using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils
{
    class Timer
    {
        public enum State
        {
            Waiting,
            None,
            Finished
        }

        public State Status
        {
            get;
            set;
        }

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

        public void BreakWait(int secondsToWait)
        {
            int _deltaT = 0;
            int second = 0;

            if (_deltaT != Cosmos.HAL.RTC.Second)
            {
                second++;
                _deltaT = Cosmos.HAL.RTC.Second;
            }

            if (second >= secondsToWait)
            {
                Status = State.Finished;
            }
            else
            {
                Status = State.Waiting;
            }
        }
    }
}
