using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic
{
    public class GlowingLight : Thinker
    {
        public const int Speed = 8;

        public GlowingLight(Sector sector)
        {
            Sector = sector;
        }

        public Sector Sector { get; }
        public int MinLight { get; set; }
        public int MaxLight { get; set; }
        public int Direction { get; set; }
    }
}