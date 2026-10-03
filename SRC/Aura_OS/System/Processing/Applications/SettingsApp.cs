/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Settings application. The window is Resources/UI/Layouts/Settings.xml.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Processing.Applications
{
    public class SettingsApp : Application
    {
        private TextBox _username;
        private TextBox _computerName;
        private TextBox _themeBmpPath;
        private TextBox _themeXmlPath;
        private Slider _windowsAlpha;
        private Slider _taskbarAlpha;
        private DropDown _resolution;
        private DropDown _scale;
        private TextBox _wallpaperPath;
        private Checkbox _guiDebug;
        private Checkbox _autoLogin;
        private Dialog _dialog;

        private List<Mode> _modes;
        private int _oldScreenWidth;
        private int _oldScreenHeight;
        private string _oldWallpaperPath;

        public SettingsApp(int x = 0, int y = 0) : base(AppLayout.Load("Settings"), x, y)
        {
            _username = Find<TextBox>("username");
            _computerName = Find<TextBox>("computerName");
            _themeBmpPath = Find<TextBox>("themeBmpPath");
            _themeXmlPath = Find<TextBox>("themeXmlPath");
            _windowsAlpha = Find<Slider>("windowsAlpha");
            _taskbarAlpha = Find<Slider>("taskbarAlpha");
            _resolution = Find<DropDown>("resolution");
            _scale = Find<DropDown>("scale");
            _wallpaperPath = Find<TextBox>("wallpaperPath");
            _guiDebug = Find<Checkbox>("guiDebug");
            _autoLogin = Find<Checkbox>("autoLogin");
            _dialog = Find<Dialog>("dialog");

            On("save", Save);
            On("closeDialog", CloseDialog);

            // The display resolution: Kernel.ScreenWidth/Height are the UI size, divided by the scale.
            _oldScreenWidth = Kernel.Canvas.Width;
            _oldScreenHeight = Kernel.Canvas.Height;

            // A null TextBox.Text would be a null deref (kernel halt) on the first draw
            _username.Text = Kernel.userLogged ?? "";
            _computerName.Text = Kernel.ComputerName ?? "";
            _themeBmpPath.Text = Kernel.ThemeManager.BmpPath ?? "";
            _themeXmlPath.Text = Kernel.ThemeManager.XmlPath ?? "";
            _windowsAlpha.Value = Explorer.WindowManager.WindowsTransparency;
            _taskbarAlpha.Value = Explorer.WindowManager.TaskbarTransparency;
            LoadResolutions();
            LoadScales();
            _wallpaperPath.Text = Explorer.Desktop.GetWallpaperPath() ?? "";
            _oldWallpaperPath = _wallpaperPath.Text;
            _guiDebug.Checked = Kernel.GuiDebug;

            // Auto log in is kept in settings.ini, which a live system does not have.
            if (Kernel.Installed)
            {
                Settings config = new Settings(AuraPaths.SettingsIni);
                _autoLogin.Checked = config.GetValue("autologin") == "true";
            }
            else
            {
                Layout.SetVisible("autoLoginRow", false);
            }
        }

        private void Save()
        {
            // Reset a previous error.
            _dialog.SetState(DialogState.Information);
            _dialog.Message = "Settings updated.";

            Kernel.userLogged = _username.Text;
            Kernel.ComputerName = _computerName.Text;
            if (!string.IsNullOrEmpty(Kernel.ComputerName))
            {
                Cosmos.Kernel.System.Network.Config.DnsConfig.HostName = Kernel.ComputerName;
            }
            Kernel.ThemeManager.BmpPath = NormalizePath(_themeBmpPath.Text);
            Kernel.ThemeManager.XmlPath = NormalizePath(_themeXmlPath.Text);
            Explorer.WindowManager.WindowsTransparency = (byte)_windowsAlpha.Value;
            Explorer.WindowManager.TaskbarTransparency = (byte)_taskbarAlpha.Value;
            Kernel.GuiDebug = _guiDebug.Checked;

            string wallpaperPath = NormalizePath(_wallpaperPath.Text);
            string wallpaperError = null;

            // A missing file or a BMP the gen3 loader rejects would throw out of Kernel.Run (crash screen)
            if (_oldWallpaperPath != _wallpaperPath.Text)
            {
                if (!File.Exists(wallpaperPath))
                {
                    wallpaperError = "Wallpaper path is not valid.";
                }
                else
                {
                    try
                    {
                        Explorer.Desktop.SetWallpaper(wallpaperPath);
                        _oldWallpaperPath = _wallpaperPath.Text;
                    }
                    catch (Exception)
                    {
                        // GEN3-GAP(bmp): no top-down, bitfield or < 24 bpp BMPs
                        wallpaperError = "Wallpaper could not be loaded.";
                    }
                }
            }

            string resolutionError = wallpaperError == null ? ApplyResolution() : null;

            if (wallpaperError != null || resolutionError != null)
            {
                _dialog.SetState(DialogState.Error);
                _dialog.Message = wallpaperError ?? resolutionError;
                _dialog.MarkDirty();
            }
            else if (Kernel.Installed)
            {
                if (UpdateDialog())
                {
                    Settings config = new Settings(AuraPaths.SettingsIni);
                    config.EditValue("hostname", Kernel.ComputerName);
                    config.EditValue("themeBmpPath", Kernel.ThemeManager.BmpPath);
                    config.EditValue("themeXmlPath", Kernel.ThemeManager.XmlPath);
                    config.EditValue("windowsTransparency", Explorer.WindowManager.WindowsTransparency.ToString());
                    config.EditValue("taskbarTransparency", Explorer.WindowManager.TaskbarTransparency.ToString());
                    config.EditValue("screenWidth", _oldScreenWidth.ToString());
                    config.EditValue("screenHeight", _oldScreenHeight.ToString());
                    config.EditValue("screenScale", Kernel.ScreenScale.ToString());
                    config.EditValue("wallpaperPath", wallpaperPath);
                    config.EditValue("autologin", _autoLogin.Checked ? "true" : "false");
                    config.Push();
                }
            }

            _dialog.Visible = true;

            MarkDirty();
        }

        private void CloseDialog()
        {
            _dialog.Visible = false;

            MarkDirty();
        }

        /// <summary>
        /// User-typed path to an absolute gen3 path (gen2 DOS-style input accepted, see AuraPath.FromLegacy).
        /// The embedded: theme sentinel and empty input are returned unchanged.
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || Files.IsEmbeddedPath(path))
            {
                return path;
            }

            return AuraPath.Resolve(path);
        }

        /// <summary>
        /// A theme file is valid on disk or as an embedded resource (live mode / missing install files).
        /// </summary>
        private static bool IsValidThemePath(string path)
        {
            return Files.IsEmbeddedPath(path) || File.Exists(path);
        }

        /// <summary>
        /// Lists the display's modes in the resolution drop down and selects the running one.
        /// GEN3-GAP(display-mode): only VMware SVGA II switches modes; on GOP/virtio-gpu
        /// AvailableModes holds just the current mode, set at boot by limine.conf.
        /// </summary>
        private void LoadResolutions()
        {
            _modes = new List<Mode>();

            if (Kernel.Canvas != null)
            {
                foreach (Mode mode in Kernel.Canvas.AvailableModes)
                {
                    _modes.Add(mode);
                }
            }

            int current = FindMode(_oldScreenWidth, _oldScreenHeight);

            // The running mode is always selectable, even when the display does not list it.
            if (current == -1)
            {
                _modes.Add(new Mode(_oldScreenWidth, _oldScreenHeight, ColorDepth.ColorDepth32));
                current = _modes.Count - 1;
            }

            foreach (Mode mode in _modes)
            {
                _resolution.AddItem(mode.Width + "x" + mode.Height);
            }

            _resolution.SelectedIndex = current;
        }

        /// <summary>
        /// Index of width x height in the resolution list, -1 when not listed.
        /// </summary>
        private int FindMode(int width, int height)
        {
            for (int i = 0; i < _modes.Count; i++)
            {
                if (_modes[i].Width == width && _modes[i].Height == height)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Lists the UI scales (Explorer.Scales) in the scale drop down and selects the running one.
        /// </summary>
        private void LoadScales()
        {
            foreach (int scale in Explorer.Scales)
            {
                _scale.AddItem(scale + "%");
            }

            _scale.SelectedIndex = Array.IndexOf(Explorer.Scales, Kernel.ScreenScale);
        }

        private int GetSelectedScale()
        {
            if (_scale.SelectedIndex < 0)
            {
                return Kernel.ScreenScale;
            }

            return Explorer.Scales[_scale.SelectedIndex];
        }

        private Mode GetSelectedMode()
        {
            if (_resolution.SelectedIndex < 0)
            {
                return new Mode(_oldScreenWidth, _oldScreenHeight, ColorDepth.ColorDepth32);
            }

            return _modes[_resolution.SelectedIndex];
        }

        /// <summary>
        /// Switches the screen to the selected resolution and scale now (Explorer.ChangeResolution).
        /// Returns the error to show, or null; on a refusal the running resolution and scale are
        /// selected again.
        /// </summary>
        private string ApplyResolution()
        {
            Mode resolution = GetSelectedMode();
            int scale = GetSelectedScale();

            if (resolution.Width == _oldScreenWidth && resolution.Height == _oldScreenHeight && scale == Kernel.ScreenScale)
            {
                return null;
            }

            string error;
            bool changed = Explorer.ChangeResolution(resolution.Width, resolution.Height, scale, out error);

            // The mode the display is really in (a refused switch changes nothing).
            _oldScreenWidth = Kernel.Canvas.Width;
            _oldScreenHeight = Kernel.Canvas.Height;

            if (!changed)
            {
                _resolution.SelectedIndex = FindMode(_oldScreenWidth, _oldScreenHeight);
                _scale.SelectedIndex = Array.IndexOf(Explorer.Scales, Kernel.ScreenScale);
            }

            return error;
        }

        public bool UpdateDialog()
        {
            if (!IsValidThemePath(NormalizePath(_themeBmpPath.Text)))
            {
                _dialog.SetState(DialogState.Error);
                _dialog.Message = "Theme .bmp path is not valid.";
                _dialog.MarkDirty();

                return false;
            }
            else if (!IsValidThemePath(NormalizePath(_themeXmlPath.Text)))
            {
                _dialog.SetState(DialogState.Error);
                _dialog.Message = "Theme .xml path is not valid.";
                _dialog.MarkDirty();

                return false;
            }
            else if (!File.Exists(NormalizePath(_wallpaperPath.Text)))
            {
                _dialog.SetState(DialogState.Error);
                _dialog.Message = "Wallpaper path is not valid.";
                _dialog.MarkDirty();

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
