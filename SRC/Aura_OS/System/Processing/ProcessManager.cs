/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Process manager (style monotask but used to handle Update methods)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aura_OS.System;
using Aura_OS.System.Graphics.UI.GUI;

namespace Aura_OS.Processing
{
    /// <summary>
    /// Manages processes within AuraOS, allowing for registration,
    /// starting, and stopping of processes. While the system is styled as monotask,
    /// this manager is used to handle Update methods for processes.
    /// </summary>
    public class ProcessManager : IManager
    {
        /// <summary>
        /// A list of all registered processes.
        /// </summary>
        public List<Process> Processes;

        private uint nextProcessId = 0;

        /// <summary>
        /// Initializes the process manager, preparing it to manage processes.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting process manager...");

            Processes = new List<Process>();
            nextProcessId = 0;
        }

        /// <summary>
        /// Registers a new process with the process manager.
        /// </summary>
        /// <param name="process">The process to register.</param>
        /// <returns>True if the process was successfully registered; false if the process is already registered.</returns>
        public bool Register(Process process)
        {
            if (!Processes.Contains(process))
            {
                Processes.Add(process);
                process.SetID(nextProcessId++);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unregisters a process with the process manager.
        /// </summary>
        /// <param name="process">The process to unregister.</param>
        /// <returns>True if the process was successfully unregistered; false if the process does not exist.</returns>
        public bool Unregister(Process process)
        {
            return Processes.Remove(process);
        }

        /// <summary>
        /// Starts a registered process.
        /// </summary>
        /// <param name="process">The process to start.</param>
        /// <returns>True if the process was found and started; false otherwise.</returns>
        public bool Start(Process process)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] == process) { Processes[i].Start(); return true; }
            }
            return false;
        }

        /// <summary>
        /// Stops a registered process.
        /// </summary>
        /// <param name="process">The process to stop.</param>
        /// <returns>True if the process was found and stopped; false otherwise.</returns>
        public bool Stop(Process process)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] == process) { Processes[i].Stop(); return true; }
            }
            return false;
        }

        /// <summary>
        /// Updates all registered processes. This method is typically called in the OS's main loop.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i].Running)
                {
                    Processes[i].Update();
                }
            }
        }

        internal Process GetProcessByPid(uint pid)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i].ID == pid) { return Processes[i]; }
            }
            return null;
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Process Manager";
        }
    }
}
