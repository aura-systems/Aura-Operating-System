/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Network IPCONFIG
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Aura_OS.System.Network;
using Aura_OS.System.Network.DHCP;
using Aura_OS.System.Network.IPV4;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class IPConfig
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public IPConfig() { }

        static HAL.Drivers.Network.RTL8168 RTL8168NIC;

        /// <summary>
        /// c = command, c_SystemInfomation
        /// </summary>
        public static void c_IPConfig(string cmd)
        {
            string[] args = cmd.Split(' ');

            if (args[1] == "/release")
            {
                System.Network.DHCP.Core.SendReleasePacket();
            }
            else if (args[1] == "/set")
            {
                NetworkStack.RemoveAllConfigIP();

                if (Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]) && Utils.Misc.IsIpv4Address(args[5]))
                {
                    string Interface = args[2];

                    Utils.Settings settings = new Utils.Settings(@"0:\System\" + Interface + ".conf");
                    settings.EditValue("ipaddress", args[3]);
                    settings.EditValue("subnet", args[4]);
                    settings.EditValue("gateway", args[5]);
                    settings.EditValue("dns01", "0.0.0.0");
                    settings.PushValues();
                    
                    NetworkInit.Enable();
                }
                else
                {
                    L.Text.Display("notcorrectaddress");
                }                
            }
            else if (args[1] == "/renew")
            {
                System.Network.DHCP.Core.SendDiscoverPacket();
            }
            else
            {
                L.List_Translation.Ipconfig();
            }
        }
    }
}
