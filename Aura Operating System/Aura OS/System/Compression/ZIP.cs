using Aura_OS.System.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Compression
{
    class ZIP
    {
        private string ZIPFile;
        private List<int> FileHeaders = new List<int>();
        private List<int> FileEnds = new List<int>();
        int debug;

        public ZIP(string filename)
        {
            ZIPFile = filename;
        }

        private Byte[] BinaryContent()
        {
            Byte[] zip = File.ReadAllBytes(ZIPFile);
            return zip;            
        }

        private bool IsZIPFile()
        {
            Byte[] zip = BinaryContent();
            if ((zip[0] == 80) && (zip[1] == 75) && (((zip[2] == 3) && (zip[3] == 4)) || ((zip[2] == 5) && (zip[6] == 6)))) //80 75 3 4 or 80 75 5 6 
                                                                                                                            //(first for a zip file and second for an empty zip file)
            {
                return true;
            }
            return false;
        }

        public int Count()
        {
            Byte[] zip = BinaryContent();
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

        public List<string> ListFiles()
        {
            Byte[] zip = BinaryContent();
            List<Byte> Files = new List<Byte>();
            List<string> Names = new List<string>();
            int a = 0;
            int b = 0;
            int filenamesize = 0;
            int pointer = 0;

            foreach (Byte file in zip)
            {                
                if ((zip[a] == 80) && (zip[a + 1] == 75) && (((zip[a + 2] == 3) && (zip[a + 3] == 4))))
                {
                    b = a - 1;
                    if (b > 0)
                    {
                        FileEnds.Add(b);
                    }
                    //Console.WriteLine(a); //position pointer
                    FileHeaders.Add(a);

                    filenamesize = zip[a + 26]; // 8 - 12
                    //filenamesize = filenamesize + zip[a + 27]; //2 bytes
                    //Console.WriteLine(filenamesize);
                    pointer = filenamesize + 30;
                    for (int i = 30; i < pointer; i++)
                    {
                        Files.Add(zip[a + i]);
                    }
                    Files.Add(0x21); //separator !
                    filenamesize = 0;
                }

                if ((zip[a] == 80) && (zip[a + 1] == 75) && (((zip[a + 2] == 1) && (zip[a + 3] == 2))))
                {
                    FileEnds.Add(a - 1);                   
                    
                }

                a++;
            }
            string names = Encoding.ASCII.GetString(Files.ToArray());
            string[] files = names.Split('!');

            for (int i = 0; i < files.Length - 1; i++)
            {
                Names.Add(files[i]);
                //Console.WriteLine("zip > [" + i + "] " + files[i]);
            }

            return Names;
        }

        private uint ZipHash(Byte[] file)
        {
            return CRC_32.Crc32Algorithm.Compute(file);
        }

        public bool Integrity()
        {
            throw new NotImplementedException();
        }

        private int FileNameLenght(int a)
        {
            Byte[] zip = BinaryContent();
            int filenamesize = 0;
            filenamesize = zip[a + 26]; // 8 - 12
            filenamesize = filenamesize + zip[a + 27]; //2 bytes
            return filenamesize;
        }

        private int ExtraFieldLenght(int pointer)
        {
            Byte[] zip = BinaryContent();
            int extrafield = 0;
            extrafield = zip[pointer + 28];
            extrafield = extrafield + zip[pointer + 29];
            return extrafield;
        }

        private List<byte> CompressedFiles(int start, int end)
        {
            Byte[] zip = BinaryContent();
            List<byte> CompressedData = new List<byte>();

            for (int i = start; i < end; i++)
            {
                CompressedData.Add(zip[i]);
            }

            return CompressedData;
        }

        List<byte> uncompresseddata = new List<byte>();

        private void DeflateFunction()
        {
            uncompresseddata = Deflate.Inflate(CompressedFiles(1981, 1987));
        }

        int i = 0;
        public void Open()
        {
            Byte[] zip = BinaryContent();
            if (IsZIPFile()) //if it's a zip file
            {
                Console.WriteLine("zip > There is " + Count() + " file(s) in the zip archive.");
                Console.WriteLine();
                //Console.WriteLine("zip > CRC_32= " + ZipHash().ToString());
                Console.WriteLine();
                //ListFiles();
                foreach (var item in ListFiles())
                {
                    debug = 0;
                }
                foreach (int pointer in FileHeaders)
                {
                    Console.WriteLine("fileheader pointer: " + pointer);
                    Console.WriteLine("filenamelength: " + FileNameLenght(pointer));
                    Console.WriteLine("extrafieldlength: " + ExtraFieldLenght(pointer));
                    Console.WriteLine("file's start: " + (pointer + FileNameLenght(pointer) + ExtraFieldLenght(pointer) + 30));
                    Console.WriteLine("file's end: " +  FileEnds[i]);
                    Console.WriteLine();
                    i = i + 1;
                }

                DeflateFunction();

                foreach (int item in uncompresseddata)
                {
                    Console.WriteLine("---- UNCOMPRESSED DATA  ----- ");
                    Console.Write(item + " ");
                    Console.WriteLine();
                }
            }
        }
    }
}
