/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Desktop
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;
using Aura_OS.System.Security;
using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class LoginScreen : Component
    {
        private TextBox _username;
        private TextBox _password;
        private Button _button;
        private KeyboardLayoutButton _keyboardButton;
        private Image _wallpaper;
        private string _error;

        public LoginScreen(int x, int y, int width, int height) : base(x, y, width, height)
        {
            _username = new TextBox(3, 3, 200, 23, "");

            AddChild(_username);

            _password = new TextBox(3, 3, 200, 23, "");
            _password.Password = true;

            AddChild(_password);

            _button = new Button("Login", 3, 3, 200, 23);
            _button.Click = new Action(() => {
            _username.Draw(this);
                Login(_username.Text, _password.Text);
            });

            AddChild(_button);

            // Keyboard layout switcher at the bottom-right, so AZERTY users can type their password.
            _keyboardButton = new KeyboardLayoutButton(3, 3, 40, 23);
            AddChild(_keyboardButton);

            LayoutControls();
        }

        /// <summary>
        /// Fits the login screen and its wallpaper to the screen after a resolution change. While hidden
        /// it holds no screen-sized buffer (see Hide), and Show sizes it to the screen then.
        /// </summary>
        public void ResizeToScreen()
        {
            if (Visible)
            {
                SetSize((int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);
                LayoutControls();
            }
        }

        private void LayoutControls()
        {
            _username.X = Width / 2 - _username.Width / 2;
            _username.Y = Height / 2 - _username.Height / 2 + 20;
            _password.X = _username.X;
            _password.Y = _username.Y + 23 + 6;
            _button.X = _password.X;
            _button.Y = _password.Y + 23 + 6;
            _keyboardButton.X = Width - 40 - 8;
            _keyboardButton.Y = Height - 23 - 8;

            // gen2 blanked to black when the wallpaper did not match the screen; gen3 runs at the
            // real framebuffer size (GEN3-GAP(display-mode)), so scale once here, never per frame.
            _wallpaper = null;
            _wallpaper = ImageUtils.ScaleToScreen(Kernel.wallpaper2);
            MarkDirty();
        }

        public override void Update()
        {
            base.Update();

            KeyEvent keyEvent = null;

            while (Input.KeyboardManager.TryGetKey(out keyEvent))
            {
                switch (keyEvent.Key)
                {
                    case Key.Tab:
                        // GEN3-GAP(null-deref): FocusedComponent is null before any focus, and calling
                        // Equals on it is a fatal #PF in gen3.
                        if (ReferenceEquals(Kernel.MouseManager.FocusedComponent, _username))
                        {
                            Kernel.MouseManager.FocusedComponent = _password;
                            _username.SetSelected(false);
                            _password.SetSelected(true);
                        }
                        else if (ReferenceEquals(Kernel.MouseManager.FocusedComponent, _password))
                        {
                            Kernel.MouseManager.FocusedComponent = _username;
                            _password.SetSelected(false);
                            _username.SetSelected(true);
                        }
                        else
                        {
                            Kernel.MouseManager.FocusedComponent = _username;
                        }
                        break;
                    case Key.Enter:
                        _button.Click();
                        break;
                    default:
                        _username.Update(keyEvent);
                        _password.Update(keyEvent);
                        break;

                }
            }

            _username.UpdateNoGetKey();
            _password.UpdateNoGetKey();
            _button.Update();
            _keyboardButton.Update();
        }

        public override void Draw()
        {
            base.Draw();

            DrawImage(_wallpaper, X, Y);

            DrawImage(Kernel.auralogo_white, Width / 2 - (int)Kernel.auralogo_white.Width / 2, _username.Y - (int)Kernel.auralogo_white.Height - 24);

            _username.Draw(this);
            _password.Draw(this);
            _button.Draw(this);
            _keyboardButton.Draw(this);

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
            _keyboardButton.Visible = false;
            Explorer.Taskbar.Visible = true;

            // GEN3-GAP(oom): the hidden login screen gives back its screen buffer and scaled wallpaper
            // (two screen-sized buffers); Show allocates them again.
            SetSize(1, 1);
            _wallpaper = null;
        }

        public void Show()
        {
            foreach (Application app in Explorer.WindowManager.Applications)
            {
                app.Window.Minimize.Click();
            }

            SetSize((int)Kernel.ScreenWidth, (int)Kernel.ScreenHeight);
            LayoutControls();

            Visible = true;
            _username.Text = "";
            _username.Visible = true;
            _password.Text = "";
            _password.Visible = true;
            _button.Visible = true;
            _keyboardButton.Visible = true;
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
                Kernel.UserDirectory = AuraPaths.UsersDir + dirUsername + "/";
                Kernel.CurrentDirectory = Kernel.UserDirectory;

                Explorer.Desktop.MainPanel.CurrentPath = Kernel.CurrentDirectory;
                Explorer.Desktop.MainPanel.RefreshFilesystem();

                Settings config = new Settings(AuraPaths.SettingsIni);
                Kernel.ComputerName = config.GetValue("hostname");

                if (!string.IsNullOrEmpty(Kernel.ComputerName))
                {
                    // DNS and FTP report this name.
                    Cosmos.Kernel.System.Network.DnsConfig.HostName = Kernel.ComputerName;
                }

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