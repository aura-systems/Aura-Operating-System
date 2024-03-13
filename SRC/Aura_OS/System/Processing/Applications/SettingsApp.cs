/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Interpreter.Commands;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Utils;
using Cosmos.System.Graphics;

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
        private TextBox _resX;
        private TextBox _resY;

        private Label _usernameLabel;
        private Label _passwordLabel;
        private Label _computerNameLabel;
        private Label _themeBmpPathLabel;
        private Label _themeXmlPathLabel;
        private Label _windowsAlphaLabel;
        private Label _taskbarAlphaLabel;
        private Label _resLabel;

        private Checkbox _autoLogin;
        private Checkbox _guiDebug;

        private Slider _windowsAlpha;
        private Slider _taskbarAlpha;

        private Button _save;

        private Dialog _dialog;

        private uint _oldScreenWidth;
        private uint _oldScreenHeight;
        private bool _waitingReboot = false;

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

            int textBoxXOffset = 6 + (_themeXmlPathLabel.Text.Length * Kernel.font.Width);

            _username = new TextBox(textBoxXOffset, baseY, 200, 23, "");
            _password = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 1, 200, 23, "");
            _computerName = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 2, 200, 23, "");
            _themeBmpPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 3, 200, 23, "");
            _themeXmlPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 4, 200, 23, "");
            _windowsAlpha = new Slider(textBoxXOffset, baseY + (23 + spacing) * 5, 200, 23);
            _taskbarAlpha = new Slider(textBoxXOffset, baseY + (23 + spacing) * 6, 200, 23);
            _resX = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 7, 100, 23);
            _resY = new TextBox(textBoxXOffset + 103, baseY + (23 + spacing) * 7, 100, 23);

            _guiDebug = new Checkbox("GUI Debug: ", Color.Black, labelX, baseY + (23 + spacing) * 8);

            if (Kernel.Installed)
            {
                Settings config = new Settings(@"0:\System\settings.ini");
                string autologin = config.GetValue("autologin");
                byte windowsTransparency = byte.Parse(config.GetValue("windowsTransparency"));
                _windowsAlpha.Value = windowsTransparency;
                byte taskbarTransparency = byte.Parse(config.GetValue("taskbarTransparency"));
                _taskbarAlpha.Value = taskbarTransparency;

                if (autologin == "true")
                {
                    _autoLogin = new Checkbox("Auto LogIn: ", Color.Black, labelX, baseY + (23 + spacing) * 9, true);
                }
                else
                {
                    _autoLogin = new Checkbox("Auto LogIn: ", Color.Black, labelX, baseY + (23 + spacing) * 9);
                }

                _save = new Button("Save Settings", Width / 2 - 100 / 2, baseY + (23 + spacing) * 9, 100, 23);
            }
            else
            {
                _windowsAlpha.Value = 0xFF;
                _taskbarAlpha.Value = 0xFF;
                _save = new Button("Save Settings", Width / 2 - 100 / 2, baseY + (23 + spacing) * 9, 100, 23);
            }
            
            _save.Click = new Action(() =>
            {
                Kernel.userLogged = _username.Text;
                Kernel.ComputerName = _computerName.Text;
                Kernel.ThemeManager.BmpPath = _themeBmpPath.Text;
                Kernel.ThemeManager.XmlPath = _themeXmlPath.Text;
                Explorer.WindowManager.WindowsTransparency = (byte)_windowsAlpha.Value;
                Explorer.WindowManager.TaskbarTransparency = (byte)_taskbarAlpha.Value;
                Kernel.GuiDebug = _guiDebug.Checked;

                if (Kernel.Installed)
                {
                    Settings config = new Settings(@"0:\System\settings.ini");
                    config.EditValue("hostname", Kernel.ComputerName);
                    config.EditValue("themeBmpPath", Kernel.ThemeManager.BmpPath);
                    config.EditValue("themeXmlPath", Kernel.ThemeManager.XmlPath);
                    config.EditValue("windowsTransparency", Explorer.WindowManager.WindowsTransparency.ToString());
                    config.EditValue("taskbarTransparency", Explorer.WindowManager.TaskbarTransparency.ToString());
                    config.EditValue("screenWidth", _resX.Text);
                    config.EditValue("screenHeight", _resY.Text);
                    if (_autoLogin.Checked)
                    {
                        config.EditValue("autologin", "true");
                    }
                    else
                    {
                        config.EditValue("autologin", "false");
                    }
                    config.Push();

                    UpdateDialog();
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
            AddChild(_resX);
            AddChild(_resY);

            AddChild(_usernameLabel);
            AddChild(_passwordLabel);
            AddChild(_computerNameLabel);
            AddChild(_themeBmpPathLabel);
            AddChild(_themeXmlPathLabel);
            AddChild(_windowsAlphaLabel);
            AddChild(_taskbarAlphaLabel);
            AddChild(_resLabel);

            AddChild(_dialog);

            if (Kernel.Installed)
            {
                AddChild(_autoLogin);
            }

            AddChild(_guiDebug);

            AddChild(_save);

            _oldScreenWidth = Kernel.ScreenWidth;
            _oldScreenHeight = Kernel.ScreenHeight;

            _username.Text = Kernel.userLogged;
            _password.Text = "";
            _computerName.Text = Kernel.ComputerName;
            _themeBmpPath.Text = Kernel.ThemeManager.BmpPath;
            _themeXmlPath.Text = Kernel.ThemeManager.XmlPath;
            _resX.Text = Kernel.ScreenWidth.ToString();
            _resY.Text = Kernel.ScreenHeight.ToString();
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
                _resX.Update();
                _resY.Update();

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
            _resLabel.Draw();
            _resLabel.DrawInParent();

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
            _resX.Draw();
            _resX.DrawInParent();
            _resY.Draw();
            _resY.DrawInParent();

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

        public void UpdateDialog()
        {
            uint width = uint.Parse(_resX.Text);
            uint height = uint.Parse(_resY.Text);

            if (_waitingReboot == false && (_oldScreenWidth != width || _oldScreenHeight != height))
            {
                bool modeExists = false;

                foreach (var mode in Kernel.Canvas.AvailableModes)
                {
                    if (mode.Width == width && mode.Height == height)
                    {
                        modeExists = true;
                    }
                }

                if (modeExists)
                {
                    _dialog.SetState(DialogState.Information);
                    _dialog.Message = "Settings updated. Reboot needed to change resolution.";
                    _dialog.AddButton("Reboot", new Action(() =>
                    {
                        Cosmos.System.Power.Reboot();
                    }));
                    _dialog.MarkDirty();
                    _waitingReboot = true;
                }
                else
                {
                    _dialog.SetState(DialogState.Error);
                    _dialog.Message = width + "x" + height + "@32 is not a valid resolution. Type lsres to list available resolutions.";
                    _dialog.MarkDirty();
                    _waitingReboot = false;
                }
            }
        }
    }
}
