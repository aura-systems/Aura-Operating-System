using Aura_OS.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE
{
    public unsafe class Layer
    {
        public uint* Pixels { get; set; }

        public Layer(Surface surface)
        {
            Pixels = (uint*)Memory.MemAlloc(surface.Height * surface.Width * 4);
        }
    }
}
