using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Compression
{
    class ZIP
    {
        private string ZIPFile;
        int pointer = 0;

        public ZIP(string filename)
        {
            ZIPFile = filename;
        }

        private Byte[] FileHeader()
        {
            Byte[] zip = File.ReadAllBytes(ZIPFile);
            return zip;            
        }

        private bool IsZIPFile()
        {
            Byte[] zip = FileHeader();
            if ((zip[0] == 80) && (zip[1] == 75) && (((zip[2] == 3) && (zip[3] == 4)) || ((zip[2] == 5) && (zip[6] == 6)))) //80 75 3 4 or 80 75 5 6 
                                                                                                                            //(first for a zip file and second for an empty zip file)
            {
                return true;
            }
            return false;
        }

        private bool IsVersion()
        {
            Byte[] zip = FileHeader();
            if ((zip[4] == 10) && (zip[5] == 00))
            {
                return true;
            }
            return false;
        }

        private string ListFiles()
        {
            Byte[] zip = FileHeader();
            List<Byte> bname = new List<Byte>();

            //detect all 80 75 3 4
            //register their positions
            //get their names
            string signature = "";
            int pointer = 0;
            Byte[] test = new Byte[] { 0x50, 0x4b, 0x03, 0x04, 0xa, 0x0, 0x8, 0x50, 0x4b, 0x03, 0x04, 0xa, 0x0, 0x8 };
            foreach (Byte file in test)
            {
                pointer = pointer + 1;

                if (file == 80)
                {
                    signature = signature + 80;
                    Console.WriteLine("stop " + pointer);
                    Console.WriteLine("file detected!");
                    continue;
                }

                if (file == 75)
                {
                    signature = signature + 75;
                    continue;
                }

                if (file == 3)
                {
                    signature = signature + 3;
                    continue;
                }

                if (file == 4)
                {
                    signature = signature + 4;
                    continue;
                }
            }

            //int lenght = zip[26] + zip[27]; //13 + 0 = 13
            //Console.WriteLine("zip> filename lenght: " + lenght);
            //for (int i = 30; i < 30+lenght; i++){
            //    bname.Add(zip[i]);
            //    pointer = i;
            //    Console.WriteLine("zip> byte 0x" + pointer + " " + zip[i]);
            //}
            //pointer = pointer + 1;
            return Encoding.ASCII.GetString(bname.ToArray());
        }

        public void Open()
        {
            Byte[] zip = FileHeader();
            if (IsZIPFile()) //if it's a zip file
            {
                if (IsVersion()) //if the zip file is supported
                {
                    Console.ForegroundColor = ConsoleColor.Green;                    
                    Console.WriteLine("Good version: executing zip file...");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine();
                Console.WriteLine(ListFiles());
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("zip> byte 0x" + pointer + " " + zip[pointer]);
            }
        }
    }
}
