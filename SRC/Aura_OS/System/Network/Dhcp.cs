/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dhcp utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using CosmosHttp.Client;
using System.Net;
using System.Text;
using Aura_OS.System.Processing.Processes;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Aura_OS.System.Processing.Interpreter.Commands;
using System;

namespace Aura_OS.System.Network
{
    public static class Dhcp
    {
        public static void Release()
        {
            var xClient = new DHCPClient();
            xClient.SendReleasePacket();
            xClient.Close();

            NetworkConfiguration.ClearConfigs();

            Kernel.NetworkConnected = false;
            Explorer.Taskbar.MarkDirty();
        }

        public static bool Ask()
        {
            var xClient = new DHCPClient();
            if (xClient.SendDiscoverPacket() != -1)
            {
                xClient.Close();
                Console.WriteLine("Configuration applied! Your local IPv4 Address is " + NetworkConfiguration.CurrentAddress + ".");
                Kernel.NetworkConnected = true;
                return true;
            }
            else
            {
                NetworkConfiguration.ClearConfigs();

                xClient.Close();
                return false;
            }
        }
    }
}
