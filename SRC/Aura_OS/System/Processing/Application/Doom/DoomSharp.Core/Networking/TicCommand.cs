using DoomSharp.Core.Input;

namespace DoomSharp.Core.Networking;

/// <summary>
/// The data sampled per tick (single player)
/// and transmitted to other peers (multiplayer).
/// Mainly movements/button commands per game tick,
/// plus a checksum for internal state consistency.
/// </summary>
public class TicCommand
{
    public sbyte ForwardMove { get; set; }
    public sbyte SideMove { get; set; }
    public short AngleTurn { get; set; }
    public short Consistency { get; set; }
    public char ChatChar { get; set; }
    public ButtonCode Buttons { get; set; }
}