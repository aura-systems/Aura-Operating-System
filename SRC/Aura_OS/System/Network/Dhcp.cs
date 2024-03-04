/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dhcp utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Aura_OS.System.Processing.Processes;

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
