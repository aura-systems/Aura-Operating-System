/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Resource Manager. Used to store icons
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System.Collections.Generic;

namespace Aura_OS.System.Graphics
{
    public class ResourceManager
    {
        public static Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();

        public static Bitmap GetImage(string key)
        {
            try
            {
                Bitmap bitmap = Icons[key];
                return bitmap;
            }
            catch
            {
                Crash.StopKernel(key + " not found", "Error while getting resource file.", "0x00000000", "0");
                throw;
            }
        }
    }
}
