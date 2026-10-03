/*
* PROJECT:          Aura Operating System Development
* CONTENT:          System information application. The window is Resources/UI/Layouts/SystemInfo.xml.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Network;

namespace Aura_OS.System.Processing.Applications
{
    public class SystemInfoApp : Application
    {
        private Button _checkUpdate;
        private Label _updateStatus;

        public SystemInfoApp(int x = 0, int y = 0) : base(AppLayout.Load("SystemInfo"), x, y)
        {
            _checkUpdate = Find<Button>("checkUpdate");
            _updateStatus = Find<Label>("updateStatus");

            Find<Label>("version").Text = "[version " + Kernel.Version + "-" + Kernel.Revision + "]";

            On("checkUpdate", CheckUpdate);
        }

        /// <summary>
        /// Shows the update check result in place of the button, red when outdated.
        /// </summary>
        private void CheckUpdate()
        {
            bool outdated;
            string status = GetUpdateStatus(out outdated);

            _updateStatus.TextColor = outdated ? Color.Red : Color.Green;
            _updateStatus.Text = status;
            _updateStatus.Visible = true;
            _checkUpdate.Visible = false;

            MarkDirty();
        }

        /// <summary>
        /// Compares the running version with the last release (os.json). Without network, the
        /// running version.
        /// </summary>
        private static string GetUpdateStatus(out bool outdated)
        {
            outdated = false;

            if (!NetworkHelper.IsConfigured)
            {
                return "Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]";
            }

            // Phase 1: still synchronous on the UI thread (gen2 behaviour).
            // GEN3-GAP(backend): the HTTP download can throw (no TLS, os.json endpoint down).
            try
            {
                (string latestVersion, string latestRevision, _) = Network.Version.GetLastVersionInfo();

                if (string.IsNullOrEmpty(Kernel.Version) || string.IsNullOrEmpty(latestVersion) || string.IsNullOrEmpty(Kernel.Revision) || string.IsNullOrEmpty(latestRevision))
                {
                    return "Failed to parse os.json.";
                }

                string latest = latestVersion + "-" + latestRevision;
                int versionComparisonResult = Network.Version.CompareVersions(Kernel.Version, latestVersion);

                if (versionComparisonResult > 0)
                {
                    return "You are on a dev version (last release is " + latest + ").";
                }

                if (versionComparisonResult < 0)
                {
                    outdated = true;
                    return "Your version is outdated (last release is " + latest + ").";
                }

                int revisionComparisonResult = Network.Version.CompareRevisions(Kernel.Revision, latestRevision);

                if (revisionComparisonResult > 0)
                {
                    return "You are on a dev version (last release is " + latest + ").";
                }

                if (revisionComparisonResult < 0)
                {
                    outdated = true;
                    return "Your revision is outdated (last release is " + latest + ").";
                }

                return "You are up to date.";
            }
            catch (Exception ex)
            {
                Logs.DoOSLog("[Error] Update check failed: " + ex.Message);
                return "Failed to check for updates.";
            }
        }
    }
}
