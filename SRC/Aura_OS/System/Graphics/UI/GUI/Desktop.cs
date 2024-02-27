/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI.Components;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public FilesystemPanel MainPanel;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            MainPanel = new FilesystemPanel(Kernel.CurrentVolume, Color.White, x + 4, y + 4, width - 7 - 75, height - Taskbar.taskbarHeight);
            MainPanel.OpenNewWindow = true;
            MainPanel.Borders = false;
            MainPanel.Background = false;

            MainPanel.Visible = false;

            MainPanel.UpdateCurrentFolder();

            AddChild(MainPanel);
        }

        public override void Draw()
        {
            base.Draw();

            DrawImage(Kernel.wallpaper, X, Y);

            MainPanel.UpdateCurrentFolder();


            //MainPanel.Draw(this);
        }
    }
}