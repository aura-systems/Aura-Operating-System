using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class Intercept
{
    public Fixed Frac { get; set; } // along trace line
    public bool IsLine { get; set; }
    public MapObject? Thing { get; set; }
    public Line? Line { get; set; }
}