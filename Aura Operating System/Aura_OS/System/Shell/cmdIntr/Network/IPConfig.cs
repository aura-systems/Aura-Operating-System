/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Network IPCONFIG
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Network;
using Aura_OS.System.Network.UDP.DHCP;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Linq;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Network
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
            if (NetworkStack.ConfigEmpty())
            {
                Console.WriteLine("No network configuration detected! Use ipconfig /help");
            }
            foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
            {
                switch (device.CardType)
                {
                    case HAL.Drivers.Network.CardType.Ethernet:
                        Console.WriteLine("Ethernet Card : " + device.NameID + " - " + device.Name);
                        break;
                    case HAL.Drivers.Network.CardType.Wireless:
                        Console.WriteLine("Wireless Card : " + device.NameID + " - " + device.Name);
                        break;
                }
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                Console.WriteLine("MAC Address          : " + device.MACAddress.ToString());
                Console.WriteLine("IP Address           : " + NetworkConfig.Get(device).IPAddress.ToString());
                Console.WriteLine("Subnet mask          : " + NetworkConfig.Get(device).SubnetMask.ToString());
                Console.WriteLine("Default Gateway      : " + NetworkConfig.Get(device).DefaultGateway.ToString());
                Console.WriteLine("Preferred DNS server : " + settings.GetValue("dns01"));
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandIPConfig
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0] == "/help")
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("- ipconfig /listnic    List network devices");
                Console.WriteLine("- ipconfig /set        Manually set an IP Address");
                Console.WriteLine("- ipconfig /ask        Find the DHCP server and ask a new IP address");
                Console.WriteLine("- ipconfig /release    Tell the DHCP server to make the IP address available");
            }
            else if (arguments[0] == "/release")
            {
                //System.Network.UDP.DHCP.Core.SendReleasePacket();
            }
            else if (arguments[0] == "/ask")
            {
                System.Network.UDP.DHCP.Core.SendDiscoverPacket();
            }
            else if (arguments[0] == "/listnic")
            {
                foreach (var device in NetworkDevice.Devices)
                {
                    switch (device.CardType)
                    {
                        case CardType.Ethernet:
                            Console.WriteLine("Ethernet Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                        case CardType.Wireless:
                            Console.WriteLine("Wireless Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                    }
                }
            }
            else if (arguments[0] == "/set")
            {
                if (arguments.Count < 5)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {device} {IPv4} {Subnet} {Gateway}");
                }
                else
                {
                    Address ip = Address.Parse(arguments[2]);
                    Address subnet = Address.Parse(arguments[3]);
                    Address gw = Address.Parse(arguments[4]);
                    NetworkDevice nic = NetworkDevice.GetDeviceByName(arguments[1]);

                    if (nic == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Couldn't find network device: " + arguments[1]);
                    }
                    if (ip != null && subnet != null && gw != null)
                    {
                        NetworkInit.Enable(nic, ip, subnet, gw);
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
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Wrong usage, please type: ipconfig /help");
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
