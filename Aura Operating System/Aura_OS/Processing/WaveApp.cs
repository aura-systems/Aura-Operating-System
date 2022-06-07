using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using WaveOS.Graphics;
using Sys = Cosmos.System;

namespace WaveOS
{
    public class WaveApp
    {
        public string Name;
        public string[] args;

        public List<string> barItems = new List<string>();
        public virtual void Initialize() { }
        public virtual int Run()
        {
            return 0;
        }


    }

    public class WaveGUIApp : WaveApp
    {
        public Canvas Canv { get { return Aura_OS.Kernel.canvas; } }
        //public virtual void Initialize() { }
    }
}
