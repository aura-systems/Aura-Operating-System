using DoomSharp.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace DoomSharp.Core.Data;

public class WadFileCollection : List<WadLump>, IDisposable
{
    private readonly List<WadFile> _wadFiles = new();
    private static IWadStreamProvider _wadStreamProvider;

    public static void Init(IWadStreamProvider wadStreamProvider)
    {
        _wadStreamProvider = wadStreamProvider;
    }

    /// <summary>
    /// Pass a null terminated list of files to use.
    /// All files are optional, but at least one file
    ///  must be found.
    /// Files with a .wad extension are idlink files
    ///  with multiple lumps.
    /// Other files are single lumps with the base filename
    ///  for the lump name.
    /// Lump names can appear multiple times.
    /// The name searcher looks backwards, so a later file
    ///  does override all earlier ones.
    /// </summary>
    /// <param name="files"></param>
    public static WadFileCollection InitializeMultipleFiles(IEnumerable<string> files)
    {
        if (_wadStreamProvider == null)
        {
            throw new InvalidOperationException("WadStreamProvider is not initialized");
        } 
        
        var collection = new WadFileCollection();

        foreach (var file in files)
        {
            var wadFile = _wadStreamProvider.LoadFromFile(file);
            if (wadFile != null)
            {
                collection._wadFiles.Add(wadFile);
                collection.AddRange(wadFile.Lumps);
            }
        }

        if (collection.LumpCount < 1)
        {
            throw new Exception("W_InitFiles: no files found");
        }

        return collection;
    }

    public byte[]? GetLumpName(string name, PurgeTag tag)
    {
        return GetLumpNum(GetNumForName(name), tag);
    }

    public byte[]? GetLumpNum(int lump, PurgeTag tag)
    {
        if (lump < 0)
        {
            return null;
        }

        if (lump >= LumpCount)
        {
            DoomGame.Error($"W_CacheLumpNum: {lump} >= numlumps");
        }

        if (this[lump].Data == null)
        {
            // read the lump in
            ReadLump(lump, this[lump]);
            ChangeTag(this[lump], tag);
        }
        else
        {
            ChangeTag(this[lump], tag);
        }

        return this[lump].Data!;
    }

    public int GetNumForName(string name)
    {
        var num = CheckNumForName(name);
        if (num == -1)
        {
            DoomGame.Console.WriteLine($"W_GetNumForName: {name} not found!");
        }

        return num;
    }

    public int CheckNumForName(string name)
    {
        // scan backwards so patch lump files take precedence
        var i = LumpCount;
        while (i-- > 0)
        {
            if (string.Equals(this[i].Lump.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        
        // TFB. Not found.
        return -1;
    }

    public int LumpLength(int lump)
    {
        if (lump >= LumpCount)
        {
            DoomGame.Error($"W_LumpLength: {lump} >= numlumps");
            return -1;
        }

        return this[lump].Lump.Size;
    }

    public void ReadLump(int lump, WadLump destination)
    {
        if (lump < 0 || lump >= LumpCount)
        {
            DoomGame.Error($"W_ReadLump: {lump} >= numlumps (or < 0)");
        }

        destination.File.ReadLumpData(destination);
    }

    private void ChangeTag(WadLump lump, PurgeTag tag)
    {
        lump.Tag = tag;
    }

    public int LumpCount => Count;

    public void Dispose()
    {
        foreach (var item in _wadFiles)
        {
            item.Dispose();
        }
    }
}