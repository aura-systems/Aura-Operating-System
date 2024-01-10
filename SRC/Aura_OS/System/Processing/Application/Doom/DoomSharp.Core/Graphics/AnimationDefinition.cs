namespace DoomSharp.Core.Graphics;

public record AnimationDefinition(bool IsTexture, string EndName, string StartName, int Speed)
{
    /// <summary>
    /// Floor/ceiling animation sequences,
    ///  defined by first and last frame,
    ///  i.e. the flat (64x64 tile) name to
    ///  be used.
    /// The full animation sequence is given
    ///  using all the flats between the start
    ///  and end entry, in the order found in
    ///  the WAD file.
    /// </summary>
    public static readonly AnimationDefinition[] Definitions =
    {
        new(false, "NUKAGE3",  "NUKAGE1",  8),
        new(false, "FWATER4",  "FWATER1",  8),
        new(false, "SWATER4",  "SWATER1",  8),
        new(false, "LAVA4",    "LAVA1",    8),
        new(false, "BLOOD3",   "BLOOD1",   8),

        // DOOM II flat animations.
        new(false, "RROCK08",  "RROCK05",  8),
        new(false, "SLIME04",  "SLIME01",  8),
        new(false, "SLIME08",  "SLIME05",  8),
        new(false, "SLIME12",  "SLIME09",  8),

        new(true,  "BLODGR4",  "BLODGR1",  8),
        new(true,  "SLADRIP3", "SLADRIP1", 8),

        new(true,  "BLODRIP4", "BLODRIP1", 8),
        new(true,  "FIREWALL", "FIREWALA", 8),
        new(true,  "GSTFONT3", "GSTFONT1", 8),
        new(true,  "FIRELAVA", "FIRELAV3", 8),
        new(true,  "FIREMAG3", "FIREMAG1", 8),
        new(true,  "FIREBLU2", "FIREBLU1", 8),
        new(true,  "ROCKRED3", "ROCKRED1", 8),

        new(true,  "BFALL4",   "BFALL1",   8),
        new(true,  "SFALL4",   "SFALL1",   8),
        new(true,  "WFALL4",   "WFALL1",   8),
        new(true,  "DBRAIN4",  "DBRAIN1",  8),
    };
}