/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Init networking
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Network.IPV4;
using System.Collections.Generic;

namespace Aura_OS.System
{
    class NetworkInit
    {

        public static bool Enable(NetworkDevice device, Address ip, Address subnet, Address gw)
        {            
            if (device != null)
            {
                Kernel.LocalNetworkConfig = new Config(ip, subnet, gw);
                Network.NetworkStack.ConfigIP(device, Kernel.LocalNetworkConfig);
                Kernel.debugger.Send(Kernel.LocalNetworkConfig.ToString());
                return true;
            }
            return false;
        }

        public static void Init()
        {
            CustomConsole.WriteLineInfo("Searching for Network cards...");

            #region AMDPCNETII

            CustomConsole.WriteLineInfo("Searching for AMDPCNETII...");
            Cosmos.HAL.PCIDevice AMDPCNETII = Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.AMD, Cosmos.HAL.DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                CustomConsole.WriteLineInfo("NIC IRQ: " + AMDPCNETII.InterruptLine);

                new AMDPCNetII(AMDPCNETII);

                CustomConsole.WriteLineInfo("NIC MAC Address: " + NetworkDevice.Devices[0].MACAddress.ToString());

                Network.NetworkStack.Init();
                NetworkDevice.Devices[0].Enable();
            }

            #endregion

            if (NetworkDevice.Devices.Count == 0)
            {
                CustomConsole.WriteLineError("No supported network card found!!");
            }
            else
            {
                CustomConsole.WriteLineOK("Network initialization done!");
            }
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
