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
    class CommandIPConfig : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandIPConfig(string[] commandvalues) : base(commandvalues)
        {
        }

        /// <summary>
        /// CommandIPConfig without args
        /// </summary>
        public override ReturnInfo Execute()
        {
            L.List_Translation.Ipconfig();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandIPConfig
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0] == "/release")
            {
                System.Network.DHCP.Core.SendReleasePacket();
            }
            else if (arguments[0] == "/set")
            {
                if (arguments.Count < 4)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {IPv4} {Subnet} {Gateway}");
                    //ipconfig /set PCNETII 192.168.1.32 255.255.255.0 -g 192.168.1.254 -d 8.8.8.8
                }
                else
                {
                    Address ip = Address.Parse(arguments[1]);
                    Address subnet = Address.Parse(arguments[2]);
                    Address gw = Address.Parse(arguments[3]);

                    if (ip != null && subnet != null && gw != null)
                    {
                        NetworkInit.Enable(ip, subnet, gw);
                        Console.WriteLine("Config OK!");
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP addresses (make sure they are well formated).");
                    }

                    /*if (NetworkInterfaces.Interface(arguments[1]) != "null")
                    {
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + NetworkInterfaces.Interface(arguments[1]) + ".conf");
                        NetworkStack.RemoveAllConfigIP();
                        ApplyIP(arguments.ToArray(), settings); //TODO Fix that (arguments)
                        settings.Push();
                    }
                    else
                    {
                        Console.WriteLine("This interface doesn't exists.");
                    } */
                }
            }
            else if (arguments[0] == "/renew")
            {
                System.Network.DHCP.Core.SendDiscoverPacket();
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {IPv4} {Subnet} {Gateway}");
            }
            return new ReturnInfo(this, ReturnCode.OK);
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
