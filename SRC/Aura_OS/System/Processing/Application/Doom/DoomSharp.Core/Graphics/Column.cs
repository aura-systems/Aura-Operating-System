namespace DoomSharp.Core.Graphics;

public record Column(byte TopDelta, byte Length, byte[] Pixels)
{
    public Column? Next { get; set; } = null;
}