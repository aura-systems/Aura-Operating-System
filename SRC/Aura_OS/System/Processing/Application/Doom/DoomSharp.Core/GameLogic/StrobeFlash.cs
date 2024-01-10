using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class StrobeFlash : Thinker
{
    public const int StrobeBright = 5;
    public const int SlowDark = 35;
    public const int FastDark = 15;

    public Sector? Sector { get; set; }
    public int Count { get; set; }
    public int MinLight { get; set; }
    public int MaxLight { get; set; }
    public int DarkTime { get; set; }
    public int BrightTime { get; set; }
}