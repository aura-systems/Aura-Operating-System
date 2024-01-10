namespace DoomSharp.Core.Graphics;

public enum WipeMethod
{
    /// <summary>
    /// Simple gradual pixel change for 8-bit only
    /// </summary>
    ColorTransform = 0,
    
    /// <summary>
    /// Weird screen melt
    /// </summary>
    Melt = 1
}