/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using System.IO;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Desktop : Component
    {
        public FilesystemPanel MainPanel;
        private string _wallpaperPath;
        private Image _wallpaper;
        private Image _wallpaperSource;

        public Desktop(int x, int y, int width, int height) : base(x, y, width, height)
        {
            if (Kernel.Installed)
            {
                CustomConsole.WriteLineInfo("Retrieving wallpaper from " + AuraPaths.SettingsIni + ".");
                Settings config = new Settings(AuraPaths.SettingsIni);
                string wallpaperPath = config.GetValue("wallpaperPath");

                try
                {
                    SetWallpaper(wallpaperPath);
                }
                catch (Exception ex)
                {
                    // GEN3-GAP(bmp): a wallpaper the gen3 BMP loader rejects must not stop the boot.
                    CustomConsole.WriteLineWarning("Cannot load wallpaper " + wallpaperPath + ": " + ex.Message);
                    _wallpaperPath = string.IsNullOrEmpty(wallpaperPath) ? "Embedded" : AuraPath.FromLegacy(wallpaperPath);
                    _wallpaperSource = Kernel.wallpaper1;
                    _wallpaper = ImageUtils.ScaleToScreen(_wallpaperSource);
                    MarkDirty();
                }
            }
            else
            {
                _wallpaperPath = "Embedded";
                // Kernel.wallpaper1 is Files.Wallpaper, already decoded at boot (Files.LoadFiles).
                _wallpaperSource = Kernel.wallpaper1;
                _wallpaper = ImageUtils.ScaleToScreen(_wallpaperSource);
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

        /// <summary>
        /// Loads and shows a wallpaper. A missing file falls back to the embedded wallpaper;
        /// a BMP the loader rejects throws (SettingsApp reports it) and keeps the current one.
        /// </summary>
        public void SetWallpaper(string path)
        {
            // gen2-installed disks store DOS-style paths (drive 0, backslashes): convert them.
            path = AuraPath.FromLegacy(path);

            Image wallpaper;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                wallpaper = new Bitmap(File.ReadAllBytes(path));
            }
            else
            {
                wallpaper = Kernel.wallpaper1;
            }

            // gen2 blanked to black when the wallpaper did not match the screen; gen3 runs at the
            // real framebuffer size (GEN3-GAP(display-mode)), so scale once here, never per frame.
            _wallpaper = ImageUtils.ScaleToScreen(wallpaper);
            _wallpaperSource = wallpaper;
            _wallpaperPath = string.IsNullOrEmpty(path) ? "Embedded" : path;

            MarkDirty();
        }

        /// <summary>
        /// Fits the desktop and its wallpaper to the screen after a resolution change.
        /// </summary>
        public void ResizeToScreen()
        {
            SetSize((int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);
            MainPanel.SetSize(Width - 7 - 75, Height - Taskbar.taskbarHeight);

            // Drop the old scaled wallpaper first, so a collection during the scaling can reclaim it.
            _wallpaper = null;
            _wallpaper = ImageUtils.ScaleToScreen(_wallpaperSource);
        }
    }
}