/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using System.IO;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Input;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Security;
using Aura_OS.System.Users;
using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class LoginScreen : Component
    {
        private TextBox _username;
        private TextBox _password;
        private Button _button;
        private string _error;

        public LoginScreen(int x, int y, int width, int height) : base(x, y, width, height)
        {
            _username = new TextBox(3, 3, 200, 23, "");
            _username.X = Width / 2 - _username.Width / 2;
            _username.Y = Height / 2 - _username.Height / 2 + 20;

            AddChild(_username);

            _password = new TextBox(3, 3, 200, 23, "");
            _password.X = _username.X;
            _password.Y = _username.Y + 23 + 6;
            _password.Password = true;

            AddChild(_password);

            _button = new Button("Login", _password.X, _password.Y + 23 + 6, 200, 23);
            _button.Click = new Action(() => {
            _username.Draw(this);
                Login(_username.Text, _password.Text);
            });

            AddChild(_button);
        }

        public override void Update()
        {
            base.Update();

            _username.Update();
            _password.Update();
            _button.Update();
        }

        public override void Draw()
        {
            base.Draw();

            DrawImage(Kernel.wallpaper2, X, Y);
            DrawImage(Kernel.auralogo_white, Width / 2 - (int)Kernel.auralogo_white.Width / 2, _username.Y - (int)Kernel.auralogo_white.Height - 24);

            _username.Draw(this);
            _password.Draw(this);
            _button.Draw(this);

            if (_error != null)
            {
                DrawString(_error, Color.White, Width / 2 - (_error.Length * Kernel.font.Width) / 2, _button.Y + 23 + 6);
            }
        }

        public void Hide()
        {
            Kernel.MouseManager.FocusedComponent = _username;

            Visible = false;
            _username.Visible = false;
            _password.Visible = false;
            _button.Visible = false;
            Explorer.Taskbar.Visible = true;
        }

        public void Show()
        {
            foreach (Application app in Explorer.WindowManager.Applications)
            {
                app.Window.Minimize.Click();
            }

            Visible = true;
            _username.Text = "";
            _username.Visible = true;
            _password.Text = "";
            _password.Visible = true;
            _button.Visible = true;
            _error = null;
            Explorer.Taskbar.Visible = false;
            Explorer.StartMenu.Visible = false;
        }

        public bool Login(string username, string password)
        {
            string Sha256psw = Sha256.hash(password);
            string type;

            Users.Users.LoadUsers();

            if (Users.Users.GetUser("user:" + username).Contains(Sha256psw))
            {
                Kernel.LoggedIn = true;

                Hide();

                string dirUsername = username;
                if (dirUsername.Length > 11)
                {
                    dirUsername = dirUsername.Substring(0, 11);
                }

                Kernel.LoggedIn = true;
                Kernel.userLogged = username; 
                Kernel.UserDirectory = @"0:\Users\" + dirUsername + @"\";
                Kernel.CurrentDirectory = Kernel.UserDirectory;

                Explorer.Desktop.MainPanel.CurrentPath = Kernel.CurrentDirectory;
                Explorer.Desktop.MainPanel.RefreshFilesystem();

                Settings config = new Settings(@"0:\System\settings.ini");
                Kernel.ComputerName = config.GetValue("hostname");

                return true;
            }
            else
            {
                _error = "User not found or password incorrect.";
                MarkDirty();
                return false;
            }
        }
    }
}