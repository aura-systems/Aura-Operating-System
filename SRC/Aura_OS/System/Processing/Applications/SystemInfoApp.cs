/*
* PROJECT:          Aura Operating System Development
* CONTENT:          System information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;

namespace Aura_OS.System.Processing.Applications
{
    public class SystemInfoApp : Application
    {
        public static string ApplicationName = "SystemInfo";

        public Color _greenPen = Color.Green;

        const string _title = "Aura Operating System";
        const string _credit = "Created by Alexy DA CRUZ and Valentin CHARBONNIER.";
        const string _website = "Project: github.com/aura-systems/Aura-Operating-System";
        const string _website2 = "Kernel: github.com/CosmosOS/Cosmos";

        public SystemInfoApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {

        }

        public override void Draw()
        {
            base.Draw();

            var version = "[version " + Kernel.Version + "-" + Kernel.Revision + "]";

            Kernel.canvas.DrawString(_title, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, Kernel.BlackColor, x + width / 2 - _title.Length * Cosmos.System.Graphics.Fonts.PCScreenFont.Default.Width / 2, y + 1 * Kernel.font.Height);
            Kernel.canvas.DrawString(version, Kernel.font, Kernel.BlackColor, x + width / 2 - version.Length * Kernel.font.Width / 2, y + 3 * Kernel.font.Height);

            Kernel.canvas.DrawString(_credit, Kernel.font, Kernel.BlackColor, x + width / 2 - _credit.Length * Kernel.font.Width / 2, y + 5 * Kernel.font.Height);
            Kernel.canvas.DrawString(_website, Kernel.font, _greenPen, x + width / 2 - _website.Length * Kernel.font.Width / 2, y + 7 * Kernel.font.Height);

            Kernel.canvas.DrawImageAlpha(Kernel.AuraLogo, x + width / 2 - (int)Kernel.AuraLogo.Width / 2, y + 9 * Kernel.font.Height);

            Kernel.canvas.DrawString(_website2, Kernel.font, _greenPen, x + width / 2 - _website2.Length * Kernel.font.Width / 2, y + 19 * Kernel.font.Height);
            Kernel.canvas.DrawImageAlpha(Kernel.CosmosLogo, x + width / 2 - (int)Kernel.CosmosLogo.Width / 2, y + 21 * Kernel.font.Height);
        }
    }
}
