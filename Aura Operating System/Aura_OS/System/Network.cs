/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Init networking
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>; Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Aura_OS.System.Network.Config;
using Aura_OS.System.Network.IPv4;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Network;
using System.Collections.Generic;

namespace Aura_OS.System
{
    class NetworkInit
    {
        public static bool Enable(NetworkDevice device, Address ip, Address subnet, Address gw)
        {            
            if (device != null)
            {
                IPConfig config = new IPConfig(ip, subnet, gw);
                Network.NetworkStack.ConfigIP(device, config);
                Kernel.debugger.Send(config.ToString());
                return true;
            }
            return false;
        }

        static bool IsSavedConf(string device)
        {
            if (Setup.FileSystem() == "true")
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + device + ".conf");
                if ((settings.GetValue("ipaddress") != "0.0.0.0") || (settings.GetValue("subnet") != "0.0.0.0") || (settings.GetValue("gateway") != "0.0.0.0"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
