using LibDotNetParser;
using LibDotNetParser.DotNet.Tabels.Defs;
using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.CILApi
{
    public class DotNetType
    {
        private readonly PEFile file;
        private readonly TypeDef type;
        private readonly TypeFlags flags;
        private readonly int NextTypeIndex;

        public string Name { get; private set; }
        public string NameSpace { get; private set; }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(NameSpace))
                    return Name;
                else
                    return NameSpace + "." + Name;
            }
        }
        public bool IsPublic
        {
            get
            {
                return (flags & TypeFlags.tdPublic) != 0;
            }
        }
        public bool IsInterface
        {
            get
            {
                return (flags & TypeFlags.tdInterface) != 0;
            }
        }

        private List<DotNetMethod> methods = new List<DotNetMethod>();
        public List<DotNetMethod> Methods
        {
            get
            {
                return methods;
            }
        }

        private List<DotNetField> fields = new List<DotNetField>();
        public List<DotNetField> Fields
        {
            get
            {
                return fields;
            }
        }

        public DotNetFile File { get; internal set; }

        /// <summary>
        /// Should be used internaly
        /// </summary>
        /// <param name="file"></param>
        /// <param name="item"></param>
        /// <param name="NextTypeIndex"></param>
        public DotNetType(DotNetFile file, TypeDef item, int NextTypeIndex)
        {
            this.file = file.Backend;
            this.type = item;
            this.File = file;
            this.NextTypeIndex = NextTypeIndex;
            this.flags = (TypeFlags)item.Flags;

            Name = this.file.ClrStringsStream.GetByOffset(item.Name);
            NameSpace = this.file.ClrStringsStream.GetByOffset(item.Namespace);
            InitMethods();
            InitFields();
        }
        public override string ToString()
        {
            return $"Type {FullName}";
        }
        private void InitFields()
        {
            fields.Clear();
            uint startIndex = type.FieldList;

            int max;

            if (file.Tabels.TypeDefTabel.Count <= NextTypeIndex)
            {
                //Happens when this is the last type
                max = file.Tabels.FieldTabel.Count;
            }
            else
            {
                //Get the max value from the next type.
                max = (int)file.Tabels.TypeDefTabel[NextTypeIndex].FieldList;
                max--;
            }

            if (file.Tabels.FieldTabel.Count < max)
            {
                //No more fields for this type
                return;
            }
            for (uint i = startIndex - 1; i < max; i++)
            {
                if (file.Tabels.FieldTabel.Count != 0)
                {
                    Field f;
                    f = file.Tabels.FieldTabel[(int)i];
                    //var item = file.Tabels.FieldTabel[(int)i -1 ];
                    fields.Add(new DotNetField(file, f, this, (int)i+1));
                }
            }
        }

        private void InitMethods()
        {
            methods.Clear();
            uint startIndex = type.MethodList;

            int max;

            if (file.Tabels.TypeDefTabel.Count <= NextTypeIndex)
            {
                //Happens when this is the last type
                max = file.Tabels.MethodTabel.Count;
            }
            else
            {
                //Get the max value from the next type.
                max = (int)file.Tabels.TypeDefTabel[NextTypeIndex].MethodList;
                max--;
            }

            for (uint i = startIndex - 1; i < max; i++)
            {
                if ((startIndex - 1) == max)
                {
                    //No methods for this type, contiune
                    break;
                }
                if (file.Tabels.MethodTabel.Count != 1)
                {
                    Method m;
                    if ((int)i != file.Tabels.MethodTabel.Count)
                        m= file.Tabels.MethodTabel[(int)i];
                    else
                        m = file.Tabels.MethodTabel[(int)i - 1];
                    try
                    {
                        methods.Add(new DotNetMethod(file, m, this));
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error while creating method: " + ex);
                    }
                }
            }
        }
    }
}
