namespace DoomSharp.Core.GameLogic;

public enum FloorType
{
    /// <summary>
    /// lower floor to highest surrounding floor
    /// </summary>
    LowerFloor,

    /// <summary>
    /// lower floor to lowest surrounding floor
    /// </summary>
    LowerFloorToLowest,

    /// <summary>
    /// lower floor to highest surrounding floor VERY FAST
    /// </summary>
    TurboLower,

    /// <summary>
    /// raise floor to lowest surrounding ceiling
    /// </summary>
    RaiseFloor,

    /// <summary>
    /// raise floor to next highest surrounding floor
    /// </summary>
    RaiseFloorToNearest,

    /// <summary>
    /// raise floor to shortest height texture around it
    /// </summary>
    RaiseToTexture,

    /// <summary>
    /// lower floor to lowest surrounding floor and change floorpic
    /// </summary>
    LowerAndChange,

    RaiseFloor24,
    RaiseFloor24AndChange,
    RaiseFloorCrush,

    /// <summary>
    /// raise to next highest floor, turbo-speed
    /// </summary>
    RaiseFloorTurbo,
    DonutRaise,
    RaiseFloor512
}