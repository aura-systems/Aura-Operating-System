using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DoomSharp.Core;
using DoomSharp.Core.Abstractions;
using DoomSharp.Core.Data;

namespace DoomSharp.Windows.Data
{
    internal sealed class WadStreamProvider : IWadStreamProvider
    {
        public WadFile LoadFromFile(string file)
        {
            var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
            var br = new BinaryReader(fs, Encoding.ASCII, false);

            DoomGame.Console.WriteLine($" adding {file}");

            if (string.Equals(Path.GetExtension(file), ".wad", StringComparison.OrdinalIgnoreCase))
            {
                // WAD file
                return LoadWad(file, br);
            }
            else
            {
                // Single lump file
                // TODO
            }

            return null;
        }

        private static WadFile? LoadWad(string file, BinaryReader reader)
        {
            var header = new WadFile.WadInfo
            {
                Identification = Encoding.ASCII.GetString(reader.ReadBytes(4)).TrimEnd('\0'),
                NumLumps = reader.ReadInt32(),
                InfoTableOfs = reader.ReadInt32()
            };

            var wadFile = new WadFile(reader)
            {
                Header = header
            };

            if (string.Equals(wadFile.Header.Identification, "IWAD", StringComparison.Ordinal) == false)
            {
                if (string.Equals(wadFile.Header.Identification, "PWAD", StringComparison.Ordinal) == false)
                {
                    DoomGame.Error($"Wad file {file} doesn't have IWAD or PWAD id");
                    return null;
                }
            }

            var fileInfo = new List<WadLump>(wadFile.LumpCount);

            reader.BaseStream.Seek(wadFile.Header.InfoTableOfs, SeekOrigin.Begin);
            for (var i = 0; i < wadFile.LumpCount; i++)
            {
                var lump = WadFile.FileLump.ReadFromWadData(reader);
                fileInfo.Add(new WadLump(wadFile, lump));
            }

            wadFile.Lumps = fileInfo;

            return wadFile;
        }
    }
}
