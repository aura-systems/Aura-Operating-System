/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Registry of the gen3 kernel keyboard layouts
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Input.Layouts;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Utils;
using CosmosKeyboard = Cosmos.Kernel.System.Input.KeyboardManager;

namespace Aura_OS.System.Input
{
    /// <summary>
    /// Every keyboard layout Aura can use, keyed by the short code shown in the
    /// taskbar. Single source of truth for the taskbar/login layout menu, the
    /// setkeyboardmap command and the 'keyboardLayout' entry of settings.ini.
    /// </summary>
    public static class KeyboardLayouts
    {
        /// <summary>Name of the settings.ini entry holding the layout code.</summary>
        private const string SettingName = "keyboardLayout";

        /// <summary>Code of the active layout (the kernel boots on the US map).</summary>
        public static string CurrentCode { get; private set; } = "US";

        /// <summary>All selectable layout codes, in menu order.</summary>
        public static readonly string[] Codes = { "US", "GB", "FR", "DE", "ES", "TR", "DV" };

        /// <summary>Accepted aliases (gen2 kept "azerty"/"qwerty"); see <see cref="ResolveCode"/>.</summary>
        public static readonly string[] Aliases = { "azerty", "qwerty", "qwertz", "dvorak" };

        /// <summary>
        /// Human-readable name for a layout code (menu label next to the code).
        /// </summary>
        public static string GetDisplayName(string code)
        {
            // Accept any case and the aliases too ("fr", "azerty"); unknown input is echoed back.
            string resolved = ResolveCode(code);

            switch (resolved)
            {
                case "US": return "English (US, QWERTY)";
                case "GB": return "English (UK, QWERTY)";
                case "FR": return "French (AZERTY)";
                case "DE": return "German (QWERTZ)";
                case "ES": return "Spanish (QWERTY)";
                case "TR": return "Turkish (Q)";
                case "DV": return "English (US, Dvorak)";
                default: return code;
            }
        }

        /// <summary>
        /// Maps a code or an alias (case-insensitive) to its layout code.
        /// </summary>
        /// <returns>The layout code ("US", "FR", ...), or null when nothing matches.</returns>
        public static string ResolveCode(string codeOrAlias)
        {
            if (codeOrAlias == null)
            {
                return null;
            }

            string code = codeOrAlias.Trim().ToUpperInvariant();

            switch (code)
            {
                case "AZERTY": return "FR";
                case "QWERTY": return "US";
                case "QWERTZ": return "DE";
                case "DVORAK": return "DV";
            }

            for (int i = 0; i < Codes.Length; i++)
            {
                if (Codes[i] == code)
                {
                    return code;
                }
            }

            return null;
        }

        private static KeyboardLayout Create(string code)
        {
            switch (code)
            {
                case "US": return new USStandardLayout();
                case "GB": return new GBStandardLayout();
                case "FR": return new AuraFRStandardLayout(); // AZERTY with an AltGr level (GEN3-GAP(keyboard-altgr))
                case "DE": return new DEStandardLayout();
                case "ES": return new ESStandardLayout();
                case "TR": return new TRStandardLayout();
                case "DV": return new USDvorakLayout();
                default: return null;
            }
        }

        /// <summary>
        /// Activates the layout for the given code or alias (case-insensitive).
        /// </summary>
        /// <returns>False when the keyboard is compiled out or the code matches no known layout.</returns>
        public static bool Set(string codeOrAlias)
        {
            // SetLayout throws when the keyboard support is compiled out.
            if (!KernelFeatures.Keyboard)
            {
                return false;
            }

            string code = ResolveCode(codeOrAlias);
            if (code == null)
            {
                return false;
            }

            KeyboardLayout layout = Create(code);
            if (layout == null)
            {
                return false;
            }

            CosmosKeyboard.SetLayout(layout);
            CurrentCode = code;
            return true;
        }

        /// <summary>
        /// Saves <see cref="CurrentCode"/> as 'keyboardLayout' in settings.ini (add or edit).
        /// Does nothing in live mode (Aura not installed).
        /// </summary>
        public static void Persist()
        {
            if (!Kernel.Installed)
            {
                return;
            }

            string path = AuraPaths.SettingsIni;

            // Never create settings.ini from here (Setup owns it); gen2 Settings also dereferenced
            // null content for a missing file (C6).
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                Settings config = new Settings(path);

                if (config.GetValue(SettingName) == "null")
                {
                    config.PutValue(SettingName, CurrentCode);
                }
                else
                {
                    config.EditValue(SettingName, CurrentCode);
                }

                config.PushValues();
            }
            catch (Exception ex)
            {
                Logs.DoOSLog("[Error] Cannot save the keyboard layout: " + ex.Message);
            }
        }
    }
}
