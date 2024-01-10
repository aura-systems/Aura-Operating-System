using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class Door : Thinker
{
    public static readonly Fixed DefaultSpeed = Fixed.FromInt(2);
    public const int Wait = 150;

    public Door(Sector sector)
    {
        Sector = sector;
    }

    public Sector Sector { get; }
    public DoorType Type { get; set; }
    public Fixed TopHeight { get; set; }
    public Fixed Speed { get; set; }

    /// <summary>
    /// 1 = up, 0 = waiting at top, -1 = down
    /// </summary>
    public int Direction { get; set; }

    /// <summary>
    /// Tics to wait at the top
    /// </summary>
    public int TopWait { get; set; }

    /// <summary>
    /// (keep in case a door going down is reset)
    /// when it reaches 0, start going down
    /// </summary>
    public int TopCountDown { get; set; }
}