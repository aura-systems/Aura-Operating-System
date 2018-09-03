namespace Aura_OS.System.GUI.Graphics
{
    public interface ICanvas
    {
        int Height { get; set; }
        int Width { get; set; }
        uint Blit(int x0, int y0, int w, int h);
        void DeBlit(int x0, int y0, int w, int h, uint value);
        void Clear(uint c);
        Color GetPixel(int x, int y);
        void SetPixel(int x, int y, Color c);
        void SetPixel(int x, int y, uint c);
        void WriteToScreen();
        void SetScanLine(int offset, int length, uint color);
    }
}