using DoomSharp.Core.GameLogic;

namespace DoomSharp.Core.Sound;

public class SoundChannel
{
    /// <summary>
    /// Sound information (if null, channel is available)
    /// </summary>
    public SfxInfo? SfxInfo { get; set; }

    /// <summary>
    /// The origin of sound
    /// </summary>
    public MapObject? Origin { get; set; }

    /// <summary>
    /// Handle of the sound being played
    /// </summary>
    public int Handle { get; set; }
}
