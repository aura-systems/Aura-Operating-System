using DoomSharp.Core.GameLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoomSharp.Core.Graphics;

/// <summary>
/// Your plain vanilla vertex.
/// Note: transformed values not buffered locally,
///  like some DOOM-alikes ("wt", "WebView") did.
/// </summary>
public record Vertex(Fixed X, Fixed Y);

/// <summary>
/// Each sector has a degenmobj_t in its center
///  for sound origin purposes.
/// I suppose this does not handle sound from
///  moving objects (doppler), because
///  position is prolly just buffered, not
///  updated.
/// </summary>
public class DegenMapObject : MapObject
{
    
}

/// <summary>
/// The SECTORS record, at runtime.
/// Stores things/mobjs.
/// </summary>
public record Sector(Fixed FloorHeight, Fixed CeilingHeight, short FloorPic, short CeilingPic, short LightLevel, short Tag)
{
    public short Special { get; set; }

    // 0 = untraversed, 1,2 = sndlines -1
    public int SoundTraversed { get; set; }

    // thing that made a sound (or null)
    public MapObject? SoundTarget { get; set; }

    // mapblock bounding box for height changes
    public int[] BlockBox { get; } = new int[4];

    // origin for any sounds played by the sector
    public DegenMapObject SoundOrigin { get; } = new();

    // if == validcount, already checked
    public int ValidCount { get; set; }

    // list of mobjs in sector
    public MapObject? ThingList { get; set; }

    // thinker_t for reversable actions
    public Thinker? SpecialData { get; set; }

    public int LineCount { get; set; }
    
    public Line[] Lines { get; set; } = Array.Empty<Line>();
    
    public short FloorPic { get; set; } = FloorPic;
    public Fixed FloorHeight { get; set; } = FloorHeight;
    public Fixed CeilingHeight { get; set; } = CeilingHeight;
    public short LightLevel { get; set; } = LightLevel;

    public static Sector ReadFromWadData(BinaryReader reader)
    {
        var floorHeight = Fixed.FromInt(reader.ReadInt16());
        var ceilingHeight = Fixed.FromInt(reader.ReadInt16());
        var floorPic = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        var ceilingPic = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        var lightLevel = reader.ReadInt16();
        var special = reader.ReadInt16();
        var tag = reader.ReadInt16();

        return new Sector(
            floorHeight,
            ceilingHeight,
            (short)DoomGame.Instance.Renderer.FlatNumForName(floorPic),
            (short)DoomGame.Instance.Renderer.FlatNumForName(ceilingPic),
            lightLevel,
            tag)
        {
            Special = special
        };
    }
}

public record SideDef(Fixed TextureOffset, Fixed RowOffset, Sector Sector)
{
    public Fixed TextureOffset { get; set; } = TextureOffset;

    public int TopTexture { get; set; }
    public int BottomTexture { get; set; }
    public int MidTexture { get; set; }

    public static SideDef ReadFromWadData(BinaryReader reader, List<Sector> sectors)
    {
        var textureOffset = reader.ReadInt16();
        var rowOffset = reader.ReadInt16();
        var topTexture = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        var bottomTexture = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        var midTexture = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        var sector = reader.ReadInt16();

        return new SideDef(
            Fixed.FromInt(textureOffset),
            Fixed.FromInt(rowOffset),
            sectors[sector]
        )
        {
            TopTexture = DoomGame.Instance.Renderer.TextureNumForName(topTexture),
            BottomTexture = DoomGame.Instance.Renderer.TextureNumForName(bottomTexture),
            MidTexture = DoomGame.Instance.Renderer.TextureNumForName(midTexture),
        };
    }
}

public enum SlopeType
{
    Horizontal,
    Vertical,
    Positive,
    Negative
}

public class Line
{
    public Line(Vertex v1, Vertex v2, short flags, short special, short tag)
    {
        V1 = v1;
        V2 = v2;
        Flags = flags;
        Special = special;
        Tag = tag;

        Dx = v2.X - v1.X;
        Dy = v2.Y - v1.Y;
    }

    // Vertices, from v1 to v2.
    public Vertex V1 { get; }
    public Vertex V2 { get; }

    // Precalculated v2 - v1 for side checking
    public Fixed Dx { get; }
    public Fixed Dy { get; }

    // Animation related
    public short Flags { get; set; }
    public short Special { get; set; }
    public short Tag { get; set; }

