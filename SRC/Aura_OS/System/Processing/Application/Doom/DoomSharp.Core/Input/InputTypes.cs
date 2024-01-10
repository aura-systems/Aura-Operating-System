using System;

namespace DoomSharp.Core.Input;


// Data1: keys / mouse/joystick buttons
// Data2: mouse/joystick x move
// Data3: mouse/joystick y move
public record InputEvent(EventType Type, int Data1, int Data2, int Data3);

public enum EventType
{
    KeyDown,
    KeyUp,
    Mouse,
    Joystick
}

[Flags]
public enum ButtonCode
{
    /// <summary>
    /// Press "Fire".
    /// </summary>
    Attack = 1,
    /// <summary>
    /// Use button, to open doors, activate switches.
    /// </summary>
    Use = 2,
    
    /// <summary>
    /// Flag: game events, not really buttons.
    /// </summary>
    Special = 128,
    SpecialMask = 3,

    /// <summary>
    /// Flag, weapon change pending.
    /// If true, the next 3 bits hold weapon num.
    /// </summary>
    Change = 4,
    /// <summary>
    /// The 3 bit weapon mask and shift, convenience.
    /// </summary>
    WeaponMask = 8+16+32,
    WeaponShift = 3,
    
    /// <summary>
    /// Pause the game.
    /// </summary>
    Pause = 1,
    /// <summary>
    /// Save the game at each console.
    /// </summary>
    SaveGame = 2,

    /// <summary>
    /// Savegame slot numbers
    /// Occupy the second byte of buttons.
    /// </summary>
    SaveMask = 4+8+16,
    SaveShift = 2
}

public enum Keys
{
    RightArrow = 0xae,
    LeftArrow = 0xac,
    UpArrow = 0xad,
    DownArrow = 0xaf,
    Escape = 27,
    Enter = 13,
    Tab = 9,
    F1 = 0x80 + 0x3b,
    F2 = 0x80 + 0x3c,
    F3 = 0x80 + 0x3d,
    F4 = 0x80 + 0x3e,
    F5 = 0x80 + 0x3f,
    F6 = 0x80 + 0x40,
    F7 = 0x80 + 0x41,
    F8 = 0x80 + 0x42,
    F9 = 0x80 + 0x43,
    F10 = 0x80 + 0x44,
    F11 = 0x80 + 0x57,
    F12 = 0x80 + 0x58,
    
    Backspace = 127,
    Pause = 0xff,

    Equals = 0x3d,
    Minus = 0x2d,

    RShift = 0x80 + 0x36,
    RCtrl = 0x80 + 0x1d,
    RAlt = 0x80 + 0x38,

    LAlt = RAlt
}