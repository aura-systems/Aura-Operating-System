/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using System.IO;
using Cosmos.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public FilesystemPanel MainPanel;
        private string _wallpaperPath;
        private Bitmap _wallpaper;
        private Color? _color = null;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            if (Kernel.Installed)
            {
                CustomConsole.WriteLineInfo("Retrieving wallpaper from 0:\\.");
                Settings config = new Settings(@"0:\System\settings.ini");
                SetWallpaper(config.GetValue("wallpaperPath"));
            }
            else
            {
                _wallpaperPath = "Embedded";
                _wallpaper = new Bitmap(Files.Wallpaper);
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

            if (_color != null)
            {
                Clear((Color)_color);
            }
            else
            {
                DrawImage(_wallpaper, X, Y);
            }

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
            _wallpaper = new Bitmap(File.ReadAllBytes(path));

            if (_wallpaper.Width != Kernel.ScreenWidth && _wallpaper.Height != Kernel.ScreenHeight)
            {
                _color = Color.Black;
            }

            MarkDirty();
        }
    }
}