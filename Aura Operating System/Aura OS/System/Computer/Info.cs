/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Computer Information
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
using Cosmos.HAL.PCInformation;

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
                Settings.LoadValues();
                string hostname = Settings.GetValue("hostname");
                Kernel.ComputerName = hostname;
                return hostname;
            }
            catch
            {
                return "aura-pc";
            }
        }

        /// <summary>
        /// Method to apply the computer name
        /// </summary>
        /// <param name="name"></param>
        public static void setComputerName(string name)
        {
            Settings.LoadValues();
            Settings.EditValue("hostname", name);
            Settings.PushValues();
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

        /// <summary>
        /// Display local IP Address.
        /// </summary>
        public static string GetIPAddress()
        {
            return "0.0.0.0/0";
        }

        /// <summary>
        /// Number of CPU
        /// </summary>
        public static int GetNumberOfCPU()
        {
            int number = 0;
            foreach (var x in ListProcessors)
            {
                CPUInfo.Processors.Add(x);
                number++;
            }
            return number;
        }

        private static List<Processor> _listProcessors;
        public static List<Processor> ListProcessors
        {
            get
            {
                //This is to allow multiprocessor on a future
                //TODO: search a list of processors based on the topology
                if (_listProcessors == null)
                {
                    _listProcessors = new List<Processor>();
                    _listProcessors.Add(new Processor());
                }
                return _listProcessors;
            }
        }
    }
}
