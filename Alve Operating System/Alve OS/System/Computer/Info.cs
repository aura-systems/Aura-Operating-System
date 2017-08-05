
using System.IO;
using System;
using Alve_OS.System.Translation;

namespace Alve_OS.System.Computer
{
    class Info
    {

        public static string getComputerName()
        {
            return File.ReadAllText(@"0:\System\computer.nam");
        }

        public static void setComputerName(string name)
        {
            if (File.Exists(@"0:\System\computer.nam"))
            {
                File.WriteAllText(@"0:\System\computer.nam", name);
            }
            else
            {
                File.Create(@"0:\System\computer.nam");
                File.WriteAllText(@"0:\System\computer.nam", name);

            }
        }

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

        

    }
}
