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

        public int Count()
        {
            Byte[] zip = FileHeader();
            List<string> signatures = new List<string>();            
            int a = 0;
            int count = 0;
            foreach (Byte file in zip)
            {
                if ((zip[a] == 80) && (zip[a + 1] == 75) && (((zip[a + 2] == 3) && (zip[a + 3] == 4))))
                {
                    count++;
                }
                a++;
            }

            return count;
        }

        public void ListFiles()
        {
            Byte[] zip = FileHeader();
            List<Byte> Files = new List<Byte>();
            int a = 0;
            int filenamesize = 0;
            int pointer = 0;

            foreach (Byte file in zip)
            {
                if ((zip[a] == 80) && (zip[a + 1] == 75) && (((zip[a + 2] == 3) && (zip[a + 3] == 4))))
                {
                    filenamesize = zip[a + 26]; // 8 - 12
                    //Console.WriteLine(filenamesize);
                    pointer = filenamesize + 30;
                    for (int i = 30; i < pointer; i++)
                    {
                        Files.Add(zip[a + i]);
                    }
                    Files.Add(0x21); //separator !
                    filenamesize = 0;
                }
                a++;
            }
            string names = Encoding.ASCII.GetString(Files.ToArray());
            string[] files = names.Split('!');

            for (int i = 0; i < files.Length - 1; i++)
            {
                Console.WriteLine("zip > [" + i + "] " + files[i]);
            }

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
                Console.WriteLine("zip > There is " + Count() + " file(s) in the zip archive.");
                Console.WriteLine();
                ListFiles();
            }
        }
    }
}
