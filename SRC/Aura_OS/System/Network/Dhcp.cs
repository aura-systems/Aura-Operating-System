/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Dhcp utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System.Network;

namespace Aura_OS.System.Network
{
    public static class Dhcp
    {
        public static void Release()
        {
            // GEN3-GAP(dhcp): SendReleasePacket throws when nothing is configured (and always on a
            // second NIC), so only release a real lease, and never let it escape.
            if (NetworkHelper.IsConfigured)
            {
                DhcpClient xClient = null;

                try
                {
                    xClient = new DhcpClient();
                    xClient.SendReleasePacket();
                }
                catch (Exception)
                {
                    // No route to the server: nothing to release remotely.
                }

                // gen3: finally/using do not run on the exception path, dispose explicitly.
                if (xClient != null)
                {
                    xClient.Dispose();
                }
            }

            // GEN3-GAP(dhcp): SendReleasePacket leaves a 0.0.0.0 config behind, clear everything.
            NetworkStack.RemoveAllConfigIP();

            Kernel.NetworkConnected = false; // the setter marks the taskbar dirty
        }

        public static bool Ask()
        {
            if (NetworkManager.DeviceCount == 0)
            {
                Console.WriteLine("No network device found.");
                return false;
            }

            DhcpClient xClient = null;
            int elapsed;

            try
            {
                xClient = new DhcpClient();
                elapsed = xClient.SendDiscoverPacket();
            }
            catch (Exception ex)
            {
                // GEN3-GAP(dhcp): ArgumentException with 2+ NICs (duplicate 0.0.0.0 in the address map),
                // plain Exception when an ACK carries no client address.
                Console.WriteLine("DHCP failed: " + ex.Message);
                elapsed = -1;
            }

            if (xClient != null)
            {
                xClient.Dispose();
            }

            if (elapsed == -1)
            {
                NetworkStack.RemoveAllConfigIP();
                Kernel.NetworkConnected = false;
                return false;
            }

            // GEN3-GAP(dhcp): a DHCPNAK is applied like an ACK (0.0.0.0) and a stray reply returns
            // success with nothing applied, so check the address that is really configured.
            Address address = NetworkHelper.CurrentAddress;
            if (address is null)
            {
                NetworkStack.RemoveAllConfigIP();
                Kernel.NetworkConnected = false;
                return false;
            }

            Console.WriteLine("Configuration applied! Your local IPv4 Address is " + address.ToString() + ".");
            Kernel.NetworkConnected = true;
            return true;
        }
    }
}
