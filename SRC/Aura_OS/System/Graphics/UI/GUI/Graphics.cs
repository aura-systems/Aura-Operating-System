/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Graphics
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class GUI
    {
        public void WriteByte(char ch, int mX, int mY, Color color)
        {
            Kernel.canvas.DrawChar(ch, Kernel.font, color, mX, mY);
        }

        public void SetCursorPos(int mX, int mY)
        {
            if (Kernel.console.CursorVisible)
            {
                Kernel.canvas.DrawFilledRectangle(Kernel.console.ForegroundColor, Kernel.console.x + mX * Kernel.font.Width,
                    Kernel.console.y + mY * Kernel.font.Height + Kernel.font.Height, 8, 4);
            }
        }
    }
}