/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Network IPCONFIG
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Cosmos.HAL;
using Cosmos.System.Network;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Aura_OS;
using System;
using System.Collections.Generic;
using Aura_OS.Interpreter;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandIPConfig : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandIPConfig(string[] commandvalues) : base(commandvalues)
        {
            Description = "to set a static IP or get an IP from the DHCP server";
        }

        /// <summary>
        /// CommandIPConfig without args
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (NetworkStack.ConfigEmpty())
            {
                Kernel.console.WriteLine("No network configuration detected! Use ipconfig /help");
            }
            foreach (NetworkDevice device in NetworkConfig.Keys)
            {
                switch (device.CardType)
                {
                    case CardType.Ethernet:
                        Kernel.console.Write("Ethernet Card : " + device.NameID + " - " + device.Name);
                        break;
                    case CardType.Wireless:
                        Kernel.console.Write("Wireless Card : " + device.NameID + " - " + device.Name);
                        break;
                }
                if (NetworkConfig.CurrentConfig.Key == device)
                {
                    Kernel.console.WriteLine(" (current)");
                }
                else
                {
                    Kernel.console.WriteLine();
                }
               
                Kernel.console.WriteLine("MAC Address          : " + device.MACAddress.ToString());
                Kernel.console.WriteLine("IP Address           : " + NetworkConfig.Get(device).IPAddress.ToString());
                Kernel.console.WriteLine("Subnet mask          : " + NetworkConfig.Get(device).SubnetMask.ToString());
                Kernel.console.WriteLine("Default Gateway      : " + NetworkConfig.Get(device).DefaultGateway.ToString());
                Kernel.console.WriteLine("DNS Nameservers      : ");
                foreach (Address dnsnameserver in DNSConfig.DNSNameservers)
                {
                    Kernel.console.WriteLine("                       " + dnsnameserver.ToString());
                }
            }

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
                var xClient = new DHCPClient();
                xClient.SendReleasePacket();
                xClient.Close();

                NetworkConfig.Keys.Clear();
                NetworkConfig.Values.Clear();
            }
            else if (arguments[0] == "/ask")
            {
                var xClient = new DHCPClient();
                if (xClient.SendDiscoverPacket() != -1)
                {
                    xClient.Close();
                    Kernel.console.WriteLine("Configuration applied! Your local IPv4 Address is " + NetworkConfig.CurrentConfig.Value.IPAddress.ToString() + ".");
                }
                else
                {
                    NetworkConfig.Keys.Clear();
                    NetworkConfig.Values.Clear();

                    xClient.Close();
                    return new ReturnInfo(this, ReturnCode.ERROR, "DHCP Discover failed. Can't apply dynamic IPv4 address.");
                }
            }
            else if (arguments[0] == "/listnic")
            {
                foreach (var device in NetworkDevice.Devices)
                {
                    switch (device.CardType)
                    {
                        case CardType.Ethernet:
                            Kernel.console.WriteLine("Ethernet Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                        case CardType.Wireless:
                            Kernel.console.WriteLine("Wireless Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                    }
                }
            }
            else if (arguments[0] == "/set")
            {
                if ((arguments.Count == 3) || (arguments.Count == 4)) // ipconfig /set eth0 192.168.1.2/24 {gw}
                {
                    string[] adrnetwork = arguments[2].Split('/');
                    Address ip = Address.Parse(adrnetwork[0]);
                    NetworkDevice nic = NetworkDevice.GetDeviceByName(arguments[1]);
                    Address gw = null;
                    if (arguments.Count == 4)
                    {
                        gw = Address.Parse(arguments[3]);
                    }

                    int cidr;
                    try
                    {
                        cidr = int.Parse(adrnetwork[1]);
                    }
                    catch (Exception ex)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
                    }
                    Address subnet = Address.CIDRToAddress(cidr);

                    if (nic == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Couldn't find network device: " + arguments[1]);
                    }

                    if (ip != null && subnet != null && gw != null)
                    {
                        IPConfig.Enable(nic, ip, subnet, gw);
                        Kernel.console.WriteLine("Config OK!");
                    }
                    else if (ip != null && subnet != null)
                    {
                        IPConfig.Enable(nic, ip, subnet, ip);
                        Kernel.console.WriteLine("Config OK!");
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP addresses (make sure they are well formated).");
                    }
                }                
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {device} {IPv4/CIDR} {Gateway|null}");
                }
            }
            else if (arguments[0] == "/nameserver")
            {
                if (arguments[1] == "-add")
                {
                    DNSConfig.Add(Address.Parse(arguments[2]));
                    Kernel.console.WriteLine(arguments[2] + " has been added to nameservers.");
                }
                else if (arguments[1] == "-rem")
                {
                    DNSConfig.Remove(Address.Parse(arguments[2]));
                    Kernel.console.WriteLine(arguments[2] + " has been removed from nameservers list.");
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /nameserver {add|remove} {IP}");
                }                
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Wrong usage, please type: ipconfig /help");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Kernel.console.WriteLine("Available commands:");
            Kernel.console.WriteLine("- ipconfig /listnic      List network devices");
            Kernel.console.WriteLine("- ipconfig /ask          Find the DHCP server and ask a new IP address");
            Kernel.console.WriteLine("- ipconfig /release      Tell the DHCP server to make the IP address available");
            Kernel.console.WriteLine("- ipconfig /set          Manually set an IP Address");
            Kernel.console.WriteLine("     Usage:");
            Kernel.console.WriteLine("     - ipconfig /set {device} {IPv4} {Subnet} {Gateway}");
            Kernel.console.WriteLine("- ipconfig /nameserver   Manually set an DNS server");
            Kernel.console.WriteLine("     Usage:");
            Kernel.console.WriteLine("     - ipconfig /nameserver {-add|-rem} {IPv4}");
        }
    }
}
