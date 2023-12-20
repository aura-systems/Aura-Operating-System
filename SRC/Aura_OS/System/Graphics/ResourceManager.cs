using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
