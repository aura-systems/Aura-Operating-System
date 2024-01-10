namespace DoomSharp.Core.GameLogic;

public enum MapThingFlag
{
    MTF_EASY = 1,
    MTF_NORMAL = 2,
    MTF_HARD = 4,

    // Deaf monsters/do not react to sound.
    MTF_AMBUSH = 8
}