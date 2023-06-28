using System;
using System.Collections.Generic;

namespace LibDotNetParser.CILApi
{
    public class DotNetFile
    {
        private readonly PEFile peFile;
        private readonly List<DotNetType> types = new List<DotNetType>();
        private DotNetType entryType;
        private DotNetMethod entryMethod;
        public PEFile Backend
        {
            get { return peFile; }
        }
        public List<DotNetType> Types
        {
            get
            {
                return types;
            }
        }
        /// <summary>
        /// Entry point of EXE/DLL. Will be null if EXE/DLL does not have entry point.
        /// </summary>
        public DotNetMethod EntryPoint
        {
            get
            {
                return entryMethod;
            }
        }

        public DotNetType EntryPointType
        {
            get
            {
                return entryType;
            }
        }
        public DotNetFile(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentException(nameof(Path));
            }

            peFile = new PEFile(Path);
            if (!peFile.ContainsMetadata)
            {
                throw new System.Exception("EXE File has no .NET Metadata");
            }

            FindTypes();
            InitEntryType();
        }
        public DotNetFile(byte[] file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            peFile = new PEFile(file);
            if (!peFile.ContainsMetadata)
            {
                throw new System.Exception("EXE File has no .NET Metadata");
            }

            FindTypes();
            InitEntryType();
        }

        private void InitEntryType()
        {
            var c = peFile.ClrHeader.EntryPointToken;
            var entryPoint = c & 0xFF;
            if (entryPoint == 0)
            {
                //No entry point
                entryType = null;
                entryMethod = null;
                return;
            }

            foreach (var item in Types)
            {
                foreach (var m2 in item.Methods)
                {
                    if (m2.BackendTabel == peFile.Tabels.MethodTabel[(int)entryPoint - 1])
                    {
                        entryType = m2.Parent;
                        entryMethod = m2;
                        break;
                    }
                }
            }
        }

        private void FindTypes()
        {
            int i = 0;
            foreach (var item in peFile.Tabels.TypeDefTabel)
            {
                types.Add(new DotNetType(this, item, i + 1));
                i++;
            }
        }

        public DotNetMethod GetMethod(string Namespace, string Classname, string MethodName)
        {
            foreach (var t in types)
            {
                foreach (var m in t.Methods)
                {
                    if (m.Name == MethodName && t.Name == Classname&&t.NameSpace == Namespace)
                    {
                        return m;
                    }
                }
            }
            return null;
        }
    }
}
