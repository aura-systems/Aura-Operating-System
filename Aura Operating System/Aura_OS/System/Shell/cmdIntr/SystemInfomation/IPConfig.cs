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
using System.Linq;
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

            if (args.Length == 1)
            {
                L.List_Translation.Ipconfig();
                return;
            }

            if (args[1] == "/release")
            {
                System.Network.DHCP.Core.SendReleasePacket();
            }
            else if (args[1] == "/set")
            {
                if(args.Length <= 3)
                {
                    Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                    //ipconfig /set PCNETII 192.168.1.32 255.255.255.0 -g 192.168.1.254 -d 8.8.8.8
                }
                else
                {
                    if (NetworkInterfaces.Interface(args[2]) != "null")
                    {
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + NetworkInterfaces.Interface(args[2]) + ".conf");
                        NetworkStack.RemoveAllConfigIP();
                        ApplyIP(args, settings);
                        settings.Push();
                        NetworkInit.Enable();
                    }
                    else
                    {
                        Console.WriteLine("This interface doesn't exists.");
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

        private static void ApplyIP(string[] args, Utils.Settings settings)
        {
            int args_count = args.Length;
            switch (args_count)
            {
                default:
                    Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                    break;
                case 5:
                    if (Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        settings.Edit("gateway", "0.0.0.0");
                        settings.Edit("dns01", "0.0.0.0");
                    }
                    else
                    {
                        L.Text.Display("notcorrectaddress");
                    }
                    break;
                case 7:
                    if (Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]) && Utils.Misc.IsIpv4Address(args[6]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        if (args[5] == "-g")
                        {
                            settings.Edit("gateway", args[6]);
                            settings.Edit("dns01", "0.0.0.0");
                        }
                        else if (args[5] == "-d")
                        {
                            settings.Edit("dns01", args[6]);
                            settings.Edit("gateway", "0.0.0.0");
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }
                    }
                    else
                    {
                        L.Text.Display("notcorrectaddress");
                    }
                    break;
                case 9:
                    if (Utils.Misc.IsIpv4Address(args[3]) && Utils.Misc.IsIpv4Address(args[4]) && Utils.Misc.IsIpv4Address(args[6]) && Utils.Misc.IsIpv4Address(args[8]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        if (args[5] == "-g")
                        {
                            settings.Edit("gateway", args[6]);
                        }
                        else if (args[5] == "-d")
                        {
                            settings.Edit("dns01", args[6]);
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }

                        if (args[7] == "-g")
                        {
                            settings.Edit("gateway", args[8]);
                        }
                        else if (args[7] == "-d")
                        {
                            settings.Edit("dns01", args[8]);
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }
                    }
                    break;
                
            }
        }

    }
}
