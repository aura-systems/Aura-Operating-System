/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE VESA Graphics
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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