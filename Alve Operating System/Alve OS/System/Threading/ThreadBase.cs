using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public abstract class ThreadBase
    {
        public int TID;
        public delegate void ThreadMethod();
        public ThreadMethod action = null;
        public string name;
        
        public void HandleSignal(string signal)
        {
            switch (signal)
            {
                case "START":
                    Start();
                    break;
            }
        }

        private void Start()
        {
            action.Invoke();
        }

        private void Pause()
        {

        }

        private void Resume()
        {

        }

        private void Stop()
        {

        }

        private void Kill()
        {

        }
    }
}
