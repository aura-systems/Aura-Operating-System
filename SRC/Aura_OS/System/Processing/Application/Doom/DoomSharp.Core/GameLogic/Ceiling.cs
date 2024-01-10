using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class Ceiling : Thinker
{
    public static readonly Fixed DefaultSpeed = Fixed.Unit;
    public const int Wait = 150;
    public const int MaxCeilings = 30;

    public CeilingType Type { get; set; }

    public Sector? Sector { get; set; }

    public Fixed BottomHeight { get; set; }
    public Fixed TopHeight { get; set; }

    public Fixed Speed { get; set; }
    public bool Crush { get; set; }

    // 1 = up, 0 = waiting, -1 = down
    public int Direction { get; set; }

    // ID
    public int Tag { get; set; }
    public int OldDirection { get; set; }
}
