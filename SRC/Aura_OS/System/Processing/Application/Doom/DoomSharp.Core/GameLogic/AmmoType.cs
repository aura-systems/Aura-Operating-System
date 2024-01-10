namespace DoomSharp.Core.GameLogic;

public enum AmmoType
{
    /// <summary>
    /// Pistol / chaingun ammo.
    /// </summary>
    Clip = 0,
    /// <summary>
    /// Shotgun / double barreled shotgun.
    /// </summary>
    Shell,
    /// <summary>
    /// Plasma rifle, BFG
    /// </summary>
    Cell,
    /// <summary>
    /// Missile launcher
    /// </summary>
    Missile,
    NumAmmo,
    /// <summary>
    /// Unlimited for chainsaw / fist.
    /// </summary>
    NoAmmo
}