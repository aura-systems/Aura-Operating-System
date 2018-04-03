/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE ICanvas
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Shell.VBE.CosmosGLGraphics
{
    public interface ICanvas
    {
        int Height { get; set; }
        int Width { get; set; }

        void Clear(uint c);
        Color GetPixel(int x, int y);
        void SetPixel(int x, int y, Color c);
        void SetPixel(int x, int y, uint c);
        void WriteToScreen();
        void SetScanLine(int offset, int length, uint color);
    }
}
