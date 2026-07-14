/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Registry of the gen3 kernel keyboard layouts
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;

namespace Aura_OS.System.Input
{
    /// <summary>
    /// Every keyboard scan map shipped by the gen3 kernel, keyed by the short
    /// language code shown in the taskbar. Single source of truth for the
    /// taskbar layout menu and the setkeyboardmap command.
    /// </summary>
    public static class KeyboardLayouts
    {
        /// <summary>Code of the active layout (gen3 boots on the US map).</summary>
        public static string CurrentCode { get; private set; } = "US";

        /// <summary>All selectable layout codes, in menu order.</summary>
        public static readonly string[] Codes = { "US", "GB", "FR", "DE", "ES", "TR", "DV" };

        /// <summary>
        /// Human-readable name for a layout code (menu label next to the code).
        /// </summary>
        public static string GetDisplayName(string code)
        {
            switch (code)
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

        private static ScanMapBase Create(string code)
        {
            switch (code)
            {
                case "US": return new USStandardLayout();
                case "GB": return new GBStandardLayout();
                case "FR": return new FRStandardLayout();
                case "DE": return new DEStandardLayout();
                case "ES": return new ESStandardLayout();
                case "TR": return new TRStandardLayout();
                case "DV": return new USDvorakLayout();
                default: return null;
            }
        }

        /// <summary>
        /// Activates the layout for the given code (case-insensitive).
        /// </summary>
        /// <returns>False when the code matches no known layout.</returns>
        public static bool Set(string code)
        {
            code = code.ToUpper();

            ScanMapBase scanMap = Create(code);
            if (scanMap == null)
            {
                return false;
            }

            Cosmos.Kernel.System.Keyboard.KeyboardManager.SetKeyLayout(scanMap);
            CurrentCode = code;
            return true;
        }
    }
}
