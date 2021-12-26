/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Computer Information
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
//using Cosmos.HAL.PCInformation;

namespace Aura_OS.System.Computer
{
    class Info
    {

        /// <summary>
        /// Method to get the computer name
        /// </summary>
        /// <returns></returns>
        public static string getComputerName()
        {
            try
            {
                Settings config = new Settings(@"0:\System\settings.conf");
                string hostname = config.GetValue("hostname");
                Kernel.ComputerName = hostname;
                return hostname;
            }
            catch
            {
                return "aura-pc";
            }
        }

        public static byte[] getHostname()
        {
            return Encoding.ASCII.GetBytes(getComputerName());
        }

        public static int HostnameLength()
        {
            return getHostname().Length;
        }

        /// <summary>
        /// Method to apply the computer name
        /// </summary>
        /// <param name="name"></param>
        public static void setComputerName(string name)
        {
            Settings config = new Settings(@"0:\System\settings.conf");
            config.EditValue("hostname", name);
            config.PushValues();
            Kernel.ComputerName = name;
        }

        /// <summary>
        /// Method asking to user the computer name
        /// </summary>
        public static void AskComputerName()
        {
            Console.WriteLine();
            Text.Display("askcomputername");
            Console.WriteLine();
            Text.Display("computernamename");
            var computername = Console.ReadLine();

            if ((computername.Length >= 1) && (computername.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                setComputerName(computername);
                Console.WriteLine();

                Text.Display("computernamesuccess");
                Console.WriteLine();
            }
            else
            {
                Text.Display("computernameincorrect");
                Console.WriteLine();
                AskComputerName();
            }
        }

        /// <summary>
        /// Method to get the amount of RAM.
        /// </summary>
        public static string GetAmountRAM()
        {
            return Cosmos.Core.CPU.GetAmountOfRAM() + "MB";
        }

        /// <summary>
        /// Display the MAC address.
        /// </summary>
        public static string GetMACAddress()
        {
            return Cosmos.HAL.Network.MACAddress.Broadcast + "";
        }
    }
}
