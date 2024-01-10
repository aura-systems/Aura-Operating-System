namespace DoomSharp.Core.GameLogic;

public record SwitchControl(string Name1, string Name2, int Episode)
{
    public static readonly SwitchControl[] PredefinedSwitchList =
    {
        // Doom shareware episode 1 switches
        new("SW1BRCOM", "SW2BRCOM", 1),
        new("SW1BRN1", "SW2BRN1", 1),
        new("SW1BRN2", "SW2BRN2", 1),
        new("SW1BRNGN", "SW2BRNGN", 1),
        new("SW1BROWN", "SW2BROWN", 1),

        new("SW1COMM", "SW2COMM", 1),
        new("SW1COMP", "SW2COMP", 1),
        new("SW1DIRT", "SW2DIRT", 1),
        new("SW1EXIT", "SW2EXIT", 1),
        new("SW1GRAY", "SW2GRAY", 1),
        new("SW1GRAY1", "SW2GRAY1", 1),
        new("SW1METAL", "SW2METAL", 1),
        new("SW1PIPE", "SW2PIPE", 1),
        new("SW1SLAD", "SW2SLAD", 1),
        new("SW1STARG", "SW2STARG", 1),
        new("SW1STON1", "SW2STON1", 1),
        new("SW1STON2", "SW2STON2", 1),
        new("SW1STONE", "SW2STONE", 1),
        new("SW1STRTN", "SW2STRTN", 1),

        // Doom registered episodes 2&3 switches
        new("SW1BLUE", "SW2BLUE", 2),
        new("SW1CMT", "SW2CMT", 2),
        new("SW1GARG", "SW2GARG", 2),
        new("SW1GSTON", "SW2GSTON", 2),
        new("SW1HOT", "SW2HOT", 2),
        new("SW1LION", "SW2LION", 2),
        new("SW1SATYR", "SW2SATYR", 2),
        new("SW1SKIN", "SW2SKIN", 2),
        new("SW1VINE", "SW2VINE", 2),
        new("SW1WOOD", "SW2WOOD", 2),

        // Doom II switches
        new("SW1PANEL", "SW2PANEL", 3),
        new("SW1ROCK", "SW2ROCK", 3),
        new("SW1MET2", "SW2MET2", 3),
        new("SW1WDMET", "SW2WDMET", 3),
        new("SW1BRIK", "SW2BRIK", 3),
        new("SW1MOD1", "SW2MOD1", 3),
        new("SW1ZIM", "SW2ZIM", 3),
        new("SW1STON6", "SW2STON6", 3),
        new("SW1TEK", "SW2TEK", 3),
        new("SW1MARB", "SW2MARB", 3),
        new("SW1SKULL", "SW2SKULL", 3),

        new("", "", 0)
    };
}