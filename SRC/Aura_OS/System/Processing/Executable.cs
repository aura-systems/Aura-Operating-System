/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Executable class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Aura_OS.System.Processing
{
    public class Executable
    {
        private const string ExpectedSignature = "CEXE";
        private const int SignatureSize = 4;
        private const int ArchiveSizeLength = 4;

        public string Signature { get; private set; }
        public byte[] RawData { get; private set; }
        public int ArchiveSize { get; private set; }
        public Dictionary<string, byte[]> LuaSources { get; set; }
        private byte[] ZipContent { get; set; }

        public Executable(byte[] executableBytes)
        {
            RawData = executableBytes;
            LuaSources = new Dictionary<string, byte[]>();
            ParseExecutable(executableBytes);
        }

        private void ParseExecutable(byte[] executableBytes)
        {
            Signature = Encoding.ASCII.GetString(executableBytes, 0, SignatureSize);

            if (Signature != ExpectedSignature)
            {
                throw new InvalidOperationException("This is not a Cosmos executable.");
            }

            ArchiveSize = BitConverter.ToInt32(executableBytes, SignatureSize);
            if (SignatureSize + ArchiveSizeLength + ArchiveSize > executableBytes.Length)
            {
                throw new InvalidOperationException("Cosmos executable corrupted.");
            }

            ZipContent = new byte[ArchiveSize];

            Array.Copy(executableBytes, SignatureSize + ArchiveSizeLength, ZipContent, 0, ArchiveSize);

            ExtractLuaScripts();
        }

        private void ExtractLuaScripts()
        {
            bool mainFound = false;

            using (MemoryStream zipStream = new MemoryStream(ZipContent))
            {
                using (ZipStorer zip = ZipStorer.Open(zipStream, FileAccess.Read))
                {
                    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                    foreach (ZipStorer.ZipFileEntry entry in dir)
                    {
                        using (MemoryStream fileStream = new MemoryStream())
                        {
                            zip.ExtractFile(entry, fileStream);
                            byte[] script = fileStream.ToArray();
                            LuaSources.Add(entry.FilenameInZip, script);

                            if (entry.FilenameInZip == "main.lua")
                            {
                                mainFound = true;
                            }
                        }
                    }
                }
            }

            if (!mainFound)
            {
                throw new Exception("Could not find 'main.lua' in the executable.");
            }
        }
    }
}
