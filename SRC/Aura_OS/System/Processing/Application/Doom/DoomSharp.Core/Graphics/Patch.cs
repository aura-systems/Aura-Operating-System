using System.IO;

namespace DoomSharp.Core.Graphics;

/// <summary>
/// Patches.
/// A patch holds one or more columns.
/// Patches are used for sprites and all masked pictures,
/// and we compose textures from the TEXTURE1/2 lists
/// of patches.
/// </summary>
public record struct Patch(ushort Width, ushort Height, short LeftOffset, short TopOffset, uint[] ColumnOffsets, Column?[] Columns)
{
    public static Patch FromBytes(byte[] patchData)
    {
        using var stream = new MemoryStream(patchData, false);
        using var reader = new BinaryReader(stream);

        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var left = reader.ReadInt16();
        var top = reader.ReadInt16();

        var offsets = new uint[width];
        for (var i = 0; i < width; i++)
        {
            offsets[i] = reader.ReadUInt32();
        }

        var columns = new Column?[width];
        for (var i = 0; i < width; i++)
        {
            stream.Seek(offsets[i], SeekOrigin.Begin);

            Column? currentColumn = null;
            var rowStart = reader.ReadByte();
            if (rowStart == 255)
            {
                columns[i] = null;
                continue;
            }

            while (rowStart != 255)
            {
                var pixelCount = reader.ReadByte();
                _ = reader.ReadByte(); // dummy value
                var pixels = reader.ReadBytes(pixelCount);
                _ = reader.ReadByte(); // dummy value

                var column = new Column(rowStart, pixelCount, pixels);
                if (currentColumn is null)
                {
                    columns[i] = column;
                }
                else
                {
                    currentColumn.Next = column;
                }
                
                currentColumn = column;
                rowStart = reader.ReadByte();
            }
        }

        return new Patch(width, height, left, top, offsets, columns);
    }

    public Column? GetColumnByOffset(uint offset, int skip = 0)
    {
        var i = 0;
        foreach (var columnOffset in ColumnOffsets)
        {
            if (columnOffset == offset)
            {
                return i + skip >= Columns.Length ? Columns[i + skip - Columns.Length] : Columns[i + skip];
            }

            i++;
        }

        return null;
    }
}