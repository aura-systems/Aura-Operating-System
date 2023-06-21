using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System;
using System.Drawing;
using Cosmos.System.Graphics;

namespace Aura_OS
{
    public class SystemInfo : App
    {
        public Color GreenPen = Color.Green;

        public SystemInfo(int width, int height, int x = 0, int y = 0) : base("SystemInfo", width, height, x, y)
        {
        }

        public override void UpdateApp()
        {
            Kernel.canvas.DrawString("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]", Kernel.font, Kernel.BlackColor, (int)x, (int)y);
            Kernel.canvas.DrawString("Created by Alexy DA CRUZ and Valentin CHARBONNIER.", Kernel.font, Kernel.BlackColor, (int)x, (int)y + (1 * Kernel.font.Height));
            Kernel.canvas.DrawString("Website: github.com/aura-systems", Kernel.font, GreenPen, (int)x, (int)y + (3 * Kernel.font.Height));

            Kernel.canvas.DrawImage(Kernel.AuraLogo, (int)x + 8, (int)y + (4 * Kernel.font.Height));
            Kernel.canvas.DrawImage(Kernel.CosmosLogo, (int)(x + 8 + Kernel.AuraLogo.Width + 16), (int)y + (4 * Kernel.font.Height) + 16);
        }
    }
}
