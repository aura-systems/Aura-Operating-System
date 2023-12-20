using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Graphics
{
    public class ResourceManager
    {
        public static Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();

        public static Bitmap GetImage(string key)
        {
            return Icons[key];
        }
    }
}
