namespace DoomSharp.Core.Data;

public record WadLump(WadFile File, WadFile.FileLump Lump)
{
    public byte[]? Data { get; set; }
    public PurgeTag Tag { get; set; } = PurgeTag.Cache; // Default (purgeable)
}