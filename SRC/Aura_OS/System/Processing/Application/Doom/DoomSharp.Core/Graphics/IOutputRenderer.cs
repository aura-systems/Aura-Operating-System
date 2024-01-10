namespace DoomSharp.Core.Graphics;

public interface IGraphics
{
    void Initialize();
    void UpdatePalette(byte[] palette);
    void ScreenReady(byte[] output);
    void StartTic();
}