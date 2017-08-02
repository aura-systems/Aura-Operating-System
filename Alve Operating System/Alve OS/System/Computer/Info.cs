using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            } else
            {
                File.Create(@"0:\System\computer.nam");
                File.WriteAllText(@"0:\System\computer.nam", name);

            }
        }

    }
}
