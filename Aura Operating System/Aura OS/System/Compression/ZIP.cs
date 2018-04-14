using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Compression
{
    class ZIP
    {
        private string ZIPFile;

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
            if ((zip[0] == 80) && (zip[1] == 75) && (zip[2] == 3) && (zip[3] == 4))
            {
                return true;
            }
            return false;
        }

        private bool IsVersionNeeded()
        {
            Byte[] zip = FileHeader();
            if ((zip[4] == 10) && (zip[5] == 00))
            {
                return true;
            }
            return false;
        }

        private string FileName()
        {
            Byte[] zip = FileHeader();
            List<Byte> bname = new List<Byte>();            
            int lenght = zip[26];
            Console.WriteLine(lenght);
            Console.WriteLine(zip[30]);
            for (int i = 30; i < lenght; i++){
                bname.Add(zip[i]);
                Console.WriteLine(zip[i]);
            }
            return Encoding.ASCII.GetString(bname.ToArray());
        }

        public void Open()
        {
            Byte[] zip = FileHeader();
            if (IsZIPFile()) //if it's a zip file
            {
                //if (IsVersionNeeded()) //if the zip file is supported
                //{
                //    Console.WriteLine(FileName());
                //}
                Console.WriteLine(FileName());
            }
        }
    }
}
