using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic
{
    public class FireFlicker : Thinker
    {
        public FireFlicker(Sector sector)
        {
            Sector = sector;
        }

        public Sector Sector { get; }
        public int MaxLight { get; set; }
        public int MinLight { get; set; }
        public int Count { get; set; }
    }
}