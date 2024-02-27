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

            DrawString(_title, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, Kernel.BlackColor, 0 + Width / 2 - _title.Length * Cosmos.System.Graphics.Fonts.PCScreenFont.Default.Width / 2, 0 + 1 * Kernel.font.Height);
            DrawString(version, Kernel.font, Kernel.BlackColor, 0 + Width / 2 - version.Length * Kernel.font.Width / 2, 0 + 2 * Kernel.font.Height);

            DrawString(_credit, Kernel.font, Kernel.BlackColor, 0 + Width / 2 - _credit.Length * Kernel.font.Width / 2, 0 + 5 * Kernel.font.Height);
            DrawString(_website, Kernel.font, _greenPen, 0 + Width / 2 - _website.Length * Kernel.font.Width / 2, 0 + 6 * Kernel.font.Height);

            DrawImage(Kernel.AuraLogo, 0 + Width / 2 - (int)Kernel.AuraLogo.Width / 2, 0 + 8 * Kernel.font.Height);

            DrawString(_website2, Kernel.font, _greenPen, 0 + Width / 2 - _website2.Length * Kernel.font.Width / 2, 0 + 18 * Kernel.font.Height);
            DrawImage(Kernel.CosmosLogo, 0 + Width / 2 - (int)Kernel.CosmosLogo.Width / 2, 0 + 20 * Kernel.font.Height);
        }
    }
}
