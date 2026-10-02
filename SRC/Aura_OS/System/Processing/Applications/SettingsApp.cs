/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Interpreter.Commands;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Processing.Applications
{
    public class SettingsApp : Application
    {
        public static string ApplicationName = "Settings";

        private TextBox _username;
        private TextBox _password;
        private TextBox _computerName;
        private TextBox _themeBmpPath;
        private TextBox _themeXmlPath;
        private DropDown _resolution;
        private TextBox _wallpaperPath;

        private Label _usernameLabel;
        private Label _passwordLabel;
        private Label _computerNameLabel;
        private Label _themeBmpPathLabel;
        private Label _themeXmlPathLabel;
        private Label _windowsAlphaLabel;
        private Label _taskbarAlphaLabel;
        private Label _resLabel;
        private Label _wallpaperLabel;

        private Checkbox _autoLogin;
        private Checkbox _guiDebug;

        private Slider _windowsAlpha;
        private Slider _taskbarAlpha;

        private Button _save;

        private Dialog _dialog;

        private List<Mode> _modes;
        private int _oldScreenWidth;
        private int _oldScreenHeight;
        private string _oldWallpaperPath;

        public SettingsApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            Window.Icon = Kernel.ResourceManager.GetIcon("16-settings.bmp");

            int spacing = 10;
            int baseY = 3 + Window.TopBar.Height + 6;
            int labelX = 6;

            _usernameLabel = new Label("Username: ", Color.Black, labelX, baseY);
            _passwordLabel = new Label("Password: ", Color.Black, labelX, baseY + (23 + spacing) * 1);
            _computerNameLabel = new Label("Computer Name: ", Color.Black, labelX, baseY + (23 + spacing) * 2);
            _themeBmpPathLabel = new Label("Theme BMP Path: ", Color.Black, labelX, baseY + (23 + spacing) * 3);
            _themeXmlPathLabel = new Label("Theme XML Path: ", Color.Black, labelX, baseY + (23 + spacing) * 4);
            _windowsAlphaLabel = new Label("Windows Alpha: ", Color.Black, labelX, baseY + (23 + spacing) * 5);
            _taskbarAlphaLabel = new Label("Taskbar Alpha: ", Color.Black, labelX, baseY + (23 + spacing) * 6);
            _resLabel = new Label("Resolution: ", Color.Black, labelX, baseY + (23 + spacing) * 7);
            _wallpaperLabel = new Label("Wallpaper Path: ", Color.Black, labelX, baseY + (23 + spacing) * 8);

            int textBoxXOffset = 6 + (_themeXmlPathLabel.Text.Length * Kernel.font.Width);

            _username = new TextBox(textBoxXOffset, baseY, 200, 23, "");
            _password = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 1, 200, 23, "");
            _computerName = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 2, 200, 23, "");
            _themeBmpPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 3, 200, 23, "");
            _themeXmlPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 4, 200, 23, "");
            _windowsAlpha = new Slider(textBoxXOffset, baseY + (23 + spacing) * 5, 200, 23);
            _taskbarAlpha = new Slider(textBoxXOffset, baseY + (23 + spacing) * 6, 200, 23);
            _resolution = new DropDown(textBoxXOffset, baseY + (23 + spacing) * 7, 200, 23);
            _wallpaperPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 8, 200, 23, "");

            _guiDebug = new Checkbox("GUI Debug: ", Color.Black, labelX, baseY + (23 + spacing) * 9);

            if (Kernel.Installed)
            {
                Settings config = new Settings(AuraPaths.SettingsIni);
                string autologin = config.GetValue("autologin");
                byte windowsTransparency;
                if (!byte.TryParse(config.GetValue("windowsTransparency"), out windowsTransparency))
                {
                    windowsTransparency = 0xFF;
                }
                _windowsAlpha.Value = windowsTransparency;
                byte taskbarTransparency;
                if (!byte.TryParse(config.GetValue("taskbarTransparency"), out taskbarTransparency))
                {
                    taskbarTransparency = 0xFF;
                }
                _taskbarAlpha.Value = taskbarTransparency;

                if (autologin == "true")
                {
                    _autoLogin = new Checkbox("Auto LogIn: ", Color.Black, labelX, baseY + (23 + spacing) * 10, true);
                }
                else
                {
                    _autoLogin = new Checkbox("Auto LogIn: ", Color.Black, labelX, baseY + (23 + spacing) * 10);
                }

                _save = new Button("Save Settings", Width / 2 - 100 / 2, baseY + (23 + spacing) * 11, 100, 23);
            }
            else
            {
                _windowsAlpha.Value = 0xFF;
                _taskbarAlpha.Value = 0xFF;
                _save = new Button("Save Settings", Width / 2 - 100 / 2, baseY + (23 + spacing) * 10, 100, 23);
            }
            
            _save.Click = new Action(() =>
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
                        config.EditValue("screenWidth", Kernel.ScreenWidth.ToString());
                        config.EditValue("screenHeight", Kernel.ScreenHeight.ToString());
                        config.EditValue("wallpaperPath", wallpaperPath);
                        if (_autoLogin.Checked)
                        {
                            config.EditValue("autologin", "true");
                        }
                        else
                        {
                            config.EditValue("autologin", "false");
                        }
                        config.Push();
                    }
                }

                _dialog.Visible = true;

                MarkDirty();
            });

            _dialog = new("Save", "Settings updated.", (int)Width / 2 - 302 / 2, Height / 2 - 119 / 2);
            _dialog.Visible = false;
            _dialog.AddButton("OK", new Action(() =>
            {
                _dialog.Visible = false;

                foreach (var child in Window.Children)
                {
                    child.MarkDirty();
                }
                MarkDirty();
            }));

            AddChild(_username);
            AddChild(_password);
            AddChild(_computerName);
            AddChild(_themeBmpPath);
            AddChild(_themeXmlPath);
            AddChild(_windowsAlpha);
            AddChild(_taskbarAlpha);
            AddChild(_resolution);
            AddChild(_wallpaperPath);

            AddChild(_usernameLabel);
            AddChild(_passwordLabel);
            AddChild(_computerNameLabel);
            AddChild(_themeBmpPathLabel);
            AddChild(_themeXmlPathLabel);
            AddChild(_windowsAlphaLabel);
            AddChild(_taskbarAlphaLabel);
            AddChild(_resLabel);
            AddChild(_wallpaperLabel);

            AddChild(_dialog);

            if (Kernel.Installed)
            {
                AddChild(_autoLogin);
            }

            AddChild(_guiDebug);

            AddChild(_save);

            _oldScreenWidth = (int)Kernel.ScreenWidth;
            _oldScreenHeight = (int)Kernel.ScreenHeight;

            // A null TextBox.Text would be a null deref (kernel halt) on the first draw
            _username.Text = Kernel.userLogged ?? "";
            _password.Text = "";
            _computerName.Text = Kernel.ComputerName ?? "";
            _themeBmpPath.Text = Kernel.ThemeManager.BmpPath ?? "";
            _themeXmlPath.Text = Kernel.ThemeManager.XmlPath ?? "";
            LoadResolutions();
            _wallpaperPath.Text = Explorer.Desktop.GetWallpaperPath() ?? "";
            _oldWallpaperPath = _wallpaperPath.Text;
        }

        public override void Update()
        {
            base.Update();

            if (_dialog.Visible)
            {
                _dialog.Update();
            }
            else
            {
                _username.Update();
                _password.Update();
                _computerName.Update();
                _themeBmpPath.Update();
                _themeXmlPath.Update();
                _windowsAlpha.Update();
                _taskbarAlpha.Update();
                _resolution.Update();
                _wallpaperPath.Update();

                if (Kernel.Installed)
                {
                    _autoLogin.Update();
                }

                _guiDebug.Update();

                _save.Update();
            }
        }

        public override void Draw()
        {
            base.Draw();

            _username.Draw();
            _username.DrawInParent();
            _password.Draw();
            _password.DrawInParent();
            _computerName.Draw();
            _computerName.DrawInParent();
            _themeBmpPath.Draw();
            _themeBmpPath.DrawInParent();
            _themeXmlPath.Draw();
            _themeXmlPath.DrawInParent();
            _windowsAlpha.Draw();
            _windowsAlpha.DrawInParent();
            _taskbarAlpha.Draw();
            _taskbarAlpha.DrawInParent();
            _resolution.Draw();
            _resolution.DrawInParent();
            _wallpaperPath.Draw();
            _wallpaperPath.DrawInParent();
            

            _usernameLabel.Draw();
            _usernameLabel.DrawInParent();
            _passwordLabel.Draw();
            _passwordLabel.DrawInParent();
            _computerNameLabel.Draw();
            _computerNameLabel.DrawInParent();
            _themeBmpPathLabel.Draw();
            _themeBmpPathLabel.DrawInParent();
            _themeXmlPathLabel.Draw();
            _themeXmlPathLabel.DrawInParent();
            _windowsAlphaLabel.Draw();
            _windowsAlphaLabel.DrawInParent();
            _taskbarAlphaLabel.Draw();
            _taskbarAlphaLabel.DrawInParent();
            _resLabel.Draw();
            _resLabel.DrawInParent();
            _wallpaperLabel.Draw();
            _wallpaperLabel.DrawInParent();

            if (Kernel.Installed)
            {
                _autoLogin.Draw();
                _autoLogin.DrawInParent();
            }

            _guiDebug.Draw();
            _guiDebug.DrawInParent();

            _save.Draw();
            _save.DrawInParent();

            if (_dialog.Visible)
            {
                _dialog.Draw();
                _dialog.DrawInParent();
            }
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

        private Mode GetSelectedMode()
        {
            if (_resolution.SelectedIndex < 0)
            {
                return new Mode(_oldScreenWidth, _oldScreenHeight, ColorDepth.ColorDepth32);
            }

            return _modes[_resolution.SelectedIndex];
        }

        /// <summary>
        /// Switches the screen to the selected resolution now (Explorer.ChangeResolution). Returns the
        /// error to show, or null; on a refusal the running resolution is selected again.
        /// </summary>
        private string ApplyResolution()
        {
            Mode resolution = GetSelectedMode();

            if (resolution.Width == _oldScreenWidth && resolution.Height == _oldScreenHeight)
            {
                return null;
            }

            string error;
            bool changed = Explorer.ChangeResolution(resolution.Width, resolution.Height, out error);

            // The mode the screen is really in (a refused switch changes nothing).
            _oldScreenWidth = (int)Kernel.ScreenWidth;
            _oldScreenHeight = (int)Kernel.ScreenHeight;

            if (!changed)
            {
                _resolution.SelectedIndex = FindMode(_oldScreenWidth, _oldScreenHeight);
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
