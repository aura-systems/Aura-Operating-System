using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

public class Button
{
    public ButtonWhere Where { get; set; }
    public int Texture { get; set; }
    public int Timer { get; set; }
    public Line? Line { get; set; } = null;
    public MapObject? SoundOrigin { get; set; } = null;
}

public enum ButtonWhere
{
    Top,
    Middle,
    Bottom
}