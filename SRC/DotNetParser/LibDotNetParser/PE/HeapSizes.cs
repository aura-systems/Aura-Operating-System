﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.PE
{
    /// <summary>
    /// based off of https://github.com/secana/PeNet/blob/master/src/PeNet/Header/Net/HeapSizes.cs
    /// </summary>
    public class HeapSizes
    {
        ///
        /// Size of the offsets into the "String" heap.
        /// </summary>
        public uint String { get; }

        /// <summary>
        /// Size of the offset into the "Guid" heap.
        /// </summary>
        public uint Guid { get; }

        /// <summary>
        /// Size of the offset into the "Blob" heap.
        /// </summary>
        public uint Blob { get; }

        /// <summary>
        /// Gets a value indicating whether the tables stream header contains an additional 32-bits after the table
        /// row counts.
        /// </summary>
        /// <remarks>
        /// This is an undocumented feature of the CLR.
        /// See also: https://github.com/dotnet/runtime/blob/ce2165d8084cca98b95f5d8ff9386759bfd8c722/src/coreclr/md/runtime/metamodel.cpp#L290
        /// </remarks>
        public bool HasExtraData { get; }

        /// <summary>
        /// Create a new HeapSizes instances.
        /// </summary>
        /// <param name="heapSizes">HeapSizes value from the MetaDataTablesHdr.</param>
        public HeapSizes(byte heapSizes)
        {
            String = (heapSizes & 0x1) == 0 ? 2U : 4U;
            Guid = (heapSizes & 0x2) == 0 ? 2U : 4U;
            Blob = (heapSizes & 0x4) == 0 ? 2U : 4U;
            HasExtraData = (heapSizes & 0x40) == 0;
        }
    }
}
