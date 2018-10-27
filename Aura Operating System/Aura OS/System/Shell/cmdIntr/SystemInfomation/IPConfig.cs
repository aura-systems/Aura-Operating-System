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
                if(args.Length <= 3)
                {
                    Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                }
                else
                {
                    string Interface = args[2];
                    Utils.Settings settings = new Utils.Settings(@"0:\System\" + Interface + ".conf");                    

                    if (args.Length >= 5)
                    {   
                        if (Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]))
                        {
                            NetworkStack.RemoveAllConfigIP();
                            settings.Edit("ipaddress", args[3]);
                            settings.Edit("subnet", args[4]);
                            Reload(settings);
                        }
                        else
                        {
                            L.Text.Display("notcorrectaddress");
                        }
                    }
                    else if (args.Length == 7)
                    {
                        if (Utils.Misc.IsIpv4Address(args[6]))
                        {
                            if (args[5] == "-g")
                            {
                                settings.Edit("gateway", args[6]);
                            }
                            else
                            {
                                settings.Edit("gateway", "0.0.0.0");
                            }

                            if (args[5] == "-d")
                            {
                                settings.Edit("dns01", args[6]);
                            }
                            else
                            {
                                settings.Edit("dns01", "0.0.0.0");
                            }

                            Reload(settings);
                        }
                        else
                        {
                            L.Text.Display("notcorrectaddress");
                        }
                    }
                    else if (args.Length == 9)
                    {
                        if (Utils.Misc.IsIpv4Address(args[6]) && Utils.Misc.IsIpv4Address(args[7]))
                        {
                            if (args[5] == "-g")
                            {
                                settings.Edit("gateway", args[6]);
                            }
                            if (args[6] == "-d")
                            {
                                settings.Edit("dns01", args[7]);
                            }

                            Reload(settings);
                        }
                        else
                        {
                            L.Text.Display("notcorrectaddress");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                    }                                     
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


        private static void Reload(Utils.Settings settings)
        {
            settings.Push();

            NetworkInit.Enable();
        }
    }
}
