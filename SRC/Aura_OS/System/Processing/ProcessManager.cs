/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Process manager (style monotask but used to handle Update methods)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.Processing
{
    public class ProcessManager
    {
        public List<Process> Processes;

        public void Initialize()
        {
            Processes = new List<Process>();
        }

        public bool Register(Process process)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] == process) { return false; }
            }

            Processes.Add(process);
            Processes[Processes.Count - 1].SetID((uint)(Processes.Count - 1));
            return true;
        }

        public bool Start(Process process)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] == process) { Processes[i].Start(); return true; }
            }
            return false;
        }

        public bool Stop(Process process)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] == process) { Processes[i].Stop(); return true; }
            }
            return false;
        }

        public void Update()
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                Processes[i].Update();
            }
        }
    }
}
