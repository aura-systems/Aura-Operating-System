using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleFileSystem.Structs;

namespace SimpleFileSystem
{
    public class SimpleFS
    {
        private List<Structure> IndexAria { get; set; } = new List<Structure>();

        #region Props

        public SimpleFS(IBlockDevice blockDevice)
        {
            BlockDevice = blockDevice;
            BlockBuffer = new BlockBuffer(BlockDevice);
        }

        public IBlockDevice BlockDevice { get; set; }
        public BlockBuffer BlockBuffer { get; set; }

        public SuperBlock SuperBlock { get; set; }

        #endregion

        #region Api

        public void Format()
        {
            SuperBlock = new SuperBlock
            {
                BlockSize = 2, //this should be calculated
                TotalReservedBlocks = 20,
                TotalBlocks = BlockDevice.TotalBlocks,
                SizeOfIndexInBytes = 0,
                Checksum = 0,
                DataSizeInBlocks = 0,
                TimeStamp = 0
            };

            SuperBlock.SizeOfIndexInBytes = 64 * 2;
            SuperBlock.Write(BlockBuffer);


            var volumeIdentifier = new VolumeIdentifier
            {
                TimeStamp = 0,
                VolumeName = "SFS Volume"
            };

            var offset = 1;

            BlockBuffer.Offset = BlockDevice.BlockSize * BlockDevice.TotalBlocks - offset * 64;

            volumeIdentifier.Write(BlockBuffer);
            offset++;

            BlockBuffer.Offset = BlockDevice.BlockSize * BlockDevice.TotalBlocks - offset * 64;

            var marker = new StartingMarkerEntry();
            marker.Write(BlockBuffer);

            ReadIndexAria();
        }

        public void Load()
        {
            SuperBlock = new SuperBlock();
            SuperBlock.Read(BlockBuffer);

            ReadIndexAria();
        }

        public void Save()
        {
            SuperBlock.SizeOfIndexInBytes = 64 * IndexAria.Count;
            SuperBlock.Write(BlockBuffer);

            WriteIndexAria();
        }

        public bool CreateDirectory(string name)
        {
            if (name.Length > 16320) return false;

            //dir names must be guid
            foreach (var entry in IndexAria)
            {
                if (entry is DirectoryEntry de)
                {
                    if (de.Name == name)
                    {
                        return false;
                    }
                }
            }
            
            //add it
            IndexAria.Insert(IndexAria.Count - 1, new DirectoryEntry
            {
                TimeStamp = 0,
                Name = name
            });
            Save();

            return true;
        }

        public bool DeleteDirectory(string name)
        {
            //dir names must be guid
            foreach (var entry in IndexAria)
            {
                if (entry is DirectoryEntry de)
                {
                    if (de.Name == name)
                    {
                        IndexAria.Remove(de);
                        Save();
                        return true;
                    }
                }
            }


            return false;
        }

        public List<string> GetFiles(string directory)
        {
            var re = new List<string>();
            foreach (var structure in IndexAria)
                if (structure is FileEntry fe)
                    if (fe.Name.StartsWith(directory.TrimEnd('\\')))
                        re.Add(fe.Name);

            return re;
        }

        public List<string> GetAllFiles()
        {
            var re = new List<string>();

            foreach (var structure in IndexAria)
                if (structure is FileEntry fe)
                    re.Add(fe.Name);

            return re;
        }

        public List<string> GetAllDirectories()
        {
            var re = new List<string>();

            foreach (var structure in IndexAria)
                if (structure is DirectoryEntry de)
                    re.Add(de.Name);

            return re;
        }

        public string ReadAllText(string filePath) => Encoding.ASCII.GetString(ReadAllBytes(filePath));

        public byte[] ReadAllBytes(string filePath)
        {
            FileEntry file = null;

            foreach (var entry in IndexAria)
            {
                if (entry is FileEntry fe)
                {
                    if (fe.Name == filePath)
                    {
                        file = fe;
                    }
                }
            }

            if (file == null) return null;

            var buf = new List<byte>();

            BlockBuffer.Offset = file.StartingBlock * BlockDevice.BlockSize;


            for (int i = 0; i < file.Length; i++)
            {
                buf.Add(BlockBuffer.ReadByte());
            }

            return buf.ToArray();
        }

        public bool WriteAllText(string filePath, string text) =>
            WriteAllBytes(filePath, Encoding.ASCII.GetBytes(text));

