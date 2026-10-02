/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Executable class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Compression;

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
            // GEN3-GAP(null-deref): check the header before reading it (C6).
            if (executableBytes == null || executableBytes.Length < SignatureSize + ArchiveSizeLength)
            {
                throw new InvalidOperationException("This is not a Cosmos executable.");
            }

            Signature = Encoding.ASCII.GetString(executableBytes, 0, SignatureSize);

            if (Signature != ExpectedSignature)
            {
                throw new InvalidOperationException("This is not a Cosmos executable.");
            }

            ArchiveSize = BitConverter.ToInt32(executableBytes, SignatureSize);

            // GEN3-GAP(null-deref): a negative size makes gen3's new byte[] return null instead of throwing,
            // and a huge one overflowed the gen2 sum; compare without int overflow.
            if (ArchiveSize < 0 || ArchiveSize > executableBytes.Length - SignatureSize - ArchiveSizeLength)
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

            // GEN3-GAP(finally): no using blocks, gen3 does not run finally/Dispose when an exception unwinds (C7),
            // so a corrupted archive is caught here, the ZipStorer closed explicitly, then rethrown.
            MemoryStream zipStream = new MemoryStream(ZipContent);
            ZipStorer zip = null;
            Exception error = null;

            try
            {
                zip = ZipStorer.Open(zipStream, FileAccess.Read);

                List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                foreach (ZipStorer.ZipFileEntry entry in dir)
                {
                    MemoryStream fileStream = new MemoryStream();
                    zip.ExtractFile(entry, fileStream);
                    byte[] script = fileStream.ToArray();
                    fileStream.Dispose();

                    LuaSources.Add(entry.FilenameInZip, script);

                    if (entry.FilenameInZip == "main.lua")
                    {
                        mainFound = true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (zip != null)
            {
                zip.Close();
            }

            zipStream.Dispose();

            if (error != null)
            {
                throw error;
            }

            if (!mainFound)
            {
                throw new Exception("Could not find 'main.lua' in the executable.");
            }
        }
    }
}
