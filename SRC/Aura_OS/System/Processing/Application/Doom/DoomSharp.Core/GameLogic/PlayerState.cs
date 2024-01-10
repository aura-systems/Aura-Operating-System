namespace DoomSharp.Core.GameLogic;

public enum PlayerState
{
    NotSet,
    /// <summary>
    /// Playing or camping
    /// </summary>
    Alive,
    /// <summary>
    /// Dead on the ground, view follows killer
    /// </summary>
    Dead,
    /// <summary>
    /// Ready to restart/respawn
    /// </summary>
    Reborn
}