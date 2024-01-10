namespace DoomSharp.Core.GameLogic;

public class WorldMapInfo
{
    public WorldMapInfo()
    {
        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            Players[i] = new WorldMapPlayer();
        }
    }

    /// <summary>
    /// Episode # (0 - 2)
    /// </summary>
    public int Episode { get; set; }

    /// <summary>
    /// If true, splash the secret level
    /// </summary>
    public bool DidSecret { get; set; }

    // previous and next levels, origin 0
    public int Last { get; set; }
    public int Next { get; set; }

    public int MaxKills { get; set; }
    public int MaxItems { get; set; }
    public int MaxSecret { get; set; }
    public int MaxFrags { get; set; }

    /// <summary>
    /// The par time
    /// </summary>
    public int ParTime { get; set; }

    /// <summary>
    /// The index of this player in game
    /// </summary>
    public int PlayerNum { get; set; }

    public WorldMapPlayer[] Players { get; } = new WorldMapPlayer[Constants.MaxPlayers];
}