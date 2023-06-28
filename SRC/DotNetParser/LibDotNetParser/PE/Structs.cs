using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDotNetParser.PE
{
    /// <summary>
    /// The DOS Header.
    /// </summary>
    public class DOSHeader
    {
        public ushort Magic { get; set; }
        public ushort BytesOnLastPage { get; set; }
        public ushort PagesInFile { get; set; }
        public ushort Relocations { get; set; }
        public ushort SizeOfHeader { get; set; }
        public ushort MinExtraParagraphs { get; set; }
        public ushort MaxExtraParagraphs { get; set; }
        public ushort InitialSS { get; set; }
        public ushort InitialSP { get; set; }
        public ushort Checksum { get; set; }
        public ushort InitialIP { get; set; }
        public ushort InitialCS { get; set; }
        public ushort RelocTableAddress { get; set; }
        public ushort OverlayNumber { get; set; }
        public ushort Unknown01 { get; set; }
        public ushort Unknown02 { get; set; }
        public ushort Unknown03 { get; set; }
        public ushort Unknown04 { get; set; }
        public ushort OEMIdentifier { get; set; }
        public ushort OEMInfo { get; set; }
        public ushort Unknown05 { get; set; }
        public ushort Unknown06 { get; set; }
        public ushort Unknown07 { get; set; }
        public ushort Unknown08 { get; set; }
        public ushort Unknown09 { get; set; }
        public ushort Unknown10 { get; set; }
        public ushort Unknown11 { get; set; }
        public ushort Unknown12 { get; set; }
        public ushort Unknown13 { get; set; }
        public ushort Unknown14 { get; set; }
        public ushort COFFHeaderAddress { get; set; }
    }
    /// <summary>
    /// The PE Header.
    /// </summary>
    public class PEHeader
    {
        public uint Signature { get; set; }
        public ushort Machine { get; set; }
        public ushort NumberOfSections { get; set; }
        public uint DateTimeStamp { get; set; }
        public uint PtrToSymbolTable { get; set; }
        public uint NumberOfSymbols { get; set; }
        public ushort SizeOfOptionalHeaders { get; set; }
        public ushort Characteristics { get; set; }
        public ushort OptionalMagic { get; set; }
        public byte MajorLinkerVersion { get; set; }
        public byte MinorLinkerVersion { get; set; }
        public uint SizeOfCode { get; set; }
        public uint SizeOfInitData { get; set; }
        public uint SizeOfUninitData { get; set; }
        public uint AddressOfEntryPoint { get; set; }
        public uint BaseOfCode { get; set; }
        public uint BaseOfData { get; set; }
        public ulong ImageBase { get; set; }
        public uint SectionAlignment { get; set; }
        public uint FileAlignment { get; set; }
        public ushort MajorOSVersion { get; set; }
        public ushort MinorOSVersion { get; set; }
        public ushort MajorImageVersion { get; set; }
        public ushort MinorImageVersion { get; set; }
        public ushort MajorSubsystemVersion { get; set; }
        public ushort MinorSubsystemVersion { get; set; }
        public uint Reserved1 { get; set; }
        public uint SizeOfImage { get; set; }
        public uint SizeOfHeaders { get; set; }
        public uint PEChecksum { get; set; }
        public ushort Subsystem { get; set; }
        public ushort DLLCharacteristics { get; set; }
        public ulong SizeOfStackReserve { get; set; }
        public ulong SizeOfStackCommit { get; set; }
        public ulong SizeOfHeapReserve { get; set; }
        public ulong SizeOfHeapCommit { get; set; }
        public uint LoaderFlags { get; set; }
        public uint DirectoryLength { get; set; }
        public IList<DataDirectory> Directories { get; set; }
        public IList<Section> Sections { get; set; }
    }
    /// <summary>
    /// CLR Header
    /// </summary>
    public class CLRHeader
    {

        public uint HeaderSize { get; set; }
        public ushort MajorRuntimeVersion { get; set; }
        public ushort MinorRuntimeVersion { get; set; }
        public uint MetaDataDirectoryAddress { get; set; }
        public uint MetaDataDirectorySize { get; set; }
        public uint Flags { get; set; }
        public uint EntryPointToken { get; set; }
        public uint ResourcesDirectoryAddress { get; set; }
        public uint ResourcesDirectorySize { get; set; }
        public uint StrongNameSignatureAddress { get; set; }
        public uint StrongNameSignatureSize { get; set; }
        public uint CodeManagerTableAddress { get; set; }
        public uint CodeManagerTableSize { get; set; }
        public uint VTableFixupsAddress { get; set; }
        public uint VTableFixupsSize { get; set; }
        public uint ExportAddressTableJumpsAddress { get; set; }
        public uint ExportAddressTableJumpsSize { get; set; }
        public uint ManagedNativeHeaderAddress { get; set; }
        public uint ManagedNativeHeaderSize { get; set; }
    }
    public class Section
    {
        public string Name { get; set; }
        public ulong VirtualSize { get; set; }
        public ulong VirtualAddress { get; set; }
        public ulong SizeOfRawData { get; set; }
        public ulong PointerToRawData { get; set; }
        public ulong PointerToRelocations { get; set; }
        public ulong PointerToLinenumbers { get; set; }
        public ushort NumberOfRelocations { get; set; }
        public ushort NumberOfLinenumbers { get; set; }
        public ulong Characteristics { get; set; }
    }
    public class DataDirectory
    {
        public ulong Address { get; set; }
        public ulong Size { get; set; }
    }
    public enum DataDirectoryName
    {
        Export,
        Import,
        Resource,
        Exception,
        Security,
        Relocation,
        Debug,
        Copyright,
        GlobalPtr,
        ThreadLocalStorage,
        LoadConfig,
        BoundImport,
        ImportAddressTable,
        DelayLoadImportAddressTable,
        CLRHeader,
        Reserved
    }
    /// <summary>
    /// .NET Meta data header
    /// </summary>
    public class MetadataHeader
    {
        public ulong Signature { get; set; } // always 0x424A5342 [42 53 4A 42]
        public uint MajorVersion { get; set; } // always 0x0001 [01 00]
        public uint MinorVersion { get; set; } // always 0x0001 [01 00]
        public ulong Reserved1 { get; set; } // always 0x00000000 [00 00 00 00]
        public ulong VersionStringLength { get; set; }
        public string VersionString { get; set; } // null terminated in file. VersionStringLength includes the null(s) in the length, and also is always rounded up to a multiple of 4.
        public ushort Flags { get; set; } // always 0x0000 [00 00]
        public ushort NumberOfStreams { get; set; }
    }
    /// <summary>
    /// .NET Stream Header
    /// </summary>
    public class StreamHeader
    {
        public uint Offset { get; set; } // relative to start of MetadataHeader (Same as CLRHeader.MetaDataDirectoryAddress, resolved to file offset, then add this stream Offset.)
        public uint Size { get; set; }
        public string Name { get; set; } // null terminated in file, length always rounded up to divisible by 4
    }
    /// <summary>
    /// Header for the #~ stream.
    /// </summary>
    public class MetadataStreamHeader
    {
        public uint Reserved1 { get; set; }
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public StreamOffsetSizeFlags OffsetSizeFlags { get; set; } // indicates offset sizes to be used within the other streams.
        public byte Reserved2 { get; set; } // Always set to 0x01 [01]
        public MetadataTableFlags TablesFlags { get; set; } // indicated which tables are present. 8 bytes.
        public MetadataTableFlags SortedTablesFlags { get; set; } // indicated which tables are sorted. 8 bytes.
        public uint[] TableSizes { get; set; } // Size of each table. Array count will be same as # of '1's in TableFlags.
    }
    /// <summary>
    /// If the flag is not set, the offsets into the respective heap are stored as 2-bytes,
    /// If the flag is set, then the offsets are stored as 4-bytes.
    /// </summary>
    [Flags]
    public enum StreamOffsetSizeFlags : byte
    {
        String = 0x01,
        GUID = 0x02,
        Blob = 0x04,
        TypeDefOrRef
    }
    [Flags]
    public enum MetadataTableFlags : ulong
    {
        Module = 1,
        TypeRef = 2,
        TypeDef = 4,
        Reserved1 = 8,
        Field = 16,
        Reserved2 = 32,
        Method = 64,
        Reserved3 = 128,
        Param = 256,
        InterfaceImpl = 512,
        MemberRef = 1024,
        Constant = 2048,
        CustomAttribute = 4096,
        FieldMarshal = 8192,
        DeclSecurity = 16384,
        ClassLayout = 32768,
        FieldLayout = 65536,
        StandAloneSig = 131072,
        EventMap = 262144,
        Reserved4 = 524288,
        Event = 1048576,
        PropertyMap = 2097152,
        Reserved5 = 4194304,
        Property = 8388608,
        MethodSemantics = 16777216,
        MethodImpl = 33554432,
        ModuleRef = 67108864,
        TypeSpec = 134217728,
        ImplMap = 268435456,
        FieldRVA = 536870912,
        Reserved6 = 1073741824,
        Reserved7 = 2147483648,
        Assembly = 4294967296,
        AssemblyProcessor = 8589934592,
        AssemblyOS = 17179869184,
        AssemblyRef = 34359738368,
        AssemblyRefProcessor = 68719476736,
        AssemblyRefOS = 137438953472,
        File = 274877906944,
        ExportedType = 549755813888,
        ManifestResource = 1099511627776,
        NestedClass = 2199023255552,
        GenericParam = 4398046511104,
        MethodSpec = 8796093022208,
        GenericParamConstraint = 17592186044416,
    }
    public enum FieldAttribs
    {
        // member access mask - Use this mask to retrieve 

        // accessibility information.

        fdFieldAccessMask = 0x0007,
        fdPrivateScope = 0x0000,
        // Member not referenceable.

        fdPrivate = 0x0001,
        // Accessible only by the parent type.

        fdFamANDAssem = 0x0002,
        // Accessible by sub-types only in this Assembly.

        fdAssembly = 0x0003,
        // Accessibly by anyone in the Assembly.

        fdFamily = 0x0004,
        // Accessible only by type and sub-types.

        fdFamORAssem = 0x0005,
        // Accessibly by sub-types anywhere, plus anyone in assembly.

        fdPublic = 0x0006,
        // Accessibly by anyone who has visibility to this scope.

        // end member access mask

        // field contract attributes.

        fdStatic = 0x0010,
        // Defined on type, else per instance.

        fdInitOnly = 0x0020,
        // Field may only be initialized, not written to after init.

        fdLiteral = 0x0040,
        // Value is compile time constant.

        fdNotSerialized = 0x0080,
        // Field does not have to be serialized when type is remoted.

        fdSpecialName = 0x0200,
        // field is special. Name describes how.

        // interop attributes

        fdPinvokeImpl = 0x2000,
        // Implementation is forwarded through pinvoke.

        // Reserved flags for runtime use only.

        fdReservedMask = 0x9500,
        fdRTSpecialName = 0x0400,
        // Runtime(metadata internal APIs) should check name encoding.

        fdHasFieldMarshal = 0x1000,
        // Field has marshalling information.

        fdHasDefault = 0x8000,
        // Field has default.

        fdHasFieldRVA = 0x0100,
        // Field has RVA.
    }
    internal enum TypeFlags
    {
        // Use this mask to retrieve the type visibility information.

        tdVisibilityMask = 0x00000007,
        tdNotPublic = 0x00000000,
        // Class is not public scope.

        tdPublic = 0x00000001,
        // Class is public scope.

        tdNestedPublic = 0x00000002,
        // Class is nested with public visibility.

        tdNestedPrivate = 0x00000003,
        // Class is nested with private visibility.

        tdNestedFamily = 0x00000004,
        // Class is nested with family visibility.

        tdNestedAssembly = 0x00000005,
        // Class is nested with assembly visibility.

        tdNestedFamANDAssem = 0x00000006,
        // Class is nested with family and assembly visibility.

        tdNestedFamORAssem = 0x00000007,
        // Class is nested with family or assembly visibility.


        // Use this mask to retrieve class layout information

        tdLayoutMask = 0x00000018,
        tdAutoLayout = 0x00000000,
        // Class fields are auto-laid out

        tdSequentialLayout = 0x00000008,
        // Class fields are laid out sequentially

        tdExplicitLayout = 0x00000010,
        // Layout is supplied explicitly

        // end layout mask


        // Use this mask to retrieve class semantics information.

        tdClassSemanticsMask = 0x00000060,
        tdClass = 0x00000000,
        // Type is a class.

        tdInterface = 0x00000020,
        // Type is an interface.

        // end semantics mask


        // Special semantics in addition to class semantics.

        tdAbstract = 0x00000080,
        // Class is abstract

        tdSealed = 0x00000100,
        // Class is concrete and may not be extended

        tdSpecialName = 0x00000400,
        // Class name is special. Name describes how.


        // Implementation attributes.

        tdImport = 0x00001000,
        // Class / interface is imported

        tdSerializable = 0x00002000,
        // The class is Serializable.


        // Use tdStringFormatMask to retrieve string information for native interop

        tdStringFormatMask = 0x00030000,
        tdAnsiClass = 0x00000000,
        // LPTSTR is interpreted as ANSI in this class

        tdUnicodeClass = 0x00010000,
        // LPTSTR is interpreted as UNICODE

        tdAutoClass = 0x00020000,
        // LPTSTR is interpreted automatically

        tdCustomFormatClass = 0x00030000,
        // A non-standard encoding specified by CustomFormatMask

        tdCustomFormatMask = 0x00C00000,
        // Use this mask to retrieve non-standard encoding 

        // information for native interop. 

        // The meaning of the values of these 2 bits is unspecified.


        // end string format mask


        tdBeforeFieldInit = 0x00100000,
        // Initialize the class any time before first static field access.

        tdForwarder = 0x00200000,
        // This ExportedType is a type forwarder.


        // Flags reserved for runtime use.

        tdReservedMask = 0x00040800,
        tdRTSpecialName = 0x00000800,
        // Runtime should check name encoding.

        tdHasSecurity = 0x00040000,
        // Class has security associate with it.
    }
}
