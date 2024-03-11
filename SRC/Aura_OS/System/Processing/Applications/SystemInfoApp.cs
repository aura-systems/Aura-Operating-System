/*
* PROJECT:          Aura Operating System Development
* CONTENT:          System information application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Cosmos.System.Network;

namespace Aura_OS.System.Processing.Applications
{
    public class SystemInfoApp : Application
    {
        public static string ApplicationName = "SystemInfo";

        public Color _greenPen = Color.Green;
        public Color _redPen = Color.Red;

        const string _title = "Aura Operating System";
        const string _credit = "Created by Alexy DA CRUZ and Valentin CHARBONNIER.";
        const string _website = "Project: github.com/aura-systems/Aura-Operating-System";
        const string _website2 = "Kernel: github.com/CosmosOS/Cosmos";
        private string _version = "";

        private Button _button;
        private bool _isOutdated = false;

        public SystemInfoApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            string text = "Check update";
            _button = new Button(text, (Width / 2) - ((text.Length * Kernel.font.Width) / 2), 5 * Kernel.font.Height + 2, text.Length * Kernel.font.Width + 6, Kernel.font.Height + 6);
            _button.Click = new Action(() =>
            {
                if (NetworkStack.ConfigEmpty())
                {
                    _version = "Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]";
                }
                else
                {
                    (string latestVersion, string latestRevision, string latestReleaseUrl) = Network.Version.GetLastVersionInfo();

                    int versionComparisonResult = Network.Version.CompareVersions(Kernel.Version, latestVersion);

                    if (string.IsNullOrEmpty(Kernel.Version) || string.IsNullOrEmpty(latestVersion) || string.IsNullOrEmpty(Kernel.Revision) || string.IsNullOrEmpty(latestRevision))
                    {
                        _version = "Failed to parse os.json.";
                    }
                    else
                    {
                        if (versionComparisonResult > 0)
                        {
                            _version = "You are on a dev version (last release is " + latestVersion + "-" + latestRevision + ").";
                        }
                        else if (versionComparisonResult < 0)
                        {
                            _version = "Your version is outdated (last release is " + latestVersion + "-" + latestRevision + ").";
                            _isOutdated = true;
                        }
                        else
                        {
                            int revisionComparisonResult = Network.Version.CompareRevisions(Kernel.Revision, latestRevision);
                            if (revisionComparisonResult > 0)
                            {
                                _version = "You are on a dev version (last release is " + latestVersion + "-" + latestRevision + ").";
                            }
                            else if (revisionComparisonResult < 0)
                            {
                                 _version = "Your revision is outdated (last release is " + latestVersion + "-" + latestRevision + ").";
                                _isOutdated = true;
                            }
                            else
                            {
                                _version = "You are up to date.";
                            }
                        }
                    }
                }
                _button.Visible = false;
                MarkDirty();
            });
            AddChild(_button);
        }

        public override void Update()
        {
            base.Update();

            _button.Update();
        }

        public override void Draw()
        {
            base.Draw();

            var version = "[version " + Kernel.Version + "-" + Kernel.Revision + "]";

            DrawString(_title, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, Kernel.BlackColor, 0 + Width / 2 - _title.Length * Cosmos.System.Graphics.Fonts.PCScreenFont.Default.Width / 2, 0 + 1 * Kernel.font.Height);
            DrawString(version, Kernel.font, Kernel.BlackColor, 0 + Width / 2 - version.Length * Kernel.font.Width / 2, 0 + 2 * Kernel.font.Height);

            DrawString(_credit, Kernel.font, Kernel.BlackColor, 0 + Width / 2 - _credit.Length * Kernel.font.Width / 2, 0 + 5 * Kernel.font.Height + 4);
            DrawString(_website, Kernel.font, _greenPen, 0 + Width / 2 - _website.Length * Kernel.font.Width / 2, 0 + 6 * Kernel.font.Height + 4);

            DrawImage(Kernel.AuraLogo, 0 + Width / 2 - (int)Kernel.AuraLogo.Width / 2, 0 + 8 * Kernel.font.Height);

            DrawString(_website2, Kernel.font, _greenPen, 0 + Width / 2 - _website2.Length * Kernel.font.Width / 2, 0 + 18 * Kernel.font.Height);
            DrawImage(Kernel.CosmosLogo, 0 + Width / 2 - (int)Kernel.CosmosLogo.Width / 2, 0 + 20 * Kernel.font.Height);

            if (!string.IsNullOrEmpty(_version))
            {
                DrawString(_version, Kernel.font, _isOutdated ? _redPen : _greenPen, 0 + Width / 2 - _version.Length * Kernel.font.Width / 2, 3 * Kernel.font.Height);
            }
            else
            {
                _button.Draw();
                _button.DrawInParent();
            }
        }
    }
}
