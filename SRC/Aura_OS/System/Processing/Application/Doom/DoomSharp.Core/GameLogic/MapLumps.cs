namespace DoomSharp.Core.GameLogic;

public static class MapLumps
{
    /// <summary>
    /// A separator, name, ExMx or MAPxx
    /// </summary>
    public const int Label = 0;

    /// <summary>
    /// Monsters, items, ...
    /// </summary>
    public const int Things = 1;

    /// <summary>
    /// LineDefs
    /// </summary>
    public const int LineDefs = 2;

    /// <summary>
    /// SideDefs
    /// </summary>
    public const int SideDefs = 3;

    /// <summary>
    /// Vertices, edited and BSP splits generated
    /// </summary>
    public const int Vertices = 4;

    /// <summary>
    /// LineSegs, from LineDefs split by BSP
    /// </summary>
    public const int Segs = 5;

    /// <summary>
    /// SubSectors, list of LineSegs
    /// </summary>
    public const int SubSectors = 6;

    /// <summary>
    /// BSP nodes
    /// </summary>
    public const int Nodes = 7;

    /// <summary>
    /// Sectors
    /// </summary>
    public const int Sectors = 8;

    /// <summary>
    /// LUT, sector-sector visibility
    /// </summary>
    public const int Reject = 9;

    /// <summary>
    /// LUT, motion clipping, walls/grid element
    /// </summary>
    public const int BlockMap = 10;
}