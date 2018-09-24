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
                if (!IsSavedConf())
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0), new Network.IPV4.Address(192, 168, 1, 1));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(Utils.Settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(Utils.Settings.GetValue("subnet")), Network.IPV4.Address.Parse(Utils.Settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
            }
            if (AMDPCNetIINIC != null)
            {
                if (!IsSavedConf())
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0), new Network.IPV4.Address(192, 168, 1, 1));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(Utils.Settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(Utils.Settings.GetValue("subnet")), Network.IPV4.Address.Parse(Utils.Settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
            }
        }

        static HAL.Drivers.Network.RTL8168 RTL8168NIC;
        static HAL.Drivers.Network.AMDPCNetII AMDPCNetIINIC;

        public static void Init()
        {
            CustomConsole.WriteLineInfo("Finding nic...");

            PCIDevice RTL8168 = PCI.GetDevice((VendorID)0x10EC, (DeviceID)0x8168);
            if (RTL8168 != null)
            {
                CustomConsole.WriteLineOK("Found RTL8168 NIC on PCI " + RTL8168.bus + ":" + RTL8168.slot + ":" + RTL8168.function);
                CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);

                RTL8168NIC = new HAL.Drivers.Network.RTL8168(RTL8168);

                CustomConsole.WriteLineInfo("NIC MAC Address: " + RTL8168NIC.MACAddress.ToString());

                Network.NetworkStack.Init();
                RTL8168NIC.Enable();
            }
            PCIDevice AMDPCNETII = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);

                AMDPCNetIINIC = new HAL.Drivers.Network.AMDPCNetII(AMDPCNETII);

                CustomConsole.WriteLineInfo("NIC MAC Address: " + AMDPCNetIINIC.MACAddress.ToString());

                Network.NetworkStack.Init();
                AMDPCNetIINIC.Enable();
            }
            if (RTL8168 == null && AMDPCNETII == null)
            {
                CustomConsole.WriteLineError("No supported network card found!!");
                return;
            }

            CustomConsole.WriteLineOK("Network initialization done!");
        }

        static bool IsSavedConf()
        {
            if (Setup.FileSystem() == "true")
            {
                Utils.Settings.LoadValues();
                if ((Utils.Settings.GetValue("ipaddress") != "192.168.1.70") || (Utils.Settings.GetValue("subnet") != "255.255.255.0") || (Utils.Settings.GetValue("gateway") != "192.168.1.1"))
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
