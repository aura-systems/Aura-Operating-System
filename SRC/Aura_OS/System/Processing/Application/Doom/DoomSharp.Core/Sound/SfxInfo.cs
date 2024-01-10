using System;

namespace DoomSharp.Core.Sound;

public record SfxInfo(string Name, bool Singularity, int Priority, SfxInfo? Link, int Pitch, int Volume)
{
    /// <summary>
    /// Up to 6-character name
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// Lump number of sfx
    /// </summary>
    public int LumpNum { get; set; }

    /// <summary>
    /// Sfx singularity (only one at a time)
    /// </summary>
    public bool Singularity { get; } = Singularity;

    /// <summary>
    /// Sfx priority
    /// </summary>
    public int Priority { get; } = Priority;

    /// <summary>
    /// Referenced sound if a link
    /// </summary>
    public SfxInfo? Link { get; } = Link;

    /// <summary>
    /// Pitch if a link
    /// </summary>
    public int Pitch { get; } = Pitch;

    /// <summary>
    /// Volume if a link
    /// </summary>
    public int Volume { get; } = Volume;

    /// <summary>
    /// Sound data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// This is checked every second to see if sound can be thrown out:
    /// if 0, then decrement.
    /// if -1, then throw out.
    /// if > 0, then it is in use.
    /// </summary>
    public int Usefulness { get; set; }
}
