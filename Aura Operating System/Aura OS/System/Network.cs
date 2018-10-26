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
            if (RTL8168NIC != null)
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + RTL8168NIC.Name + ".conf");
                if (!IsSavedConf(RTL8168NIC.Name))
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(0,0,0,0), new Network.IPV4.Address(0,0,0,0), new Network.IPV4.Address(0,0,0,0));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(settings.GetValue("subnet")), Network.IPV4.Address.Parse(settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
            }
            if (AMDPCNetIINIC != null)
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + AMDPCNetIINIC.Name + ".conf");
                if (!IsSavedConf(AMDPCNetIINIC.Name))
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(0,0,0,0), new Network.IPV4.Address(0,0,0,0), new Network.IPV4.Address(0,0,0,0));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(settings.GetValue("subnet")), Network.IPV4.Address.Parse(settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
            }
        }

        static HAL.Drivers.Network.RTL8168 RTL8168NIC;
        static HAL.Drivers.Network.AMDPCNetII AMDPCNetIINIC;

        public static void Init(bool debug = true)
        {
            if (debug)
            {
                CustomConsole.WriteLineInfo("Finding nic...");
            }

            PCIDevice RTL8168 = PCI.GetDevice((VendorID)0x10EC, (DeviceID)0x8168);
            if (RTL8168 != null)
            {
                if (debug)
                {
                    CustomConsole.WriteLineOK("Found RTL8168 NIC on PCI " + RTL8168.bus + ":" + RTL8168.slot + ":" + RTL8168.function);
                    CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);
                }
                RTL8168NIC = new HAL.Drivers.Network.RTL8168(RTL8168);
                if (debug)
                {
                    CustomConsole.WriteLineInfo("NIC MAC Address: " + RTL8168NIC.MACAddress.ToString());
                }
                Network.NetworkStack.Init();
                RTL8168NIC.Enable();
            }
            PCIDevice AMDPCNETII = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                if (debug)
                {
                    CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                    CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);
                }
                AMDPCNetIINIC = new HAL.Drivers.Network.AMDPCNetII(AMDPCNETII);
                if (debug)
                {
                    CustomConsole.WriteLineInfo("NIC MAC Address: " + AMDPCNetIINIC.MACAddress.ToString());
                }
                Network.NetworkStack.Init();
                AMDPCNetIINIC.Enable();
            }
            if (RTL8168 == null && AMDPCNETII == null)
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