    // Visual appearance: SideDefs.
    //  SideNum[1] will be -1 if one sided.
    public short[] SideNum { get; } = new short[2];

    // Neat. Another bounding box, for the extent
    //  of the LineDef.
    public Fixed[] BoundingBox { get; } = new Fixed[4];

    // To aid move clipping.
    public SlopeType SlopeType { get; set; }

    // Front and back sector.
    // Note: redundant? Can be retrieved from SideDefs.
    public Sector? FrontSector { get; set; }
    public Sector? BackSector { get; set; }

    // if == validcount, already checked
    public int ValidCount { get; set; }

    // thinker_t for reversable actions
    public Thinker? SpecialData { get; set; }

    public static Line ReadFromWadData(BinaryReader reader, Vertex[] vertices)
    {
        var v1 = reader.ReadInt16();
        var v2 = reader.ReadInt16();
        var flags = reader.ReadInt16();
        var special = reader.ReadInt16();
        var tag = reader.ReadInt16();

        return new Line(vertices[v1], vertices[v2], flags, special, tag)
        {
            SideNum =
            {
                [0] = reader.ReadInt16(),
                [1] = reader.ReadInt16()
            }
        };
    }

    /// <summary>
    /// Returns the sector next to the current one, or <c>null</c> if not two-sided line.
    /// </summary>
    public Sector? GetNextSector(Sector sector)
    {
        if ((Flags & Constants.Line.TwoSided) == 0)
        {
            return null;
        }

        return FrontSector == sector ? BackSector : FrontSector;
    }
}

/// <summary>
/// A SubSector.
/// References a Sector.
/// Basically, this is a list of LineSegs,
///  indicating the visible walls that define
///  (all or some) sides of a convex BSP leaf.
/// </summary>
public record SubSector(short NumLines, short FirstLine)
{
    public Sector? Sector { get; set; }

    public static SubSector ReadFromWadData(BinaryReader reader)
    {
        return new SubSector(reader.ReadInt16(), reader.ReadInt16());
    }
}

/// <summary>
/// The LineSeg
/// </summary>
public record Segment(Vertex V1, Vertex V2, Fixed Offset, Angle Angle, SideDef SideDef, Line LineDef, Sector FrontSector, Sector? BackSector)
{
    public static Segment ReadFromWadData(BinaryReader reader, Vertex[] vertices, SideDef[] sides, Line[] lines)
    {
        var v1 = vertices[reader.ReadInt16()];
        var v2 = vertices[reader.ReadInt16()];

        var angle = reader.ReadInt16() << 16;
        var lineDef = lines[reader.ReadInt16()];

        var side = reader.ReadInt16();
        var sideDef = sides[lineDef.SideNum[side]];
        var offset = Fixed.FromInt(reader.ReadInt16());

        var frontSector = sideDef.Sector;
        Sector? backSector = null;
        if ((lineDef.Flags & Constants.Line.TwoSided) != 0)
        {
            backSector = sides[lineDef.SideNum[side ^ 1]].Sector;
        }

        return new Segment(v1, v2, offset, new Angle(angle), sideDef, lineDef, frontSector, backSector);
    }
}

/// <summary>
/// BSP Node
/// </summary>
public class Node : DividerLine
{
    public Node(Fixed x, Fixed y, Fixed dx, Fixed dy)
    {
        X = x;
        Y = y;
        Dx = dx;
        Dy = dy;

        BoundingBox = new Fixed[2][];
        for (var i = 0; i < 2; i++)
        {
            BoundingBox[i] = new Fixed[4];
        }
    }

    /// <summary>
    /// Bounding box for each child.
    /// </summary>
    public Fixed[][] BoundingBox { get; }

    /// <summary>
    /// If NF_SUBSECTOR its a subsector.
    /// </summary>
    public ushort[] Children { get; } = new ushort[2];

    public static Node ReadFromWadData(BinaryReader reader)
    {
        var x = Fixed.FromInt(reader.ReadInt16());
        var y = Fixed.FromInt(reader.ReadInt16());
        var dx = Fixed.FromInt(reader.ReadInt16());
        var dy = Fixed.FromInt(reader.ReadInt16());

        var node = new Node(x, y, dx, dy);

        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                node.BoundingBox[i][j] = Fixed.FromInt(reader.ReadInt16());
            }
        }

        for (var i = 0; i < 2; i++)
        {
            node.Children[i] = reader.ReadUInt16();
        }

        return node;
    }
}