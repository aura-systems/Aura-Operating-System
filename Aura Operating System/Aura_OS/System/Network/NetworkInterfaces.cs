using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Utils;
using System.Collections.Generic;

namespace Aura_OS.System.Network
{
    class NetworkInterfaces
    {
        private static List<string> PCIName = new List<string>();
        private static List<string> CustomName = new List<string>();

        public static void Init()
        {
            int ID = 0;
            Settings interfaces = new Settings(@"0:\System\interfaces.conf");
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                PCIName.Add(networkDevice.Name);
                if(networkDevice.CardType == CardType.Ethernet)
                {
                    CustomName.Add("eth" + ID);
                }
                else if (networkDevice.CardType == CardType.Wireless)
                {
                    CustomName.Add("wls" + ID);
                }
                else
                {
                    CustomName.Add("unk" + ID);
                }
                Save(ID, interfaces);
                ID++;
            }
            interfaces.Push();
        }

        public static string Interface(string interfaceName)
        {
            Settings interfaces = new Settings(@"0:\System\interfaces.conf");
            return interfaces.Get(interfaceName);            
        }

        private static void Save(int ID, Settings interfaces)
        {
            interfaces.Edit(CustomName[ID], PCIName[ID]);
        }
    }
}
