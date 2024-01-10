using System;

namespace DoomSharp.Core.GameLogic;

[Flags]
public enum Cheat
{
    /// <summary>
    /// No clipping, walk through barriers
    /// </summary>
    NoClip = 1,
    /// <summary>
    /// No damage, no health loss
    /// </summary>
    GodMode = 2,
    /// <summary>
    /// Not really a cheat, just a debug aid
    /// </summary>
    NoMomentum = 4
}