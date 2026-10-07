/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Network IPCONFIG
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Aura_OS.System.Network;
using Cosmos.Kernel.System.Network;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
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
            if (!NetworkHelper.IsConfigured)
            {
                Console.WriteLine("No network configuration detected! Use ipconfig /help");
            }

            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                NetworkAdapter adapter = NetworkManager.GetAdapter(i);
                IPConfig config = adapter.IPConfig;

                // GEN3-GAP(dhcp): a DHCP timeout/release can leave a 0.0.0.0 config behind, it counts as none.
                if (config is null || config.Address.IsZero)
                {
                    continue;
                }

                // GEN3-GAP(nic-names): no interface names nor card types; both gen3 drivers are Ethernet.
                Console.Write("Ethernet Card : " + NetworkHelper.AliasOf(adapter) + " - " + adapter.Name);

                if (adapter == NetworkManager.Primary)
                {
                    Console.WriteLine(" (current)");
                }
                else
                {
                    Console.WriteLine();
                }

                Console.WriteLine("MAC Address          : " + adapter.MacAddress?.ToString());
                Console.WriteLine("IP Address           : " + config.Address.ToString());
                Console.WriteLine("Subnet mask          : " + config.SubnetMask.ToString());
                Console.WriteLine("Default Gateway      : " + config.DefaultGateway.ToString());
                Console.WriteLine("DNS Nameservers      : ");
                for (int j = 0; j < DnsConfig.Nameservers.Count; j++)
                {
                    Console.WriteLine("                       " + DnsConfig.Nameservers[j].ToString());
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
            if (arguments.Count == 0)
            {
                return Execute();
            }

            if (arguments[0] == "/release")
            {
                Dhcp.Release();
            }
            else if (arguments[0] == "/ask")
            {
                if (Dhcp.Ask())
                {
                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "DHCP Discover failed. Can't apply dynamic IPv4 address.");
                }
            }
            else if (arguments[0] == "/listnic")
            {
                if (NetworkManager.DeviceCount == 0)
                {
                    Console.WriteLine("No network device found.");
                }

                for (int i = 0; i < NetworkManager.DeviceCount; i++)
                {
                    NetworkAdapter adapter = NetworkManager.GetAdapter(i);

                    // GEN3-GAP(nic-names): eth{Index} aliases, no card type (both gen3 drivers are Ethernet).
                    Console.WriteLine("Ethernet Card - " + NetworkHelper.AliasOf(adapter) + " - " + adapter.Name
                        + " (" + adapter.MacAddress?.ToString() + ")"
                        + (adapter.LinkUp ? " - link up" : " - link down")
                        + (adapter == NetworkManager.Primary ? " (current)" : ""));
                }
            }
            else if (arguments[0] == "/set")
            {
                if (arguments.Count != 3 && arguments.Count != 4) // ipconfig /set eth0 192.168.1.2/24 {gw|null}
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {device} {IPv4/CIDR} [Gateway|null]");
                }

                NetworkAdapter nic = NetworkHelper.FindAdapter(arguments[1]);
                if (!nic.IsValid)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Couldn't find network device: " + arguments[1] + " (see ipconfig /listnic)");
                }

                string[] adrnetwork = arguments[2].Split('/');
                if (adrnetwork.Length != 2)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP address, use the IPv4/CIDR format (e.g. 192.168.1.2/24).");
                }

                int cidr;
                if (!int.TryParse(adrnetwork[1], out cidr) || cidr < 1 || cidr > 32)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Invalid CIDR: " + adrnetwork[1] + " (expected 1 to 32).");
                }

                Address4 ip = Address4.Parse(adrnetwork[0], AddressNumericStyle.Dec);
                Address4 subnet = Address4.CIDRToAddress(cidr);

                // No gateway (or "null"): 0.0.0.0, only on-link traffic works.
                Address4 gw = Address4.Zero;
                if (arguments.Count == 4 && arguments[3] != "null")
                {
                    gw = Address4.Parse(arguments[3], AddressNumericStyle.Dec);
                }

                if (ip is null || subnet is null || gw is null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP addresses (make sure they are well formated).");
                }

                bool applied;
                try
                {
                    applied = IPConfig.Enable(nic, ip, subnet, gw);
                }
                catch (Exception ex)
                {
                    // GEN3-GAP(dhcp): ArgumentException when another adapter already holds this address
                    // (duplicate key in the stack's address map).
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't apply the configuration: " + ex.Message);
                }

                if (!applied)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't apply the configuration to " + arguments[1] + ".");
                }

                // GEN3-GAP(tcp-primary): TCP always sources from NetworkManager.Primary, make the configured card primary.
                if (nic != NetworkManager.Primary)
                {
                    try
                    {
                        NetworkManager.Primary = nic;
                    }
                    catch (Exception)
                    {
                        // Invalid handle or network disabled: keep the current primary.
                    }
                }

                Console.WriteLine("Config OK!");
                Kernel.NetworkConnected = true;
            }
            else if (arguments[0] == "/nameserver")
            {
                if (arguments.Count != 3 || (arguments[1] != "-add" && arguments[1] != "-rem"))
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /nameserver {-add|-rem} {IP}");
                }

                Address nameserver = Address.Parse(arguments[2]);
                if (nameserver is null)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP address: " + arguments[2]);
                }

                if (arguments[1] == "-add")
                {
                    DnsConfig.Add(nameserver);
                    Console.WriteLine(nameserver.ToString() + " has been added to nameservers.");
                }
                else
                {
                    DnsConfig.Remove(nameserver);
                    Console.WriteLine(nameserver.ToString() + " has been removed from nameservers list.");
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
            Console.WriteLine("Available commands:");
            Console.WriteLine("- ipconfig /listnic      List network devices");
            Console.WriteLine("- ipconfig /ask          Find the DHCP server and ask a new IP address");
            Console.WriteLine("- ipconfig /release      Tell the DHCP server to make the IP address available");
            Console.WriteLine("- ipconfig /set          Manually set an IP Address");
            Console.WriteLine("     Usage:");
            Console.WriteLine("     - ipconfig /set {device} {IPv4/CIDR} [Gateway|null]");
            Console.WriteLine("     - {device} is eth0, eth1... (see /listnic), e.g. ipconfig /set eth0 192.168.1.2/24 192.168.1.254");
            Console.WriteLine("- ipconfig /nameserver   Manually set an DNS server");
            Console.WriteLine("     Usage:");
            Console.WriteLine("     - ipconfig /nameserver {-add|-rem} {IP}");
        }
    }
}
