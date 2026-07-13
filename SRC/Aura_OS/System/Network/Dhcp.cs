/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dhcp utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4.UDP.DHCP;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Network
{
    public static class Dhcp
    {
        public static void Release()
        {
            var xClient = new DHCPClient();
            xClient.SendReleasePacket();

            // gen3 SendReleasePacket() closes the client itself and re-enables a
            // 0.0.0.0 placeholder config; RemoveAllConfigIP() (unlike the gen2
            // ClearConfigs()) also clears the route/interface tables so the stack
            // reads as unconfigured again.
            NetworkStack.RemoveAllConfigIP();

            Kernel.NetworkConnected = false;
            Explorer.Taskbar.MarkDirty();
        }

        public static bool Ask()
        {
            var xClient = new DHCPClient();
            if (xClient.SendDiscoverPacket() != -1)
            {
                xClient.Close();

                // gen3 CurrentAddress is nullable when no lease was applied.
                var currentAddress = NetworkConfigManager.CurrentAddress;
                if (currentAddress != null)
                {
                    Console.WriteLine("Configuration applied! Your local IPv4 Address is " + currentAddress + ".");
                }

                Kernel.NetworkConnected = true;
                return true;
            }
            else
            {
                NetworkStack.RemoveAllConfigIP();

                xClient.Close();
                return false;
            }
        }
    }
}