        public bool WriteAllBytes(string filePath, byte[] data)
        {
            if (filePath.Length > 16320) return false;

            FileEntry fileEntry = null;

            //file names must be guid
            foreach (var entry in IndexAria)
            {
                if (entry is FileEntry fe)
                {
                    if (fe.Name == filePath)
                    {
                        fileEntry = fe;
                        break;
                    }
                }
            }

            if (fileEntry == null)
            {
                fileEntry = new FileEntry
                {
                    StartingBlock = SuperBlock.DataSizeInBlocks + SuperBlock.TotalReservedBlocks,
                    EndingBlock = (SuperBlock.DataSizeInBlocks + SuperBlock.TotalReservedBlocks) +
                                  (data.Length / BlockDevice.BlockSize) + 1,
                    Length = data.Length,
                    Name = filePath,
                    TimeStamp = 0
                };

                SuperBlock.DataSizeInBlocks += fileEntry.EndingBlock - fileEntry.StartingBlock;

                //add it
                IndexAria.Insert(IndexAria.Count - 1, fileEntry);
            }

            //write data
            var bl = fileEntry.EndingBlock - fileEntry.StartingBlock;

            var buf = new byte[BlockDevice.BlockSize];

            long c = 0;
            for (int i = 0; i < bl - 1; i++)
            {
                Array.Copy(data, i * BlockDevice.BlockSize, buf, 0, BlockDevice.BlockSize);
                c += BlockDevice.BlockSize;
                BlockDevice.WriteBlock(fileEntry.StartingBlock + i, buf);
            }


            Array.Copy(data, c, buf, 0, data.Length - c);
            BlockDevice.WriteBlock(fileEntry.StartingBlock + (bl - 1), buf);

            Save();
            return true;
        }

        public bool FileExists(string filePath)
        {
            foreach (var entry in IndexAria)
                if (entry is FileEntry fe)
                    if (fe.Name == filePath)
                        return true;
            return false;
        }

        public bool DeleteFile(string filePath)
        {
            foreach (var entry in IndexAria)
                if (entry is FileEntry fe)
                    if (fe.Name == filePath)
                    {
                        IndexAria.Remove(fe);
                        Save();
                        return true;
                    }

            return false;
        }

        #endregion

        #region Private

        private void ReadIndexAria()
        {
            IndexAria.Clear();

            var endOfVolume = BlockDevice.BlockSize * BlockDevice.TotalBlocks;
            var offset = 1;

            while (endOfVolume > offset * 64)
            {
                BlockBuffer.Offset = endOfVolume - offset * 64;

                var id = BlockBuffer.ReadByte();
                BlockBuffer.Offset--;

                switch (id)
                {
                    case 0x01: // VolumeIdentifier
                    {
                        var x = new VolumeIdentifier();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);
                    }
                        break;
                    case 0x02: // Starting Marker Entry
                    {
                        var x = new StartingMarkerEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);
                        return;
                    }
                    case 0x10: // Unused Entry
                    {
                        var x = new UnusedEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);
                    }
                        break;
                    case 0x11: // Directory Entry 
                    {
                        var x = new DirectoryEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);
                        if (x.Continuations > 0)
                        {
                            offset += x.Continuations + 1;
                            BlockBuffer.Offset = endOfVolume - offset * 64;
                            x.Name = BlockBuffer.ReadString();
                        }
                    }
                        break;
                    case 0x12: // File Entry
                    {
                        var x = new FileEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);

                        if (x.Continuations > 0)
                        {
                            offset += x.Continuations + 1;
                            BlockBuffer.Offset = endOfVolume - offset * 64;
                            x.Name = BlockBuffer.ReadString();
                        }
                    }
                        break;
                    case 0x18: // Unusable Entry
                    {
                        var x = new UnusableEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);
                    }
                        break;
                    case 0x19: // Deleted Directory Entry
                    {
                        var x = new DeletedDirectoryEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);

                        if (x.Continuations > 0)
                        {
                            offset += x.Continuations + 1;
                            BlockBuffer.Offset = endOfVolume - offset * 64;
                            x.Name = BlockBuffer.ReadString();
                        }
                    }
                        break;
                    case 0x1A: // Deleted File Entry
                    {
                        var x = new DeletedFileEntry();
                        x.Read(BlockBuffer);
                        IndexAria.Add(x);

                        if (x.Continuations > 0)
                        {
                            offset += x.Continuations + 1;
                            BlockBuffer.Offset = endOfVolume - offset * 64;
                            x.Name = BlockBuffer.ReadString();
                        }
                    }
                        break;
                }

                offset++;
            }
        }

        private void WriteIndexAria()
        {
            var endOfVolume = BlockDevice.BlockSize * BlockDevice.TotalBlocks;
            var offset = 1;

            foreach (var structure in IndexAria)
            {
                BlockBuffer.Offset = endOfVolume - offset * 64;

                structure.Write(BlockBuffer);

                if (structure.Continuations > 0)
                {
                    offset += (structure.Name.Length / 64) + 1;
                    BlockBuffer.Offset = endOfVolume - offset * 64;
                    BlockBuffer.WriteString(structure.Name);
                }

                offset++;
            }
        }

        #endregion
    }
}