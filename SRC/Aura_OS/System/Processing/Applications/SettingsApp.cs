/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Memory information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Utils;
using Cosmos.System.Graphics;
using System;
using System.ComponentModel;
using System.Drawing;
using Component = Aura_OS.System.Graphics.UI.GUI.Components.Component;

namespace Aura_OS.System.Processing.Applications
{
    public class SettingsApp : Application
    {
        public static string ApplicationName = "Settings";

        TextBox _username;
        TextBox _password;
        TextBox _computerName;
        TextBox _themeBmpPath;
        TextBox _themeXmlPath;

        Label _usernameLabel;
        Label _passwordLabel;
        Label _computerNameLabel;
        Label _themeBmpPathLabel;
        Label _themeXmlPathLabel;

        Checkbox _autoLogin;

        Button _save;

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

            int textBoxXOffset = 6 + (_themeXmlPathLabel.Text.Length * Kernel.font.Width);

            _username = new TextBox(textBoxXOffset, baseY, 200, 23, "");
            _password = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 1, 200, 23, "");
            _computerName = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 2, 200, 23, "");
            _themeBmpPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 3, 200, 23, "");
            _themeXmlPath = new TextBox(textBoxXOffset, baseY + (23 + spacing) * 4, 200, 23, "");

            Settings config = new Settings(@"0:\System\settings.ini");
            string autologin = config.GetValue("autologin");

            _autoLogin = new Checkbox("Auto LogIn: ", Color.Black, labelX, baseY + (23 + spacing) * 5);
            if (autologin == "true")
            {
                _autoLogin.Checked = true;
            }
            else
            {
                _autoLogin.Checked = false;
            }

            _save = new Button("Save Settings", labelX, baseY + (23 + spacing) * 6, 100, 23);
            _save.Click = new Action(() =>
            {
                Kernel.userLogged = _username.Text;
                Kernel.ComputerName = _computerName.Text;
                Kernel.ThemeManager.BmpPath = _themeBmpPath.Text;
                Kernel.ThemeManager.XmlPath = _themeXmlPath.Text;

                if (Kernel.Installed)
                {
                    Settings config = new Settings(@"0:\System\settings.ini");
                    config.EditValue("hostname", Kernel.ComputerName);
                    config.EditValue("themeBmpPath", Kernel.ThemeManager.BmpPath);
                    config.EditValue("themeXmlPath", Kernel.ThemeManager.XmlPath);
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
            });

            AddChild(_username);
            AddChild(_password);
            AddChild(_computerName);
            AddChild(_themeBmpPath);
            AddChild(_themeXmlPath);

            AddChild(_usernameLabel);
            AddChild(_passwordLabel);
            AddChild(_computerNameLabel);
            AddChild(_themeBmpPathLabel);
            AddChild(_themeXmlPathLabel);

            AddChild(_autoLogin);
            AddChild(_save);
        }

        public override void Update()
        {
            base.Update();

            _username.Update();
            _password.Update();
            _computerName.Update();
            _themeBmpPath.Update();
            _themeXmlPath.Update();
            _autoLogin.Update();
            _save.Update();
        }

        public override void Draw()
        {
            base.Draw();

            _username.Text = Kernel.userLogged;
            _password.Text = "";
            _computerName.Text = Kernel.ComputerName;
            _themeBmpPath.Text = Kernel.ThemeManager.BmpPath;
            _themeXmlPath.Text = Kernel.ThemeManager.XmlPath;

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

            _autoLogin.Draw();
            _autoLogin.DrawInParent();

            _save.Draw();
            _save.DrawInParent();
        }
    }
}
