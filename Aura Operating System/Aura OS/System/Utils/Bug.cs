using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Utils
{
    class Bug
    {
        public int IssuesNumber = 0;

        public void ScanIssues()
        {
            //How to create a new type of error ?
            //0 x ? ?
            //0 -> I don't know why, just for the style
            //x -> Same.
            //?? -> First: Level of error 0-9; Second: Number of the problem;

            if (!Directory.Exists(@"0:\System"))
            {
                IssuesNumber++;
                Crash.StopKernel("Error 0x91: Aura is not installed anymore");
            }
            else if(!Directory.Exists(@"0:\Users"))
            {
                IssuesNumber++;
                Console.WriteLine("Error 0x01: Users personnal folders doesn't exist !");
            }
            else if (!File.Exists(@"0:\System\settings.conf"))
            {
                IssuesNumber++;
                Crash.StopKernel("Error 0x92: Config file doesn't exist ! Need to be reinstalled.");
            }
            
            if (IssuesNumber <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Scan is finished ! Aura has not encountered a bug !");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Scan is finished ! Aura has encountered {IssuesNumber} bug(s) !");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

        public void DeclareBug()
        {
            throw new NotImplementedException("Need Networking");
        }

        public void CreateReport()
        {
            throw new NotImplementedException("Later, or networking");
        }        

    }
}
