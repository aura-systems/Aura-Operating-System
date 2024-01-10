using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic
{
    public class LightFlash : Thinker
    {
        public LightFlash(Sector sector)
        {
            Sector = sector;
        }

        public Sector Sector { get; }

        public int Count { get; set; }
        public int MaxLight { get; set; }
        public int MinLight { get; set; }
        public int MaxTime { get; set; }
        public int MinTime { get; set; }
    }
}