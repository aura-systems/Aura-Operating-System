using System.IO;

namespace DoomSharp.Core.GameLogic;

/// <summary>
/// Thing definition, position, orientation and type,
/// plus skill/visibility flags and attributes.
/// </summary>
public struct MapThing
{
    public const int SizeOfStruct = 2 + 2 + 2 + 2 + 2;

    public short X { get; set; }
    public short Y { get; set; }
    public short Angle { get; set; }
    public short Type { get; set; }
    public short Options { get; set; }

    public static MapThing FromWadData(BinaryReader reader)
    {
        return new MapThing
        {
            X = reader.ReadInt16(),
            Y = reader.ReadInt16(),
            Angle = reader.ReadInt16(),
            Type = reader.ReadInt16(),
            Options = reader.ReadInt16(),
        };
    }
}