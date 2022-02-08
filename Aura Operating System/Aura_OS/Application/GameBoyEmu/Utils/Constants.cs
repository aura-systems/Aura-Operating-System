using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDMG.Utils {
    public static class Constants {
        public const int DMG_4Mhz = 4194304;
        public const float REFRESH_RATE = 59.7275f;
        public const int CYCLES_PER_UPDATE = (int)(DMG_4Mhz / REFRESH_RATE);
        public const float MILLIS_PER_FRAME = 16.74f;
    }
}
