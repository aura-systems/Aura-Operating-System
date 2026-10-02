/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Theme manager.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Utils;
using System.IO;

namespace Aura_OS.System.Graphics.UI.GUI.Skin
{
    /// <summary>
    /// Manages themes for the user interface, loading and applying skins.
    /// </summary>
    public class ThemeManager : IManager
    {
        /// <summary>
        /// The skin parser used to load and interpret theme files.
        /// </summary>
        private SkinParsing _skinParser;

        public string XmlPath;
        public string BmpPath;

        /// <summary>
        /// Loads the default theme and initializes the theme management system.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting theme manager...");

            _skinParser = new SkinParsing();

            XmlPath = null;

            if (Kernel.Installed)
            {
                Settings config = new Settings(AuraPaths.SettingsIni);
                string themeXmlPath = AuraPath.FromLegacy(config.GetValue("themeXmlPath"));

                if (!string.IsNullOrEmpty(themeXmlPath) && File.Exists(themeXmlPath))
                {
                    XmlPath = themeXmlPath;
                }
            }

            if (XmlPath == null)
            {
                // GEN3-GAP(iso-files): no ISO volume; the default skin is an embedded resource.
                XmlPath = Files.EmbeddedScheme + "UI/Themes/Suave.skin.xml";
            }

            // Disk or embedded; the UTF-8 BOM (Suave.skin.xml has one) is stripped on both paths.
            _skinParser.loadSkin(Files.ReadAllText(XmlPath));
        }

        /// <summary>
        /// Retrieves the specified frame by name from the currently loaded theme.
        /// </summary>
        /// <param name="name">The name of the frame to retrieve.</param>
        /// <returns>The requested frame if found; otherwise, null.</returns>
        public Frame GetFrame(string name)
        {
            return _skinParser.GetFrame(name);
        }

        /// <summary>
        /// Gets the name of the currently loaded theme.
        /// </summary>
        /// <returns>The name of the current theme.</returns>
        public string GetThemeName()
        {
            return _skinParser.GetSkinName();
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Theme Manager";
        }
    }
}
