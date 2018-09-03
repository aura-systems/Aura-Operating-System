using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.Graphics.Rasterizing
{
    public class MinMaxPair
    {
        public MinMaxPair(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int Min { get; set; }
        public int Max { get; set; }
    }
}
