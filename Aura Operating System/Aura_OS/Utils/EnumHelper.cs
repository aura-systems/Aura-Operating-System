using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveOS.GUI;

namespace WaveOS
{
    public static class EnumHelper
    {
        public static bool IsAnchorSet(AnchorStyles value, AnchorStyles flags)
        {
            return (value & flags) == flags;
        }

        public static bool IsResizeSet(WindowResizeState value, WindowResizeState flags)
        {
            return (value & flags) == flags;
        }

    }
}
