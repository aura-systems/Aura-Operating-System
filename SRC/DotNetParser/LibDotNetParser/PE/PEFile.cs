using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LibDotNetParser.DotNet.Streams;
using LibDotNetParser.DotNet.Tabels;

namespace LibDotNetParser
{
    public class PEFile
    {
        #region PE
        public DOSHeader DosHeader { get; private set; }
        public PEHeader PeHeader { get; private set; }
        #endregion
        #region CLR
        public CLRHeader ClrHeader { get; private set; }
        public MetadataHeader ClrMetaDataHeader { get; private set; }
        public MetadataStreamHeader ClrMetaDataStreamHeader { get; private set; }

        public StringsStream ClrStringsStream { get; private set; }
        public USStream ClrUsStream { get; private set; }

        public MetadataReader MetadataReader { get; private set; }
        public Tabels Tabels { get; private set; }
        public byte[] ClrStrongNameHash { get; private set; }

        List<StreamHeader> Streams = new List<StreamHeader>();
        #endregion
        public BinaryReader RawFile;

        public bool ContainsMetadata { get; private set; } = true;
        public byte[] BlobStream { get; private set; }
        public PEFile(string FilePath)
        {
            byte[] fs = File.ReadAllBytes(FilePath);
            Init(fs);
        }

        public PEFile(byte[] file)
        {
            Init(file);
        }
        private void Init(byte[] data)
        {
            #region Parse PE Header
            RawFile = new BinaryReader(new MemoryStream(data));
            BinaryReader r = new BinaryReader(new MemoryStream(data));

            DosHeader = ReadDOSHeader(r);
            PeHeader = ReadPEHeader(DosHeader.COFFHeaderAddress, r);

            //Read all of the data
            PeHeader.Directories = ReadDirectoriesList(PeHeader.DirectoryLength, r);
            PeHeader.Sections = ReadSectionsList(PeHeader.NumberOfSections, r);
            #endregion
            #region Parse Clr header & Strong name hash
            try
            {
                ClrHeader = ReadCLRHeader(r, PeHeader);
            }
            catch
            {
                Console.WriteLine("No clr header!");
                ContainsMetadata = false;
                return;
            }

            //Read the strong name hash
            ClrStrongNameHash = ReadStrongNameHash(r, ClrHeader.StrongNameSignatureAddress, ClrHeader.StrongNameSignatureSize, PeHeader.Sections);
            #endregion
            #region Parse Metadata header

            //Skip past all of the IL Code, and get tto the metadata header
            long pos = (long)BinUtil.RVAToOffset(ClrHeader.MetaDataDirectoryAddress, PeHeader.Sections);
            r.BaseStream.Position = pos;


            ClrMetaDataHeader = new MetadataHeader();
            ClrMetaDataHeader.Signature = r.ReadUInt32();
            ClrMetaDataHeader.MajorVersion = r.ReadUInt16();
            ClrMetaDataHeader.MinorVersion = r.ReadUInt16();
            ClrMetaDataHeader.Reserved1 = r.ReadUInt32();
            ClrMetaDataHeader.VersionStringLength = r.ReadUInt32();
            ClrMetaDataHeader.VersionString = r.ReadNullTermString((int)ClrMetaDataHeader.VersionStringLength);
            ClrMetaDataHeader.Flags = r.ReadUInt16(); //reserved
            ClrMetaDataHeader.NumberOfStreams = r.ReadUInt16();

            //Simple checks
            //Debug.Assert(ClrMetaDataHeader.Signature == 0x424A5342);
            //Debug.Assert(ClrMetaDataHeader.Reserved1 == 0);
            //Debug.Assert(ClrMetaDataHeader.Flags == 0);
            #endregion
            #region Parse Streams

            //Parse the StreamHeader(s)
            for (int i = 0; i < ClrMetaDataHeader.NumberOfStreams; i++)
            {
                var hdr = new StreamHeader();

                hdr.Offset = r.ReadUInt32();
                hdr.Size = r.ReadUInt32();
                hdr.Name = r.ReadNullTermFourByteAlignedString();

                Streams.Add(hdr);
            }

            //Parse the #String stream
            var bytes = GetStreamBytes("#Strings", r);
            ClrStringsStream = new StringsStream(bytes);

            //Parse the #US Stream
            var bytes2 = GetStreamBytes("#US", r);

            //some assemblys may not have user strings
            if (bytes2 != null)
            {
                ClrUsStream = new USStreamReader(bytes2).Read();
            }


            #endregion
            #region Parse #~ Stream aka Tabels stream
            //Parse the #~ stream
            BinaryReader TableStreamR = new BinaryReader(new MemoryStream(GetStreamBytes("#~", r)));
            ClrMetaDataStreamHeader = ReadHeader(TableStreamR);

            //Parse the tabels data
            var numberOfTables = GetTableCount(ClrMetaDataStreamHeader.TablesFlags);
            ClrMetaDataStreamHeader.TableSizes = new uint[numberOfTables];

            for (var i = 0; i < numberOfTables; i++)
            {
                ClrMetaDataStreamHeader.TableSizes[i] = TableStreamR.ReadUInt32();
            }

            MetadataReader = new MetadataReader(TableStreamR.BaseStream, new HeapSizes((byte)ClrMetaDataStreamHeader.OffsetSizeFlags));
            BlobStream = GetStreamBytes("#Blob", r);

            //Parse the tabels
            Tabels = new Tabels(this);
            #endregion
        }

