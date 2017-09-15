/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Computer Information
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System.IO;
using System;
using Alve_OS.System.Translation;

namespace Alve_OS.System.Computer
{
    class Info
    {

        /// <summary>
        /// Method to get the computer name
        /// </summary>
        /// <returns></returns>
        public static string getComputerName()
        {
            if (File.Exists(@"0:\System\computer.nam"))
            {
                Kernel.ComputerName = File.ReadAllText(@"0:\System\computer.nam");
                return Kernel.ComputerName;
            }
            else
            {
                setComputerName("Alve-PC");
                return "Alve-PC";
            }
        }

        /// <summary>
        /// Method to apply the computer name
        /// </summary>
        /// <param name="name"></param>
        public static void setComputerName(string name)
        {
            if (File.Exists(@"0:\System\computer.nam"))
            {
                File.Delete(@"0:\System\computer.nam");
                File.Create(@"0:\System\computer.nam");
                File.WriteAllText(@"0:\System\computer.nam", name);
            }
            else
            {
                File.Create(@"0:\System\computer.nam");
                File.WriteAllText(@"0:\System\computer.nam", name);

            }
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

            if((computername.Length >= 1) && (computername.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
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

        public static string GetAmountRAM()
        {
            return Cosmos.Core.CPU.GetAmountOfRAM() + "MB";
        }

        public static string GetMACAdress()
        {
            return Cosmos.HAL.Network.MACAddress.Broadcast + "";
        }

        public static string GetIPAdress()
        {
            throw new NotImplementedException();
        }
    }
}
