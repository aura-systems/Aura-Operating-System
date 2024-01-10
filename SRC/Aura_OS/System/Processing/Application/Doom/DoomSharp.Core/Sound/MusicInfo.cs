using System;

namespace DoomSharp.Core.Sound;

public record MusicInfo(string Name = "")
{
    /// <summary>
    /// Up to 6-character name
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// Lump number of music
    /// </summary>
    public int LumpNum { get; set; }

    /// <summary>
    /// Music data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Music handle once registered
    /// </summary>
    public int Handle { get; set; }
}
