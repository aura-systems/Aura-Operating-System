using LibDotNetParser.DotNet.Tabels.Defs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LibDotNetParser.CILApi
{
    public class DotNetMethod
    {
        private readonly PEFile file;
        private readonly DotNetFile file2;
        private readonly MethodAttr flags;
        private readonly MethodImp implFlags;

        public Method BackendTabel;

        /// <summary>
        /// Function Signature.
        /// </summary>
        public string Signature
        {
            get;
            private set;
        }

        public List<MethodSignatureParam> Parms = new List<MethodSignatureParam>();
        public string Name { get; private set; }
        public uint RVA { get { return BackendTabel.RVA; } }
        private uint _Offset = 0;
        public uint Offset
        {
            get
            {
                try
                {
                    if (_Offset == 0)
                        _Offset = (uint)BinUtil.RVAToOffset(RVA, file.PeHeader.Sections);
                }
                catch
                {
                    //probably an internal call / no body
                }

                return _Offset;
            }
        }
        public int AmountOfParms { get; private set; }
        public DotNetFile File
        {
            get
            {
                return file2;
            }
        }
        public bool IsStatic
        {
            get
            {
                return (flags & MethodAttr.mdStatic) != 0;
            }
        }
        public bool IsInternalCall
        {
            get
            {
                return (implFlags & MethodImp.miInternalCall) != 0;
            }
        }

        public bool IsImplementedByRuntime
        {
            get
            {
                return (implFlags & MethodImp.miRuntime) != 0;
            }
        }

        public bool IsExtern
        {
            get
            {
                return RVA == 0;
            }
        }

        public bool HasThis { get; }
        public DotNetType Parent { get; }
        public bool HasReturnValue { get; private set; }

        public MethodSignatureInfoV2 SignatureInfo { get; }
        public uint ParamListIndex { get; internal set; }
        /// <summary>
        /// Max amount of items on operand stacks
        /// </summary>
        public int MaxStackSize { get; private set; }
        private byte[] MethodBody;

        /// <summary>
        /// Internal use only
        /// </summary>
        /// <param name="file"></param>
        /// <param name="item"></param>
        /// <param name="parrent"></param>
        public DotNetMethod(PEFile file, Method item, DotNetType parrent)
        {
            this.file = file;
            this.BackendTabel = item;
            this.Parent = parrent;
            this.flags = (MethodAttr)item.Flags;
            this.implFlags = (MethodImp)item.ImplFlags;
            this.file2 = parrent.File;
            ParamListIndex = item.ParamList;
            this.Name = file.ClrStringsStream.GetByOffset(item.Name);

            //method signatures
            SignatureInfo = ParseMethodSignature(item.Signature, File, this.Name);
            this.Signature = SignatureInfo.Signature;
            this.AmountOfParms = SignatureInfo.AmountOfParms;
            Parms = SignatureInfo.Params;
            this.HasThis = SignatureInfo.HasThis;

            if (SignatureInfo.ReturnVal.type != StackItemType.None)
                HasReturnValue = true;

            if (RVA != 0)
                MethodBody = ReadMethodBody();
        }

        private byte[] ReadMethodBody()
        {
            var fs = file.RawFile;
            fs.BaseStream.Seek(Offset, System.IO.SeekOrigin.Begin);

            byte format = fs.ReadByte();
            int CodeSize = 0;
            var verytinyheader = format.ConvertByteToBoolArray();


            var header = BinUtil.ConvertBoolArrayToByte(new bool[] { verytinyheader[6], verytinyheader[7] });

            var sizer = BinUtil.ConvertBoolArrayToByte(new bool[] { verytinyheader[0], verytinyheader[1], verytinyheader[2], verytinyheader[3], verytinyheader[4], verytinyheader[5], });
            fs.BaseStream.Seek(Offset + 1, System.IO.SeekOrigin.Begin);
            byte form2 = fs.ReadByte();

            fs.BaseStream.Seek(Offset + 1, System.IO.SeekOrigin.Begin);

            if (header == 3) //Fat format
            {
                byte info2 = fs.ReadByte(); //some info on header
                MaxStackSize = fs.ReadUInt16();
                CodeSize = (int)fs.ReadUInt32();

                //todo: read local variable info in #blob stream
                uint LocalVarSigTok = fs.ReadUInt32();
            }
            else //Tiny format
            {
                CodeSize = sizer;
                MaxStackSize = 8; //Default stack size for tiny format
            }
            List<byte> code = new List<byte>();

            for (uint i = Offset + 1; i < Offset + 1 + CodeSize; i++)
            {
                byte opcode = fs.ReadByte();

                code.Add(opcode);
            }

            return code.ToArray();
        }
        /// <summary>
        /// Gets the raw IL instructions for this method.
        /// </summary>
        /// <returns>raw IL instructions</returns>
        public byte[] GetBody()
        {
            return MethodBody;
        }

        internal static MethodSignatureInfoV2 ParseMethodSignature(uint signature, DotNetFile file, string FunctionName)
        {
            MethodSignatureInfoV2 ret = new MethodSignatureInfoV2();
            string sig = "";
            var blobStreamReader = new BinaryReader(new MemoryStream(file.Backend.BlobStream));
            blobStreamReader.BaseStream.Seek(signature, SeekOrigin.Begin);
            var length = IlDecompiler.ParseNumber(blobStreamReader);
            var type = IlDecompiler.ParseNumber(blobStreamReader);
            var parmaters = IlDecompiler.ParseNumber(blobStreamReader); //This field becomes "generic parameters" if type & 10 != 0
            if ((type & 0x10) != 0)
            {
                ret.AmountOfGenericParams = (int)parmaters;
                //The "real" generic param amount
                parmaters = IlDecompiler.ParseNumber(blobStreamReader);
            }


            var returnVal = ReadParam(blobStreamReader, file); //read return value
            ret.ReturnVal = returnVal;

            if (type == 0)
            {
                //Static method
                sig += "static ";
                ret.IsStatic = true;
            }
            if ((type & 0x20) != 0)
            {
                ret.HasThis = true;
            }
            sig += returnVal.TypeInString;
            sig += " " + FunctionName;
            sig += "(";
            for (ulong i = 0; i < parmaters; i++)
            {
                var parm = ReadParam(blobStreamReader, file);
                ret.Params.Add(parm);
                sig += parm.TypeInString + ", ";
                ret.AmountOfParms++;
            }
            if (parmaters > 0)
                sig = sig.Substring(0, sig.Length - 2); //Remove the last ,
            sig += ");";
            ret.Signature = sig;
            return ret;
        }
        public override string ToString()
        {
            return $"{Name} in {Parent.FullName}";
        }
        internal static MethodSignatureParam ReadParam(BinaryReader r, DotNetFile file)
        {
            string sig;
            var parm = r.ReadByte();
            MethodSignatureParam ret = new MethodSignatureParam();
            switch (parm)
            {
                case 0x00:
                    {
                        //end of list
                        sig = "End of list";
                        ret.type = StackItemType.Array; //array maybe?
                        break;
                    }
                case 0x01:
                    {
                        sig = "void";
                        ret.type = StackItemType.None;
                        break;
                    }
                case 0x02:
                    {
                        sig = "bool";
                        ret.type = StackItemType.Boolean;
                        break;
                    }
                case 0x03:
                    {
                        sig = "char";
                        ret.type = StackItemType.Char;
                        break;
                    }
                case 0x04:
                    {
                        sig = "sbyte";
                        ret.type = StackItemType.SByte;
                        break;
                    }
                case 0x05:
                    {
                        sig = "byte";
                        ret.type = StackItemType.Byte;
                        break;
                    }
                case 0x06:
                    {
                        sig = "short";
                        ret.type = StackItemType.Int16;
                        break;
                    }
                case 0x07:
                    {
                        sig = "ushort";
                        ret.type = StackItemType.UInt16;
                        break;
                    }
                case 0x08:
                    {
                        sig = "int";
                        ret.type = StackItemType.Int32;
                        break;
                    }
                case 0x09:
                    {
                        sig = "uint";
                        ret.type = StackItemType.UInt32;
                        break;
                    }
                case 0x0A:
                    {
                        sig = "long";
                        ret.type = StackItemType.Int64;
                        break;
                    }
                case 0x0B:
                    {
                        sig = "ulong";
                        ret.type = StackItemType.UInt64;
                        break;
                    }
                case 0x0C:
                    {
                        sig = "float";
                        ret.type = StackItemType.Float32;
                        break;
                    }
                case 0x0D:
                    {
                        sig = "double";
                        ret.type = StackItemType.Float64;
                        break;
                    }
                case 0x0E:
                    {
                        sig = "string";
                        ret.type = StackItemType.String;
                        break;
                    }
                case 0xF:
                    //Pointer* (followed by type)
                    {
                        sig = ReadParam(r, file).TypeInString + "*";
                        ret.type = StackItemType.Int32;
                        break;
                    }
                case 0x10:
                    //byref* //followed by type
                    {
                        sig = "ref " + ReadParam(r, file).TypeInString;
                        ret.type = StackItemType.Int32;
                        break;
                    }
                case 0x11:
                    //valve type (followed by typedef or typeref token)
                    {
                        var t = IlDecompiler.ParseNumber(r); //type of the type
                        IlDecompiler.DecodeTypeDefOrRef(t, out uint rowType, out uint index);
                        string name;
                        string Namespace;
                        ret.type = StackItemType.Object;
                        ret.IsClass = true;
                        if (rowType == 0)
                        {
                            //typedef
                            var tt = file.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                            name = file.Backend.ClrStringsStream.GetByOffset(tt.Name);
                            Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.Namespace);

                            //resolve it
                            foreach (var item in file.Types)
                            {
                                if (item.NameSpace == Namespace && item.Name == name)
                                {
                                    ret.ClassType = item;
                                }
                            }
                        }
                        else if (rowType == 1)
                        {
                            //typeref
                            var tt = file.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                            name = file.Backend.ClrStringsStream.GetByOffset(tt.TypeName);
                            Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.TypeNamespace);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        if (!string.IsNullOrEmpty(Namespace))
                            sig = $"{Namespace}.{name}";
                        else
                            sig = $"{name}";
                        break;
                    }
                case 0x12:
                    //class followed by typedef or typeref token
                    {
                        var t = IlDecompiler.ParseNumber(r); //type of the type
                        IlDecompiler.DecodeTypeDefOrRef(t, out uint rowType, out uint index);
                        string name;
                        string Namespace;
                        ret.type = StackItemType.Object;
                        ret.IsClass = true;
                        if (rowType == 0)
                        {
                            //typedef
                            var tt = file.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                            name = file.Backend.ClrStringsStream.GetByOffset(tt.Name);
                            Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.Namespace);

                            //resolve it
                            foreach (var item in file.Types)
                            {
                                if (item.NameSpace == Namespace && item.Name == name)
                                {
                                    ret.ClassType = item;
                                }
                            }
                        }
                        else if (rowType == 1)
                        {
                            //typeref
                            var tt = file.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                            name = file.Backend.ClrStringsStream.GetByOffset(tt.TypeName);
                            Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.TypeNamespace);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        if (!string.IsNullOrEmpty(Namespace))
                            sig = $"{Namespace}.{name}";
                        else
                            sig = $"{name}";
                        break;
                    }
                case 0x13:
                    //GENERIC_PARM
                    {
                        var b = IlDecompiler.ParseNumber(r);
                        sig = "T";
                        ret.type = StackItemType.Any;
                        break;
                    }
                case 0x14:
                    {
                        sig = "[][]";
                        ret.type = StackItemType.Array;
                        break;
                    }
                case 0x15:
                    {
                        //ELEMENT_TYPE_GENERICINST
                        var t = IlDecompiler.ParseNumber(r); //type of the generic type
                        var c = IlDecompiler.ParseNumber(r); //generic type (TypeDefOrRefEncoded)
                        var d = IlDecompiler.ParseNumber(r); //Count of generic args
                        IlDecompiler.DecodeTypeDefOrRef((uint)c, out uint rowType, out uint index);
                        ret.IsGeneric = true;
                        ret.type = StackItemType.Object;
                        List<MethodSignatureParam> @params = new List<MethodSignatureParam>();
                        for (ulong i = 0; i < d; i++)
                        {
                            @params.Add(ReadParam(r, file));
                        }


                        if (rowType == 0)
                        {
                            //typedef
                            var tt = file.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                            var name = file.Backend.ClrStringsStream.GetByOffset(tt.Name);
                            var Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.Namespace);

                            if (!string.IsNullOrEmpty(Namespace))
                                sig = $"{Namespace}.{name}<";
                            else
                                sig = $"{name}<";

                            ret.GenericClassNamespace = Namespace;
                            ret.GenericClassName = name;
                        }
                        else if (rowType == 1)
                        {
                            //typeref
                            var tt = file.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                            var name = file.Backend.ClrStringsStream.GetByOffset(tt.TypeName);
                            var Namespace = file.Backend.ClrStringsStream.GetByOffset(tt.TypeNamespace);

                            if (!string.IsNullOrEmpty(Namespace))
                                sig = $"{Namespace}.{name}<";
                            else
                                sig = $"{name}<";

                            ret.GenericClassNamespace = Namespace;
                            ret.GenericClassName = name;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        foreach (var item in @params)
                        {
                            sig += item.TypeInString + ", ";
                        }
                        sig = sig.Substring(0, sig.Length - 2);
                        sig += ">";
                        break;
                    }
                case 0x16:
                    {
                        //TypedRefrerence structure
                        sig = "TypedReference";
                        ret.type = StackItemType.Object;
                        break;
                    }
                case 0x18:
                    {
                        //IntPtr
                        sig = "IntPtr";
                        ret.type = StackItemType.IntPtr;
                        break;
                    }
                case 0x19:
                    {
                        //UIntPtr
                        sig = "UIntPtr";
                        ret.type = StackItemType.UIntPtr;
                        break;
                    }
                case 0x1B:
                    {
                        sig = "func ptr";
                        ret.type = StackItemType.MethodPtr;
                        break;
                    }
                case 0x1C:
                    {
                        sig = "object";
                        ret.type = StackItemType.Object;
                        break;
                    }
                case 0x1D:
                    {
                        ret.ArrayType = ReadParam(r, file);
                        sig = ret.ArrayType.TypeInString + "[]";
                        ret.type = StackItemType.Array;
                        ret.IsArray = true;
                        break;
                    }
                case 0x1E:
                    //MVar
                    var num = IlDecompiler.ParseNumber(r);
                    sig = "T" + num;

                    break;
                case 0x1F:
                    var num3 = IlDecompiler.ParseNumber(r);
                    IlDecompiler.DecodeTypeDefOrRef((uint)num3, out uint rowType3, out uint index3);
                    sig = "TODO: 0x1F";
                    break;
                case 0x20:
                    var num2 = IlDecompiler.ParseNumber(r);
                    IlDecompiler.DecodeTypeDefOrRef((uint)num2, out uint rowType2, out uint index2);
                    sig = "TODO: 0x20";
                    break;
                default:
                    throw new System.NotImplementedException("Unknown byte: 0x" + parm.ToString("X"));
            }

            ret.TypeInString = sig;
            return ret;
        }
        public class MethodSignatureInfoV2
        {
            public MethodSignatureParam ReturnVal { get; set; }
            public List<MethodSignatureParam> Params = new List<MethodSignatureParam>();
            public bool IsStatic { get; set; } = false;
            public string Signature { get; set; } = "";
            public int AmountOfParms { get; set; } = 0;
            public int AmountOfGenericParams { get; set; } = 0;
            public bool HasThis { get; internal set; }
        }
        public class MethodSignatureParam
        {
            public StackItemType type;
            public string TypeInString;

            public bool IsGeneric { get; set; } = false;
            public string GenericClassNamespace { get; set; }
            public string GenericClassName { get; set; }
            public bool IsClass { get; set; } = false;
            public DotNetType ClassType { get; set; }
            public bool IsArray { get; set; } = false;
            public MethodSignatureParam ArrayType { get; set; }
        }
    }
}