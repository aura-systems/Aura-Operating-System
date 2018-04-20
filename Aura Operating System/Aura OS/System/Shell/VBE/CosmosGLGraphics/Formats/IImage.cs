using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE.CosmosGLGraphics.Formats
{
    public interface IImage
    {
        Image Read(byte[] bytes);
    }
}
