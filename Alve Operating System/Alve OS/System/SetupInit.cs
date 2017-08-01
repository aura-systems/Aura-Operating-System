using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alve_OS.System
{
    class SetupInit
    {

        public static void Init()
        {
            try
            {
            Directory.Exists(@"0:\System");
            Directory.Exists(@"0:\System\Users");
            Directory.Exists(@"0:\Users");
            }
            catch
            {
                Console.WriteLine("Error");
            }
        }

    }
}
