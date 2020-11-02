/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Init networking
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.HAL;

namespace Aura_OS.System
{
    class NetworkInit
    {

        public static void Enable()
        {            
            if (AMDPCNetIINIC != null)
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + AMDPCNetIINIC.Name + ".conf");
                if (!IsSavedConf(AMDPCNetIINIC.Name))
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 35), new Network.IPV4.Address(255, 255, 255, 0), new Network.IPV4.Address(192, 168, 1, 1));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(settings.GetValue("subnet")), Network.IPV4.Address.Parse(settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }

                Kernel.debugger.Send(Kernel.LocalNetworkConfig.ToString());

            }
        }

        static HAL.Drivers.Network.AMDPCNetII AMDPCNetIINIC;

        public static void Init(bool debug = true)
        {
            if (debug)
            {
                CustomConsole.WriteLineInfo("Finding nic...");
            }

            PCIDevice AMDPCNETII = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                if (debug)
                {
                    CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                    CustomConsole.WriteLineInfo("NIC IRQ: " + AMDPCNETII.InterruptLine);
                }
                AMDPCNetIINIC = new HAL.Drivers.Network.AMDPCNetII(AMDPCNETII);
                if (debug)
                {
                    CustomConsole.WriteLineInfo("NIC MAC Address: " + AMDPCNetIINIC.MACAddress.ToString());
                }
                Network.NetworkStack.Init();
                AMDPCNetIINIC.Enable();
            }
            if (AMDPCNETII == null)
            {
                CustomConsole.WriteLineError("No supported network card found!!");
                return;
            }
            if (debug)
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
