using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CosmosELFCore
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Elf32_Sym
    {
        [FieldOffset(0)] public uint st_name;
        [FieldOffset(4)] public uint st_value;
        [FieldOffset(8)] public uint st_size;
        [FieldOffset(12)] public byte st_info;
        [FieldOffset(13)] public byte st_other;
        [FieldOffset(14)] public ushort st_shndx;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Elf32_Rel
    {
        [FieldOffset(0)] public uint r_offset;
        [FieldOffset(4)] public uint r_info;
    }


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct Elf32_Ehdr
    {
        [FieldOffset(0)] public fixed byte e_ident[16];
        [FieldOffset(16)] public ushort e_type;
        [FieldOffset(18)] public ushort e_machine;
        [FieldOffset(20)] public uint e_version;
        [FieldOffset(24)] public uint e_entry;
        [FieldOffset(28)] public uint e_phoff;
        [FieldOffset(32)] public uint e_shoff;
        [FieldOffset(36)] public uint e_flags;
        [FieldOffset(40)] public ushort e_ehsize;
        [FieldOffset(42)] public ushort e_phentsize;
        [FieldOffset(44)] public ushort e_phnum;
        [FieldOffset(46)] public ushort e_shentsize;
        [FieldOffset(48)] public ushort e_shnum;
        [FieldOffset(50)] public ushort e_shstrndx;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Elf32_Shdr
    {
        [FieldOffset(0)] public uint sh_name;
        [FieldOffset(4)] public uint sh_type;
        [FieldOffset(8)] public uint sh_flags;
        [FieldOffset(12)] public uint sh_addr;
        [FieldOffset(16)] public uint sh_offset;
        [FieldOffset(20)] public uint sh_size;
        [FieldOffset(24)] public uint sh_link;
        [FieldOffset(28)] public uint sh_info;
        [FieldOffset(32)] public uint sh_addralign;
        [FieldOffset(36)] public uint sh_entsize;
    }

    public enum SectionType
    {
        None = 0,  
        ProgramInformation = 1,  
        SymbolTable = 2,   
        StringTable = 3, 
        RelocationAddend = 4,  
        NotPresentInFile = 8,  
        Relocation = 9,  
    }

    [Flags]
    public enum SectionAttributes
    {
        Write = 0x01,
        Alloc = 0x02  ,
        Executable = 0x4
    }


    public enum SymbolBinding
    {
        Local = 0, // Local scope
        Global = 1, // Global scope
        Weak = 2  // Weak, (ie. __attribute__((weak)))
    }

    public enum RelocationType
    {
        R386None = 0, // No relocation
        R38632 = 1, // Symbol + Offset
        R386Pc32 = 2  // Symbol + Offset - Section Offset
    };

    public enum SymbolType
    {
        None = 0, // No type
        Object = 1, // Variables, arrays, etc.
        Function = 2,  // Methods or functions
        Common = 5
    }

    public unsafe class Elf32Sym
    {
        public string Name;
        public uint NameOffset;
        public uint Value;
        public uint Size;
        public byte Info;
        public byte Other;
        public ushort Shndx;
        public SymbolBinding Binding;
        public SymbolType Type;

        public Elf32Sym(Elf32_Sym* st)
        {
            NameOffset = st->st_name;
            Value = st->st_value;
            Size = st->st_size;
            Info = st->st_info;
            Other = st->st_other;
            Shndx = st->st_shndx;

            Binding = (SymbolBinding) (Info >> 0x4);
            Type = (SymbolType) (Info & 0x0F);
        }
    }


    public unsafe class Elf32Rel
    {
        public uint Offset;
        public uint Info;
        public int Section;
        public uint Symbol;
        public RelocationType Type;

        public Elf32Rel(Elf32_Rel* st)
        {
            Offset = st->r_offset;
            Info = st->r_info;

            Symbol = Info >> 8;
            Type = (RelocationType) (byte) Info;
        }
    }


    public unsafe class Elf32Ehdr
    {
        public ushort Type;
        public ushort Machine;
        public uint Version;
        public uint Entry;
        public uint Phoff;
        public uint Shoff;
        public uint Flags;
        public ushort Ehsize;
        public ushort Phentsize;
        public ushort Phnum;
        public ushort Shentsize;
        public ushort Shnum;
        public ushort Shstrndx;

        public Elf32Ehdr(Elf32_Ehdr* st)
        {
            Type = st->e_type;
            Machine = st->e_machine;
            Version = st->e_version;
            Entry = st->e_entry;
            Phoff = st->e_phoff;
            Shoff = st->e_shoff;
            Flags = st->e_flags;
            Ehsize = st->e_ehsize;
            Phentsize = st->e_type;
            Phnum = st->e_phnum;
            Shentsize = st->e_shentsize;
            Shnum = st->e_shnum;
            Shstrndx = st->e_shstrndx;
        }
    }


    public unsafe class Elf32Shdr
    {
        public string Name;
        public uint NameOffset;
        public SectionType Type;
        public SectionAttributes Flag;
        public uint Addr;
        public uint Offset;
        public uint Size;
        public uint Link;
        public uint Info;
        public uint Addralign;
        public uint Entsize;

        public Elf32Shdr(Elf32_Shdr* st)
        {
            NameOffset = st->sh_name;
            Type = (SectionType)st->sh_type;
            Flag = (SectionAttributes)st->sh_flags;
            Addr = st->sh_addr;
            Offset = st->sh_offset;
            Size = st->sh_size;
            Link = st->sh_link;
            Info = st->sh_info;
            Addralign = st->sh_addralign;
            Entsize = st->sh_entsize;
        }
    }
}