/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Resource Manager. Used to store icons
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System.Collections.Generic;

namespace Aura_OS.System.Graphics
{
    /// <summary>
    /// Manages resources such as icons for AuraOS. 
    /// </summary>
    public class ResourceManager : IManager
    {
        /// <summary>
        /// A dictionary to hold icons with their names as keys.
        /// </summary>
        public Dictionary<string, Bitmap> _icons;

        /// <summary>
        /// Initializes the resource manager and prepares it to store icons.
        /// </summary>
        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting resource manager...");

            _icons = new Dictionary<string, Bitmap>();

            CustomConsole.WriteLineInfo("Loading icons...");
            Files.LoadImages();
        }

        /// <summary>
        /// Adds an icon to the resource manager.
        /// </summary>
        /// <param name="name">The name to associate with the icon.</param>
        /// <param name="icon">The bitmap icon to store.</param>
        public void AddIcon(string name, Bitmap icon)
        {
            _icons.Add(name, icon);
        }

        /// <summary>
        /// Retrieves an icon from the resource manager by its name.
        /// </summary>
        /// <param name="key">The name of the icon to retrieve.</param>
        /// <returns>The bitmap icon associated with the given name.</returns>
        /// <exception cref="System.Exception">Thrown when the icon is not found.</exception>
        public Bitmap GetIcon(string key)
        {
            try
            {
                Bitmap bitmap = _icons[key];
                return bitmap;
            }
            catch
            {
                Crash.StopKernel(key + " not found", "Error while getting resource file.", "0x00000000", "0");
                throw;
            }
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Resource Manager";
        }
    }
}
