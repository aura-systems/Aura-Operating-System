/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Video card detection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System
{
    public class Video
    {
        public static string GetVideo()
        {
            // TO DO: Scroll console in SVGAII
            if (Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.VMWare, Cosmos.HAL.DeviceID.SVGAIIAdapter) != null)
            {
                return "SVGAII";
            }
            else if (Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.Bochs, Cosmos.HAL.DeviceID.BGA) != null || Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.VirtualBox, Cosmos.HAL.DeviceID.VBVGA) != null)
            {
                return "VBE";
            }
            else
            {
                return "VGATextmode";
            }
        }
    }
}
