using System;

namespace DoomSharp.Core.Graphics;

public class Sprite
{
    public int NumFrames { get; set; }
    public SpriteFrame[] Frames { get; set; } = Array.Empty<SpriteFrame>();
}

public class SpriteFrame
{
    public const int FullBright = 0x8000;
    public const int FrameMask = 0x7FFF;

    public bool? Rotate { get; set; }

    /// <summary>
    /// Lump to use for view angles 0-7
    /// </summary>
    public short[] Lumps { get; } = { -1, -1, -1, -1, -1, -1, -1, -1 };

    /// <summary>
    /// Flip bit (true = flip) to use for view angles 0-7
    /// </summary>
    public bool[] Flip { get; } = new bool[8];
}