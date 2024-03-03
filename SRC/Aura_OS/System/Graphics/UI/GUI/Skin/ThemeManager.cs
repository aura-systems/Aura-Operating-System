/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Theme manager.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

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

        /// <summary>
        /// Loads the default theme and initializes the theme management system.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting theme manager...");

            _skinParser = new SkinParsing();

            string xmlPath = "";

            if (File.Exists(@"0:\System\settings.ini"))
            {
                Settings config = new Settings(@"0:\System\settings.ini");
                xmlPath = config.GetValue("themeXmlPath");

                if (!File.Exists(xmlPath))
                {
                    xmlPath = Files.IsoVolume + "UI\\Themes\\Suave.skin.xml";
                }
            }
            else
            {
                xmlPath = Files.IsoVolume + "UI\\Themes\\Suave.skin.xml";
            }

            _skinParser.loadSkin(File.ReadAllText(xmlPath));
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
