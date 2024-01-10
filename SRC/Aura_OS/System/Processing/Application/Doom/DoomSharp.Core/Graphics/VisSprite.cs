using DoomSharp.Core.GameLogic;

namespace DoomSharp.Core.Graphics;

/// <summary>
/// A vissprite_t is a thing
///  that will be drawn during a refresh.
/// I.e. a sprite object that is partly visible.
/// </summary>
public class VisSprite
{
    // Doubly linked list.
    public VisSprite? Prev { get; set; }
    public VisSprite? Next { get; set; }
    
    public int X1 { get; set; }
    public int X2 { get; set; }

    // for line side calculation
    public Fixed GX { get; set; }
    public Fixed GY { get; set; }

    // global bottom / top for silhouette clipping
    public Fixed GZ { get; set; }
    public Fixed GZTop { get; set; }

    // horizontal position of x1
    public Fixed StartFrac { get; set; }

    public Fixed Scale { get; set; }

    // negative if flipped
    public Fixed XiScale { get; set; }

    public Fixed TextureMid { get; set; }
    public int Patch { get; set; }

    // for color translation and shadow draw,
    //  maxbright frames as well
    public byte[]? Colormap { get; set; }

    public MapObjectFlag MapObjectFlags { get; set; }
}