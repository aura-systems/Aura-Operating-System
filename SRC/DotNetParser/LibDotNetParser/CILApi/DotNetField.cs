using LibDotNetParser.DotNet;
using LibDotNetParser.DotNet.Tabels.Defs;
using LibDotNetParser.PE;
using System.IO;
using static LibDotNetParser.CILApi.DotNetMethod;

namespace LibDotNetParser.CILApi
{
    public class DotNetField
    {
        private PEFile file;
        public DotNetType ParrentType;
        public Field BackendTabel;
        private FieldAttribs flags;
        public int IndexInTabel { get; private set; }
        public string Name { get; private set; }
        public bool IsStatic { get { return (flags & FieldAttribs.fdStatic) != 0; } }
        public bool HasDefault { get { return (flags & FieldAttribs.fdHasDefault) != 0; } }
        public bool IsPublic { get { return (flags & FieldAttribs.fdPublic) != 0; } }
        /// <summary>
        /// Value of the field.
        /// </summary>
        public MethodArgStack Value;

        public MethodSignatureParam ValueType { get; private set; }
        public DotNetField(PEFile file, Field backend, DotNetType parrent, int indexintable)
        {
            this.file = file;
            this.ParrentType = parrent;
            this.BackendTabel = backend;
            this.IndexInTabel = indexintable;
            flags = (FieldAttribs)BackendTabel.Flags;
            Name = file.ClrStringsStream.GetByOffset(backend.Name);


            //Read the field info in blob stream
            var b = new BinaryReader(new MemoryStream(file.BlobStream));
            b.BaseStream.Position = backend.Signature;

            var size = b.ReadByte();
            var prolog = b.ReadByte();

            ValueType = ReadParam(b, parrent.File);
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
