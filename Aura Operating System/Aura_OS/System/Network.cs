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
                Config config = new Config(ip, subnet, gw);
                Network.NetworkStack.ConfigIP(device, config);
                Kernel.debugger.Send(config.ToString());
                return true;
            }
            return false;
        }

        public static void Init()
        {
            CustomConsole.WriteLineInfo("Searching for Network cards...");

            #region AMDPCNETII

            int NetworkDeviceID = 0;

            CustomConsole.WriteLineInfo("Searching for Ethernet Controller...");
            foreach (Cosmos.HAL.PCIDevice device in Cosmos.HAL.PCI.Devices)
            {
                if ((device.ClassCode == 0x02) && (device.Subclass == 0x00)) // is Ethernet Controller
                {
                    if (device == Cosmos.HAL.PCI.GetDevice(device.bus, device.slot, device.function))
                    {
                        CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + device.bus + ":" + device.slot + ":" + device.function);
                        CustomConsole.WriteLineInfo("NIC IRQ: " + device.InterruptLine);

                        var AMDPCNetIIDevice = new AMDPCNetII(device);

                        AMDPCNetIIDevice.NameID = ("eth" + NetworkDeviceID);

                        CustomConsole.WriteLineInfo("NIC MAC Address: " + AMDPCNetIIDevice.MACAddress.ToString());

                        Network.NetworkStack.Init();
                        AMDPCNetIIDevice.Enable();

                        NetworkDeviceID++;
                    }                    
                }
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
