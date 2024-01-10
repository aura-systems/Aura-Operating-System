using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoomSharp.Core.Data;

public class WadFile : IDisposable
{
    public struct WadInfo
    {
        // Should be "IWAD" or "PWAD".
        public string Identification;
        public int NumLumps;
        public int InfoTableOfs;
    }

    public struct FileLump
    {
        public int FilePos;
        public int Size;
        public string Name;

        public static FileLump ReadFromWadData(BinaryReader reader)
        {
            return new FileLump
            {
                FilePos = reader.ReadInt32(),
                Size = reader.ReadInt32(),
                Name = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0')
            };
        }
    }

    public static WadFile? LoadFromFile(string file)
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
        var header = new WadInfo
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
            var lump = FileLump.ReadFromWadData(reader);
            fileInfo.Add(new WadLump(wadFile, lump));
        }

        wadFile.Lumps = fileInfo;

        return wadFile;
    }

    private readonly BinaryReader _reader;

    public WadFile(BinaryReader reader)
    {
        _reader = reader;
    }

    public WadInfo Header { get; init; }
    public ICollection<WadLump> Lumps { get; set; } = Array.Empty<WadLump>();
    public int LumpCount => Header.NumLumps;

    public void Dispose()
    {
        _reader.Dispose();
    }

    public void ReadLumpData(WadLump destination)
    {
        _reader.BaseStream.Seek(destination.Lump.FilePos, SeekOrigin.Begin);
        destination.Data = _reader.ReadBytes(destination.Lump.Size);
    }
}