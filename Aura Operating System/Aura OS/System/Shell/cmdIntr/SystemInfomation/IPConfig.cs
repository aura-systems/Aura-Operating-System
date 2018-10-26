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

                NetworkStack.RemoveAllConfigIP();

                Utils.Settings.LoadValues();
                Utils.Settings.EditValue("ipaddress", "0.0.0.0");
                Utils.Settings.EditValue("subnet", "0.0.0.0");
                Utils.Settings.EditValue("gateway", "0.0.0.0");
                Utils.Settings.EditValue("dns01", "0.0.0.0");
                Utils.Settings.PushValues();
                
                NetworkInit.Enable();
            }
            else if (args[1] == "/set")
            {
                NetworkStack.RemoveAllConfigIP();

                if (Utils.Misc.IsIpv4Address(args[2]) && Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]))
                {
                    Utils.Settings.LoadValues();
                    Utils.Settings.EditValue("ipaddress", args[2]);
                    Utils.Settings.EditValue("subnet", args[3]);
                    Utils.Settings.EditValue("gateway", args[4]);
                    Utils.Settings.EditValue("dns01", "0.0.0.0");
                    Utils.Settings.PushValues();
                    
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
