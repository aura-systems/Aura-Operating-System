/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Process base class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.Processing
{
    public enum ProcessType
    {
        KernelComponent,
        Driver,
        Utility,
        Program,
    }

    public abstract class Process
    {
        public string Name { get; protected set; }
        public uint ID { get; protected set; }
        public ProcessType Type { get; protected set; }
        public bool Initialized { get; protected set; }
        public bool Running { get; private set; }

        /// <summary>
        /// True when the process drives its own work on a dedicated gen3 kernel
        /// thread (see <see cref="ThreadedProcess"/>). The cooperative
        /// ProcessManager.Update() loop skips these so their work is not also
        /// run — twice, and on the wrong thread — from the main UI thread.
        /// </summary>
        public virtual bool IsThreaded => false;

        /// <summary>
        /// Kernel scheduler thread id backing this process, or 0 when it runs
        /// cooperatively on the main UI thread. Cooperative GUI apps that spawn
        /// a compute worker override this to report the worker's id. Surfaced
        /// by the lsprocess command.
        /// </summary>
        public virtual uint ThreadId => 0;

        private static string[] TypeNames = new string[]
        {
            "KernelComponent",
            "Driver",
            "Utility",
            "Program",
            "Unknown",
        };

        public Process(string name, ProcessType type)
        {
            Name = name;
            Type = type;
            ID = 0;
            Running = false;
        }

        public virtual void Initialize()
        {
            if (Initialized)
            {
                return;
            }
            Initialized = true;
        }

        public virtual void Start()
        {
            if (Running)
            {
                return;
            }
            Running = true;
        }

        public virtual void Stop()
        {
            if (!Running)
            {
                return;
            }
            Running = false;
        }

        public virtual void Update() { }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetID(uint id)
        {
            ID = id;
        }

        public void SetType(ProcessType type)
        {
            Type = type;
        }

        public static string GetServiceTypeString(ProcessType type)
        {
            switch (type)
            {
                case ProcessType.KernelComponent:
                {
                    return TypeNames[0];
                }
                case ProcessType.Driver:
                {
                    return TypeNames[1];
                }
                case ProcessType.Utility:
                {
                    return TypeNames[2];
                }
                case ProcessType.Program:
                {
                    return TypeNames[3];
                }
                default:
                {
                    return TypeNames[4];
                }
            }
        }
    }
}
