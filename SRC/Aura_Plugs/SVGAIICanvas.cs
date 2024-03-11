using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.Video.SVGAII;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Assembler.x86;

namespace Aura_Plugs
{
    [Plug(Target = typeof(SVGAIICanvas))]
    internal class SVGAIICanvasImpl
    {
        public static void DrawImage(SVGAIICanvas aThis, Image image, int x, int y, [FieldAccess(Name = "Cosmos.HAL.Drivers.Video.SVGAII.VMWareSVGAII Cosmos.System.Graphics.SVGAIICanvas.driver")] ref VMWareSVGAII driver)
        {
            var width = (int)image.Width;
            var height = (int)image.Height;
            var data = image.RawData;

            for (int i = 0; i < height; i++)
            {
                driver.videoMemory.Copy(aThis.GetPointOffset(x, y + i), data, i * width, width);
            }
        }

        public static void Display(SVGAIICanvas aThis, [FieldAccess(Name = "Cosmos.HAL.Drivers.Video.SVGAII.VMWareSVGAII Cosmos.System.Graphics.SVGAIICanvas.driver")] ref VMWareSVGAII driver)
        {
            driver.Update(0, 0, aThis.Mode.Width, aThis.Mode.Height);
        }
    }
}
