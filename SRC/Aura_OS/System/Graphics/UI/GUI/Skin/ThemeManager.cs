/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Theme manager.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Utils;
using System.IO;
using System.Text;

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

            // GEN3-GAP(iso9660): gen2 read the default theme from the boot ISO (Files.IsoVolume);
            // gen3 cannot read the ISO, so the default skin ships as an embedded resource
            // (Resources/UI/Themes/Suave.skin.xml) unless a themeXmlPath from settings.ini points
            // to a file on the mounted volume.
            string xmlContent = null;

            if (Kernel.Installed)
            {
                Settings config = new Settings(Kernel.RootVolume + "/System/settings.ini");
                XmlPath = config.GetValue("themeXmlPath");

                if (XmlPath != null && File.Exists(XmlPath))
                {
                    xmlContent = File.ReadAllText(XmlPath);
                }
            }

            if (xmlContent == null)
            {
                XmlPath = "Embedded:Themes/Suave.skin.xml";
                xmlContent = Encoding.UTF8.GetString(Files.GetUiResource("Themes/Suave.skin.xml"));
            }

            _skinParser.loadSkin(xmlContent);
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
