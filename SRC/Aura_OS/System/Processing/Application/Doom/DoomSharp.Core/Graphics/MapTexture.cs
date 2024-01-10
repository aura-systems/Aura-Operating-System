using System.IO;
using System.Text;

namespace DoomSharp.Core.Graphics;

/// <summary>
/// A DOOM wall texture is a list of patches
/// which are to be combined in a predefined order.
/// </summary>
public record MapTexture(string Name, bool Masked, short Width, short Height, short PatchCount, int ColumnDirectory = 0)
{
    public const int BaseSize = 8 + 4 + 2 + 2 + 2 + 4; // Name = 8 bytes, Masked = 4 byte bool

    public MapPatch[] Patches { get; } = new MapPatch[PatchCount];
    public int Size => BaseSize + PatchCount * MapPatch.Size;

    public void ReadMapPatches(byte[] data)
    {
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        // Read map patches
        for (var i = 0; i < PatchCount; i++)
        {
            Patches[i] = MapPatch.FromReader(reader);
        }
    }

    public static MapTexture FromBytes(byte[] data)
    {
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        var nameBytes = reader.ReadBytes(8);
        var name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
        var masked = reader.ReadInt32() != 0;
        var width = reader.ReadInt16();
        var height = reader.ReadInt16();
        var columnDirectory = reader.ReadInt32();
        var patchCount = reader.ReadInt16();

        return new MapTexture(name, masked, width, height, patchCount, columnDirectory);
    }
}

/// <summary>
/// Each texture is composed of one or more patches,
/// with patches being lumps stored in the WAD.
/// The lumps are referenced by number, and patched
/// into the rectangular texture space using origin
/// and possibly other attributes.
/// </summary>
public record MapPatch(short OriginX, short OriginY, short Patch, short StepDir = 0, short ColorMap = 0)
{
    public const int Size = 2 + 2 + 2 + 2 + 2;

    public static MapPatch FromReader(BinaryReader reader)
    {
        var originX = reader.ReadInt16();
        var originY = reader.ReadInt16();
        var patch = reader.ReadInt16();
        var stepDir = reader.ReadInt16();
        var colorMap = reader.ReadInt16();

        return new MapPatch(originX, originY, patch, stepDir, colorMap);
    }
}