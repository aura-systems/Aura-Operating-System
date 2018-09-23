using Cosmos.HAL.BlockDevice;
using SimpleFileSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.CosmosSFS
{
    public class CosmosBlockDevice : IBlockDevice
    {
        public CosmosBlockDevice(Partition partition)
        {
            Partition = partition;
        }

        public Partition Partition { get; set; }

        public long BlockSize => (long)Partition.BlockSize;
        public long TotalBlocks => (long)Partition.BlockCount;

        public byte[] ReadBlock(long offset)
        {
            var buf = new byte[BlockSize];
            Partition.ReadBlock((ulong)offset, 1, buf);
            return buf;
        }

        public void WriteBlock(long offset, byte[] block)
        {
            Partition.WriteBlock((ulong)offset, 1, block);
        }
    }
}