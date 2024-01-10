using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class Platform : Thinker
{
    public const int DefaultWait = 3;
    public static readonly Fixed DefaultSpeed = Fixed.Unit;
    public const int MaxPlats = 30;

    public Sector? Sector { get; set; }

    public Fixed Speed { get; set; }
    public Fixed Low { get; set; }
    public Fixed High { get; set; }

    public int Wait { get; set; }
    public int Count { get; set; }

    public PlatformState Status { get; set; }
    public PlatformState OldStatus { get; set; }

    public bool Crush { get; set; }

    public int Tag { get; set; }
    public PlatformType Type { get; set; }
}
