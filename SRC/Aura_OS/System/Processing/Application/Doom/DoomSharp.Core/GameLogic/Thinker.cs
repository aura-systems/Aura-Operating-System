using System;

namespace DoomSharp.Core.GameLogic;

public abstract class Thinker
{
    public Action<ActionParams>? Action { get; set; }
}

public record ActionParams(MapObject? MapObject = null, Player? Player = null, PlayerSprite? PlayerSprite = null, Thinker? Thinker = null);