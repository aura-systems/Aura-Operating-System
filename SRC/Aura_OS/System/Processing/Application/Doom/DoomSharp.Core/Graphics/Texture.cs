namespace DoomSharp.Core.Graphics;

/// <summary>
/// A maptexturedef_t describes a rectangular texture,
///  which is composed of one or more mappatch_t structures
///  that arrange graphic patches.
/// </summary>
public record Texture(string Name, short Width, short Height, short PatchCount)
{
    public TexturePatch[] Patches { get; } = new TexturePatch[PatchCount];
}

/// <summary>
/// A single patch from a texture definition,
///  basically a rectangular area within
///  the texture rectangle.
/// </summary>
public record TexturePatch(int OriginX, int OriginY, int Patch);