namespace DoomSharp.Core.GameLogic;

public class PlayerSprite
{
    public State? State { get; set; }
    public int Tics { get; set; }
    public Fixed SX { get; set; }
    public Fixed SY { get; set; }
}