        #region Read Windows Header
        public DOSHeader ReadDOSHeader(BinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            return new DOSHeader
            {
                Magic = reader.ReadUInt16(),
                BytesOnLastPage = reader.ReadUInt16(),
                PagesInFile = reader.ReadUInt16(),
                Relocations = reader.ReadUInt16(),
                SizeOfHeader = reader.ReadUInt16(),
                MinExtraParagraphs = reader.ReadUInt16(),
                MaxExtraParagraphs = reader.ReadUInt16(),
                InitialSS = reader.ReadUInt16(),
                InitialSP = reader.ReadUInt16(),
                Checksum = reader.ReadUInt16(),
                InitialIP = reader.ReadUInt16(),
                InitialCS = reader.ReadUInt16(),
                RelocTableAddress = reader.ReadUInt16(),
                OverlayNumber = reader.ReadUInt16(),
                Unknown01 = reader.ReadUInt16(),
                Unknown02 = reader.ReadUInt16(),
                Unknown03 = reader.ReadUInt16(),
                Unknown04 = reader.ReadUInt16(),
                OEMIdentifier = reader.ReadUInt16(),
                OEMInfo = reader.ReadUInt16(),
                Unknown05 = reader.ReadUInt16(),
                Unknown06 = reader.ReadUInt16(),
                Unknown07 = reader.ReadUInt16(),
                Unknown08 = reader.ReadUInt16(),
                Unknown09 = reader.ReadUInt16(),
                Unknown10 = reader.ReadUInt16(),
                Unknown11 = reader.ReadUInt16(),
                Unknown12 = reader.ReadUInt16(),
                Unknown13 = reader.ReadUInt16(),
                Unknown14 = reader.ReadUInt16(),
                COFFHeaderAddress = reader.ReadUInt16(),
            };
        }
        public PEHeader ReadPEHeader(ushort headerAddress, BinaryReader _assemblyReader)
        {
            _assemblyReader.BaseStream.Seek(headerAddress, SeekOrigin.Begin);
            var header = new PEHeader();

            header.Signature = _assemblyReader.ReadUInt32();
            if (header.Signature != 17744)
                throw new Exception("Invaild PE Header signature");
            header.Machine = _assemblyReader.ReadUInt16();
            header.NumberOfSections = _assemblyReader.ReadUInt16();
            header.DateTimeStamp = _assemblyReader.ReadUInt32();
            header.PtrToSymbolTable = _assemblyReader.ReadUInt32();
            header.NumberOfSymbols = _assemblyReader.ReadUInt32();
            header.SizeOfOptionalHeaders = _assemblyReader.ReadUInt16();
            header.Characteristics = _assemblyReader.ReadUInt16();
            header.OptionalMagic = _assemblyReader.ReadUInt16();
            if (header.OptionalMagic != 0x10b && header.OptionalMagic != 0x20b)
                throw new Exception("Invaild PE Optional header magic");

            header.MajorLinkerVersion = _assemblyReader.ReadByte();
            header.MinorLinkerVersion = _assemblyReader.ReadByte();
            header.SizeOfCode = _assemblyReader.ReadUInt32();
            header.SizeOfInitData = _assemblyReader.ReadUInt32();
            header.SizeOfUninitData = _assemblyReader.ReadUInt32();
            header.AddressOfEntryPoint = _assemblyReader.ReadUInt32();
            header.BaseOfCode = _assemblyReader.ReadUInt32();

            var isPE32Plus = header.OptionalMagic == 0x20b;

            //PE32+ does not have this field
            if (!isPE32Plus)
                header.BaseOfData = _assemblyReader.ReadUInt32();

            //PE32: size 4, PE32+: size 8
            if (!isPE32Plus)
                header.ImageBase = _assemblyReader.ReadUInt32();
            else
                header.ImageBase = _assemblyReader.ReadUInt64();
            header.SectionAlignment = _assemblyReader.ReadUInt32();
            header.FileAlignment = _assemblyReader.ReadUInt32();
            header.MajorOSVersion = _assemblyReader.ReadUInt16();
            header.MinorOSVersion = _assemblyReader.ReadUInt16();
            header.MajorImageVersion = _assemblyReader.ReadUInt16();
            header.MinorImageVersion = _assemblyReader.ReadUInt16();
            header.MajorSubsystemVersion = _assemblyReader.ReadUInt16();
            header.MinorSubsystemVersion = _assemblyReader.ReadUInt16();
            header.Reserved1 = _assemblyReader.ReadUInt32();
            header.SizeOfImage = _assemblyReader.ReadUInt32();
            header.SizeOfHeaders = _assemblyReader.ReadUInt32();
            header.PEChecksum = _assemblyReader.ReadUInt32();
            header.Subsystem = _assemblyReader.ReadUInt16();
            header.DLLCharacteristics = _assemblyReader.ReadUInt16();

            if (isPE32Plus)
                header.SizeOfStackReserve = _assemblyReader.ReadUInt64();
            else
                header.SizeOfStackReserve = _assemblyReader.ReadUInt32();

            if (isPE32Plus)
                header.SizeOfStackCommit = _assemblyReader.ReadUInt64();
            else
                header.SizeOfStackCommit = _assemblyReader.ReadUInt32();

            if (isPE32Plus)
                header.SizeOfHeapReserve = _assemblyReader.ReadUInt64();
            else
                header.SizeOfHeapReserve = _assemblyReader.ReadUInt32();

            if (isPE32Plus)
                header.SizeOfHeapCommit = _assemblyReader.ReadUInt64();
            else
                header.SizeOfHeapCommit = _assemblyReader.ReadUInt32();

            header.LoaderFlags = _assemblyReader.ReadUInt32();
            header.DirectoryLength = _assemblyReader.ReadUInt32();

            return header;
        }
        #endregion
        #region Read virtual directory
        private IList<DataDirectory> ReadDirectoriesList(uint directoryCount, BinaryReader _assemblyReader)
        {
            var result = new List<DataDirectory>((int)directoryCount);
            for (var i = 0; i < directoryCount; i++)
            {
                result.Add(new DataDirectory
                {
                    Address = _assemblyReader.ReadUInt32(),
                    Size = _assemblyReader.ReadUInt32()
                });
            }
            return result;
        }
        public byte[] ReadVirtualDirectory(BinaryReader reader, DataDirectory dataDirectory, IList<Section> sections)
        {
            // find the section whose virtual address range contains the data directory's virtual address.
            var section = sections[0];

            // calculate the offset into the file.
            var fileOffset = section.PointerToRawData + (dataDirectory.Address - section.VirtualAddress);

            // read the virtual directory data.
            reader.BaseStream.Seek((long)fileOffset, SeekOrigin.Begin);
            return reader.ReadBytes((int)dataDirectory.Size);
        }
        #endregion
        #region Read the sections
        private List<Section> ReadSectionsList(int numberOfSections, BinaryReader _assemblyReader)
        {
            var result = new List<Section>();
            for (var i = 0; i < numberOfSections; i++)
            {
                var s = new Section();

                s.Name = _assemblyReader.ReadNullTermString(8);
                s.VirtualSize = _assemblyReader.ReadUInt32();
                s.VirtualAddress = _assemblyReader.ReadUInt32();
                s.SizeOfRawData = _assemblyReader.ReadUInt32();
                s.PointerToRawData = _assemblyReader.ReadUInt32();
                s.PointerToRelocations = _assemblyReader.ReadUInt32();
                s.PointerToLinenumbers = _assemblyReader.ReadUInt32();
                s.NumberOfRelocations = _assemblyReader.ReadUInt16();
                s.NumberOfLinenumbers = _assemblyReader.ReadUInt16();
                s.Characteristics = _assemblyReader.ReadUInt32();

                result.Add(s);
            }
            return result;
        }
        public byte[] ReadSection(BinaryReader reader, Section section)
        {
            reader.BaseStream.Seek((long)section.PointerToRawData, SeekOrigin.Begin);
            return reader.ReadBytes((int)section.SizeOfRawData);
        }
        #endregion
        #region Read CLR Headers
        private CLRHeader ReadCLRHeader(BinaryReader assemblyReader, PEHeader peHeader)
        {
            if (peHeader.Directories.Count == 0)
                throw new InvalidDataException("PE Header has failed to parse");
            var clrDirectoryHeader = peHeader.Directories[(int)DataDirectoryName.CLRHeader];
            var clrDirectoryData = ReadVirtualDirectory(assemblyReader, clrDirectoryHeader, peHeader.Sections);
            using (var reader = new BinaryReader(new MemoryStream(clrDirectoryData)))
            {
                var a = new CLRHeader
                {
                    HeaderSize = reader.ReadUInt32(),
                    MajorRuntimeVersion = reader.ReadUInt16(),
                    MinorRuntimeVersion = reader.ReadUInt16(),
                    MetaDataDirectoryAddress = reader.ReadUInt32(),
                    MetaDataDirectorySize = reader.ReadUInt32(),
                    Flags = reader.ReadUInt32(),
                    EntryPointToken = reader.ReadUInt32(),
                    ResourcesDirectoryAddress = reader.ReadUInt32(),
                    ResourcesDirectorySize = reader.ReadUInt32(),
                    StrongNameSignatureAddress = reader.ReadUInt32(),
                    StrongNameSignatureSize = reader.ReadUInt32(),
                    CodeManagerTableAddress = reader.ReadUInt32(),
                    CodeManagerTableSize = reader.ReadUInt32(),
                    VTableFixupsAddress = reader.ReadUInt32(),
                    VTableFixupsSize = reader.ReadUInt32(),
                    ExportAddressTableJumpsAddress = reader.ReadUInt32(),
                    ExportAddressTableJumpsSize = reader.ReadUInt32(),
                    ManagedNativeHeaderAddress = reader.ReadUInt32(),
                    ManagedNativeHeaderSize = reader.ReadUInt32()
                };
                return a;
            }
        }
        private MetadataStreamHeader ReadHeader(BinaryReader _reader)
        {
            return new MetadataStreamHeader
            {
                Reserved1 = _reader.ReadUInt32(),
                MajorVersion = _reader.ReadByte(),
                MinorVersion = _reader.ReadByte(),
                OffsetSizeFlags = (StreamOffsetSizeFlags)_reader.ReadByte(),
                Reserved2 = _reader.ReadByte(),
                TablesFlags = (MetadataTableFlags)_reader.ReadUInt64(),
                SortedTablesFlags = (MetadataTableFlags)_reader.ReadUInt64(),
            };
        }
        private static byte[] ReadStrongNameHash(BinaryReader reader, uint rva, uint size, IEnumerable<Section> sections)
        {
            if (rva == 0)
                return new byte[0];

            var fileOffset = BinUtil.RVAToOffset(rva, sections);
            reader.BaseStream.Seek((long)fileOffset, SeekOrigin.Begin);
            return reader.ReadBytes((int)size);
        }
        private int GetTableCount(MetadataTableFlags tablesFlags)
        {
            var count = 0;
            while (tablesFlags != 0)
            {
                tablesFlags &= (tablesFlags - 1);
                count++;
            }
            return count;
        }
        public byte[] GetStreamBytes(BinaryReader reader, StreamHeader streamHeader, uint metadataDirectoryAddress, IEnumerable<Section> sections)
        {
            var rva = metadataDirectoryAddress + streamHeader.Offset;
            var fileOffset = BinUtil.RVAToOffset(rva, sections);
            reader.BaseStream.Seek((long)fileOffset, SeekOrigin.Begin);
            return reader.ReadBytes((int)streamHeader.Size);
        }
        #endregion
        #region Utils
        public byte[] GetStreamBytes(string streamName, BinaryReader r)
        {
            StreamHeader hdr = null;
            foreach (var item in Streams)
            {
                if (item.Name == streamName)
                {
                    hdr = item;
                    break;
                }
            }
            if (hdr == null)
                return null;

            return GetStreamBytes(r, hdr, ClrHeader.MetaDataDirectoryAddress, PeHeader.Sections);
        }
        #endregion
    }
}
