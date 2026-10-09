/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Process manager (style monotask but used to handle Update methods)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System;
using Aura_OS.System.Graphics.UI.GUI;
using Stopwatch = System.Diagnostics.Stopwatch;

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
        /// Time the main loop spent in no process (drawing the frame on the screen, collecting
        /// memory), in Stopwatch ticks.
        /// </summary>
        public long SystemTime;

        // The process the main loop's time goes to now, null for none, and since when (Stopwatch ticks).
        private Process _charged;
        private long _chargedSince;

        /// <summary>
        /// Initializes the process manager, preparing it to manage processes.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting process manager...");

            Processes = new List<Process>();
            nextProcessId = 0;
            _chargedSince = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// From now on, the main loop's time goes to that process (its CpuTime), or to SystemTime for
        /// null; the time since the last call goes to the previous one. Every Aura process runs on the
        /// main loop's thread, which the scheduler times as a whole: this splits it.
        /// </summary>
        /// <returns>The previous process, to give the time back to once this one is done.</returns>
        public Process Charge(Process process)
        {
            long now = Stopwatch.GetTimestamp();
            long elapsed = now - _chargedSince;

            if (_charged != null)
            {
                _charged.CpuTime += elapsed;
            }
            else
            {
                SystemTime += elapsed;
            }

            Process previous = _charged;
            _charged = process;
            _chargedSince = now;
            return previous;
        }

        /// <summary>
        /// Counts the time of the process running now, up to now: the times read next are the latest.
        /// </summary>
        public void Flush()
        {
            Charge(_charged);
        }

        /// <summary>
        /// Stopwatch ticks in nanoseconds, without overflowing for a time of years.
        /// </summary>
        public static long Nanoseconds(long ticks)
        {
            long frequency = Stopwatch.Frequency;
            return ticks / frequency * 1_000_000_000L + ticks % frequency * 1_000_000_000L / frequency;
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
                Process process = Processes[i];

                if (process.Running)
                {
                    Process previous = Charge(process);
                    process.Update();
                    Charge(previous);
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
