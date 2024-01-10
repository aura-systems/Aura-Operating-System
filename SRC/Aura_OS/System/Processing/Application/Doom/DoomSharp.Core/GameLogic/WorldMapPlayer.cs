namespace DoomSharp.Core.GameLogic;

public class WorldMapPlayer
{
    /// <summary>
    /// Whether the player is in game
    /// </summary>
    public bool In { get; set; }

    // player stats, kills, collected items etc.
    public int Kills { get; set; }
    public int Items { get; set; }
    public int Secret { get; set; }
    public int Time { get; set; }
    public int[] Frags { get; } = new int[4];
    
    /// <summary>
    /// Current score on entry, modified on return
    /// </summary>
    public int Score { get; set; }
}