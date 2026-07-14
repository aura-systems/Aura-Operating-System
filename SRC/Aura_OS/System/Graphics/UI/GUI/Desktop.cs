/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using System.IO;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public FilesystemPanel MainPanel;
        private string _wallpaperPath;
        private Bitmap _wallpaper;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            if (Kernel.Installed)
            {
                CustomConsole.WriteLineInfo("Retrieving wallpaper from " + Kernel.RootVolume + ".");
                Settings config = new Settings(Kernel.RootVolume + "/System/settings.ini");
                SetWallpaper(config.GetValue("wallpaperPath"));
            }
            else
            {
                _wallpaperPath = "Embedded";
                _wallpaper = ImageUtils.ScaleToScreen(new Bitmap(Files.Wallpaper));
                MarkDirty();
            }

            MainPanel = new FilesystemPanel(Kernel.CurrentVolume, Color.White, x + 4, y + 4, width - 7 - 75, height - Taskbar.taskbarHeight);
            MainPanel.OpenNewWindow = true;
            MainPanel.Borders = false;
            MainPanel.Background = false;

            MainPanel.UpdateCurrentFolder();

            AddChild(MainPanel);
        }

        public override void Draw()
        {
            base.Draw();

            DrawImage(_wallpaper, X, Y);

            MainPanel.UpdateCurrentFolder();
            MainPanel.Draw(this);
        }

        public string GetWallpaperPath()
        {
            return _wallpaperPath;
        }

        public void SetWallpaper(string path)
        {
            _wallpaperPath = path;
            // gen2 blanked to black when the wallpaper didn't match the screen;
            // gen3 runs at the real framebuffer size, so scale instead.
            _wallpaper = ImageUtils.ScaleToScreen(new Bitmap(File.ReadAllBytes(path)));

            MarkDirty();
        }
    }
}