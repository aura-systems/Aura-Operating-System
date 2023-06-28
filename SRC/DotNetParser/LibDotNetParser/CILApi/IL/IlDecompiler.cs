using LibDotNetParser.CILApi.IL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LibDotNetParser.CILApi
{
    public class IlDecompiler
    {
        private DotNetMethod m;
        private DotNetFile mainFile;
        private byte[] code;
        private List<DotNetFile> ContextTypes = new List<DotNetFile>();

        public IlDecompiler(DotNetMethod method)
        {
            if (method == null)
                throw new ArgumentException("method");

            m = method;
            mainFile = m.File;
            code = m.GetBody();
            AddReference(m.Parent.File);
        }
        public void AddReference(DotNetFile f)
        {
            ContextTypes.Add(f);
        }

        public ILInstruction[] Decompile()
        {
            List<ILInstruction> inr = new List<ILInstruction>();
            int Instructions = 0;
            for (int i = 0; i < code.Length; i++)
            {
                var instruction = GetInstructionAtOffset(i, Instructions);
                if (instruction != null)
                {
                    inr.Add(instruction);


                    i += instruction.Size;
                    Instructions++;
                }

            }

            return inr.ToArray();
        }

        public ILInstruction GetInstructionAtOffset(int Offset, int relPostion)
        {
            byte opCodeb = code[Offset];
            var opCode = OpCodes.SingleOpCodes[opCodeb];

            int size = 0;
            if (opCodeb == 0xFE)
            {
                opCodeb = code[Offset + 1];
                opCode = OpCodes.MultiOpCodes[opCodeb];
                Offset++;
                size++;

                //make sure that opcode value is correct
                opCode.Value = BitConverter.ToUInt16(new byte[] { opCodeb,0xFE }, 0);
            }
            if (opCode == null)
            {
                Console.WriteLine("ILDecompiler failed decompilation.");
                opCode = OpCodes.SingleOpCodes[0];
            }

            ILInstruction ret = new ILInstruction() { OpCode = opCode.Value, OpCodeName = opCode.Name, OperandType = opCode.OpCodeOperandType, Position = Offset, RelPosition = relPostion, Size = size };

            if (relPostion == -1)
            {
                var arr = Decompile();
                foreach (var item in arr)
                {
                    if (item.Position == Offset)
                    {
                        return item;
                    }
                }
                throw new Exception("Target instruction not found!");
            }
            //TODO: Implment the rest of these
            switch (opCode.OpCodeOperandType)
            {
                case OpCodeOperandType.InlineNone:
                    {
                        return ret;
                    }
                case OpCodeOperandType.InlinePhi:
                    //Never should be used
                    throw new InvalidOperationException();
                case OpCodeOperandType.InlineTok:
                    {
                        byte fi = code[Offset + 1];
                        byte s2 = code[Offset + 2];
                        byte t = code[Offset + 3];
                        byte Tabel = code[Offset + 4];
                        byte[] num2 = new byte[] { fi, s2, t, 0 };
                        var numb2 = BitConverter.ToInt32(num2, 0);
                        var tabel2 = BitConverter.ToInt32(code, Offset+1);
                        var tabel = tabel2 >> 24;


                        var info = new FieldInfo();
                        info.IndexInTabel = numb2;

                        //TODO: fix this as the below if statement maybe incorrect

                        if (Tabel == 1)
                        {
                            // type ref
                            var typeRef = mainFile.Backend.Tabels.TypeRefTabel[numb2 - 1];

                            info.IsInFieldTabel = false;
                            info.Name = mainFile.Backend.ClrStringsStream.GetByOffset(typeRef.TypeName);
                            info.Namespace = mainFile.Backend.ClrStringsStream.GetByOffset(typeRef.TypeNamespace);
                        }
                        else if (Tabel == 2)
                        {
                            //type def
                            var typeRef = mainFile.Backend.Tabels.TypeDefTabel[numb2 - 1];

                            info.IsInFieldTabel = true;
                            info.Name = mainFile.Backend.ClrStringsStream.GetByOffset(typeRef.Name);
                            info.Namespace = mainFile.Backend.ClrStringsStream.GetByOffset(typeRef.Namespace);
                        }
                        else if (Tabel == 0x1B)
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        ret.Size += 4;
                        ret.Operand = info;
                        return ret;
                    }
                //8 bit int operand
                case OpCodeOperandType.ShortInlineVar:
                    {
                        byte fi = code[Offset + 1];
                        ret.Size += 1;
                        ret.Operand = fi;
                        return ret;
                    }
                case OpCodeOperandType.ShortInlineBrTarget:
                    {
                        sbyte fi = (sbyte)code[Offset + 1];
                        ret.Size += 1;
                        ret.Operand = fi + 1;
                        return ret;
                    }
                case OpCodeOperandType.ShortInlineI:
                    {
                        byte fi = code[Offset + 1];
                        ret.Size += 1;
                        ret.Operand = fi;
                        return ret;
                    }
                // 16 bit int
                case OpCodeOperandType.InlineVar:
                    throw new NotImplementedException();
                // 32 bit int
                case OpCodeOperandType.InlineI:
                    {
                        var numb2 = BitConverter.ToInt32(code, Offset + 1);

                        ret.Size += 4;
                        ret.Operand = numb2;
                        return ret;
                    }
                case OpCodeOperandType.InlineBrTarget:
                    {
                        var numb2 = BitConverter.ToInt32(code, Offset + 1);

                        ret.Size += 4;
                        ret.Operand = numb2 + 4; //add the size
                        return ret;
                    }
                case OpCodeOperandType.InlineField:
                    {
                        byte f = code[Offset + 4];
                        var numb2 = BitConverter.ToUInt16(code, Offset + 1);

                        ret.Size += 4;

                        if (f == 4)
                        {
                            //Inside of field table
                            var c = mainFile.Backend.Tabels.FieldTabel[numb2 - 1];
                            var ret2 = new FieldInfo() { Name = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name), IsInFieldTabel = true, IndexInTabel = numb2 };


                            ret.Operand = ret2;
                        }
                        else if (f == 10)
                        {
                            //Inside of MemberRef table
                            var c = mainFile.Backend.Tabels.MemberRefTabelRow[numb2 - 1];

                            var ret2 = new FieldInfo() { Name = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name), IsInFieldTabel = true, IndexInTabel = numb2 };

                            DecodeMemberRefParent(c.Class, out MemberRefParentType type, out uint row2);
                            if (type == MemberRefParentType.TypeSpec)
                            {   //Method spec
                                var tt = mainFile.Backend.Tabels.TypeSpecTabel[(int)(row2 - 1)];

                                //See https://docs.microsoft.com/en-us/dotnet/framework/unmanaged-api/metadata/corelementtype-enumeration for more info
                                var b = mainFile.Backend.BlobStream[tt.Signature];
                                var bi = new BinaryReader(new MemoryStream(mainFile.Backend.BlobStream));
                                bi.BaseStream.Position = tt.Signature;

                                var a = bi.ReadByte(); //Normally 0x5
                                var type3 = bi.ReadByte(); //Normally 0x15 (ELEMENT_TYPE_GENERICINST)
                                var n = ParseNumber(bi); //
                                var n2 = ParseNumber(bi);
                                var bb = bi.ReadByte(); //PTR_END
                                var aa = bi.ReadByte(); //depends on the generic type.
                                uint type2;
                                uint index;
                                DecodeTypeDefOrRef((uint)n2, out type2, out index);
                                uint Namespace = 0;
                                uint classs = 0;

                                if (type2 == 0)
                                {
                                    //Type def
                                    var Realtabel = mainFile.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                                    Namespace = Realtabel.Namespace;
                                    classs = Realtabel.Name;
                                }
                                else if (type2 == 1)
                                {
                                    //Type ref
                                    var Realtabel = mainFile.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                                    Namespace = Realtabel.TypeNamespace;
                                    classs = Realtabel.TypeName;
                                }
                                else if (type2 == 2)
                                {
                                    //Type spec???? what we just tried to read one
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    throw new NotImplementedException("Invaild type");
                                }

                                ret2.Namespace = mainFile.Backend.ClrStringsStream.GetByOffset(Namespace);
                                ret2.Class = mainFile.Backend.ClrStringsStream.GetByOffset(classs);
                                ret.Operand = ret2;
                            }
                            else if (type == MemberRefParentType.TypeRef)
                            {
                                var tt = mainFile.Backend.Tabels.TypeDefTabel[(int)(row2 - 1)];

                                ret2.Name = mainFile.Backend.ClrStringsStream.GetByOffset(tt.Name);
                                ret2.Namespace = mainFile.Backend.ClrStringsStream.GetByOffset(tt.Namespace);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        return ret;
                    }
                case OpCodeOperandType.InlineMethod:
                    {
                        byte fi = code[Offset + 1];
                        byte s2 = code[Offset + 2];
                        byte t = code[Offset + 3];
                        byte f = code[Offset + 4]; //Method type. 6=Method,10=MemberRef
                        byte[] num2 = new byte[] { fi, s2, t, 0 };
                        var numb2 = BitConverter.ToInt32(num2, 0); //Method Token

                        if (f == 10) //MemberRef
                        {
                            //Get the method that we are calling
                            var c = mainFile.Backend.Tabels.MemberRefTabelRow[numb2 - 1];

                            #region Decode
                            //Decode the class bytes
                            DecodeMemberRefParent(c.Class, out MemberRefParentType tabel, out uint row);


                            var funcName = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name);
                            uint classs = 0;
                            uint Namespace = 0;
                            string genericArgType = "";

                            //TYPE def
                            if (tabel == MemberRefParentType.TypeDef)
                            {
                                var tt = mainFile.Backend.Tabels.TypeDefTabel[(int)row - 1];

                                classs = tt.Name;
                                Namespace = tt.Namespace;

                            }
                            //Type REF
                            else if (tabel == MemberRefParentType.TypeRef)
                            {
                                var tt = mainFile.Backend.Tabels.TypeRefTabel[(int)row - 1];

                                classs = tt.TypeName;


                                Namespace = tt.TypeNamespace;
                            }
                            //Module Ref
                            else if (tabel == MemberRefParentType.ModuleRef)
                            {
                                var tt = mainFile.Backend.Tabels.ModuleRefTabel[(int)row - 1];

                                classs = tt.Name;
                                Namespace = tt.Name;
                            }
                            //Type Spec
                            else if (tabel == MemberRefParentType.TypeSpec)
                            {
                                //See https://docs.microsoft.com/en-us/dotnet/framework/unmanaged-api/metadata/corelementtype-enumeration for more info
                                var tt = mainFile.Backend.Tabels.TypeSpecTabel[(int)row - 1];
                                var b = mainFile.Backend.BlobStream[tt.Signature];
                                var bi = new BinaryReader(new MemoryStream(mainFile.Backend.BlobStream));
                                bi.BaseStream.Position = tt.Signature;

                                var a = bi.ReadByte(); //Normally 0x5
                                var type = bi.ReadByte(); //Normally 0x15 (ELEMENT_TYPE_GENERICINST)
                                var n = ParseNumber(bi); //
                                var n2 = ParseNumber(bi);
                                var bb = bi.ReadByte(); //PTR_END
                                var aa = bi.ReadByte(); //depends on the generic type.
                                uint type2;
                                uint index;
                                DecodeTypeDefOrRef((uint)n2, out type2, out index);

                                if (type2 == 0)
                                {
                                    //Type def
                                    var Realtabel = mainFile.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                                    Namespace = Realtabel.Namespace;
                                    classs = Realtabel.Name;
                                }
                                else if (type2 == 1)
                                {
                                    //Type ref
                                    var Realtabel = mainFile.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                                    Namespace = Realtabel.TypeNamespace;
                                    classs = Realtabel.TypeName;
                                }
                                else if (type2 == 2)
                                {
                                    //Type spec???? What
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    throw new NotImplementedException("Invaild type");
                                }
                                if (aa == 0xE)
                                {
                                    //String
                                    genericArgType = "string";
                                }
                                else if (aa == 0x13)
                                {
                                    genericArgType = "var";
                                }
                                else
                                {
                                    //TODO: implement the rest of these
                                    genericArgType = "Todo";
                                    //throw new NotImplementedException();
                                }
                            }
                            //Unknown
                            else
                            {
                                classs = 0;
                                Namespace = 0;
                                throw new NotImplementedException();
                            }
                            #endregion
                            var anamespace = mainFile.Backend.ClrStringsStream.GetByOffset(Namespace);
                            var typeName = mainFile.Backend.ClrStringsStream.GetByOffset(classs);

                            //Now, resolve this method
                            //TODO: Resolve the method properly by first
                            //1) resolve the type of the method
                            //2) search for the method in the type
                            //3) get method RVA

                            //For now, resolve it by name
                            var sig = DotNetMethod.ParseMethodSignature(c.Signature, mainFile, funcName).Signature;
                            DotNetMethod m = null;
                            foreach (var type in ContextTypes)
                            {
                                foreach (var item in type.Types)
                                {
                                    foreach (var meth in item.Methods)
                                    {
                                        if (meth.Name == funcName && meth.Parent.Name == typeName && meth.Parent.NameSpace == anamespace && meth.Signature == sig)
                                        {
                                            m = meth;
                                        }
                                    }
                                }

                            }
                            uint rva = 0;
                            if (m != null)
                                rva = m.RVA;
                            else
                            {
                                //Ignore if it is system object constructor
                                if (anamespace == "System" && typeName == "Object" && funcName == ".ctor")
                                {

                                }
                                else
                                {
                                    // Console.WriteLine($"[ILDecompiler: WARN] Cannot resolve method RVA. Are you missing a call to AddReference()??. Method data: {anamespace}.{typeName}.{funcName}");
                                }
                            }

                            ret.Size += 4;
                            ret.Operand = new InlineMethodOperandData()
                            {
                                NameSpace = anamespace,
                                ClassName = typeName,
                                FunctionName = funcName,
                                RVA = rva,
                                GenericArg = genericArgType,
                                Signature = sig
                            };
                            return ret;
                        }
                        else if (f == 6)//method
                        {
                            //Get the method that we are calling
                            var c = mainFile.Backend.Tabels.MethodTabel[numb2 - 1];
                            string name = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name);
                            //Now, resolve this method
                            DotNetMethod m = null;
                            foreach (var item in mainFile.Types)
                            {
                                foreach (var meth in item.Methods)
                                {
                                    if (meth.RVA == c.RVA && meth.Name == name && meth.ParamListIndex == c.ParamList)
                                    {
                                        m = meth;
                                    }
                                }
                            }

                            string className = "CannotFind";
                            string Namespace = "CannotFind";

                            if (m != null)
                            {
                                className = m.Parent.Name;
                                Namespace = m.Parent.NameSpace;
                            }
                            ret.Size += 4;
                            ret.Operand = new InlineMethodOperandData()
                            {
                                NameSpace = Namespace,
                                ClassName = className,
                                FunctionName = name,
                                RVA = c.RVA,
                                Signature = m.Signature,
                                ParamListIndex = c.ParamList
                            };
                            return ret;
                        }
                        else if (f == 0x2B)
                        {
                            //Method spec
                            var idx = mainFile.Backend.Tabels.MethodSpecTable[numb2 - 1];
                            uint fa;
                            uint row;
                            DecodeMethodDefOrRef(idx.Method, out fa, out row);

                            if (fa == 0)
                            {
                                //Method
                                var c = mainFile.Backend.Tabels.MethodTabel[(int)(row - 1)];
                                string name = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name);
                                //Now, resolve this method
                                DotNetMethod m = null;
                                foreach (var item in mainFile.Types)
                                {
                                    foreach (var meth in item.Methods)
                                    {
                                        if (meth.RVA == c.RVA && meth.Name == name)
                                        {
                                            m = meth;
                                        }
                                    }
                                }

                                string className = "CannotFind";
                                string Namespace = "CannotFind";

                                if (m != null)
                                {
                                    className = m.Parent.Name;
                                    Namespace = m.Parent.NameSpace;
                                }
                                ret.Size += 4;
                                ret.Operand = new InlineMethodOperandData()
                                {
                                    NameSpace = Namespace,
                                    ClassName = className,
                                    FunctionName = name,
                                    RVA = c.RVA,
                                    Signature = m.Signature,

                                };
                                return ret;
                            }
                            else if (fa == 1)
                            {
                                //MemberRef
                                var c = mainFile.Backend.Tabels.MemberRefTabelRow[(int)(row - 1)];

                                #region Decode
                                //Decode the class bytes
                                DecodeMemberRefParent(c.Class, out MemberRefParentType tabel, out uint row2);


                                var funcName = mainFile.Backend.ClrStringsStream.GetByOffset(c.Name);
                                uint classs = 0;
                                uint Namespace = 0;
                                string genericArgType = "";

                                //TYPE def
                                if (tabel == MemberRefParentType.TypeDef)
                                {
                                    var tt = mainFile.Backend.Tabels.TypeDefTabel[(int)row2 - 1];

                                    classs = tt.Name;
                                    Namespace = tt.Namespace;

                                }
                                //Type REF
                                else if (tabel == MemberRefParentType.TypeRef)
                                {
                                    var tt = mainFile.Backend.Tabels.TypeRefTabel[(int)row2 - 1];

                                    classs = tt.TypeName;


                                    Namespace = tt.TypeNamespace;
                                }
                                //Module Ref
                                else if (tabel == MemberRefParentType.ModuleRef)
                                {
                                    var tt = mainFile.Backend.Tabels.ModuleRefTabel[(int)row2 - 1];

                                    classs = tt.Name;
                                    Namespace = tt.Name;
                                }
                                //Type Spec
                                else if (tabel == MemberRefParentType.TypeSpec)
                                {
                                    //See https://docs.microsoft.com/en-us/dotnet/framework/unmanaged-api/metadata/corelementtype-enumeration for more info
                                    var tt = mainFile.Backend.Tabels.TypeSpecTabel[(int)row2 - 1];
                                    var b = mainFile.Backend.BlobStream[tt.Signature];
                                    var bi = new BinaryReader(new MemoryStream(mainFile.Backend.BlobStream));
                                    bi.BaseStream.Position = tt.Signature;

                                    var a = bi.ReadByte(); //Normally 0x5
                                    var type = bi.ReadByte(); //Normally 0x15 (ELEMENT_TYPE_GENERICINST)
                                    var n = ParseNumber(bi); //
                                    var n2 = ParseNumber(bi);
                                    var bb = bi.ReadByte(); //PTR_END
                                    var aa = bi.ReadByte(); //depends on the generic type.
                                    uint type2;
                                    uint index;
                                    DecodeTypeDefOrRef((uint)n2, out type2, out index);

                                    if (type2 == 0)
                                    {
                                        //Type def
                                        var Realtabel = mainFile.Backend.Tabels.TypeDefTabel[(int)(index - 1)];
                                        Namespace = Realtabel.Namespace;
                                        classs = Realtabel.Name;
                                    }
                                    else if (type2 == 1)
                                    {
                                        //Type ref
                                        var Realtabel = mainFile.Backend.Tabels.TypeRefTabel[(int)(index - 1)];
                                        Namespace = Realtabel.TypeNamespace;
                                        classs = Realtabel.TypeName;
                                    }
                                    else if (type2 == 2)
                                    {
                                        //Type spec???? What
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("Invaild type");
                                    }
                                    if (aa == 0xE)
                                    {
                                        //String
                                        genericArgType = "string";
                                    }
                                    else if (aa == 0x13)
                                    {
                                        genericArgType = "var";
                                    }
                                    else
                                    {
                                        //TODO: implement the rest of these
                                        genericArgType = "Todo";
                                        //throw new NotImplementedException();
                                    }
                                }
                                //Unknown
                                else
                                {
                                    classs = 0;
                                    Namespace = 0;
                                    throw new NotImplementedException();
                                }
                                #endregion
                                var anamespace = mainFile.Backend.ClrStringsStream.GetByOffset(Namespace);
                                var typeName = mainFile.Backend.ClrStringsStream.GetByOffset(classs);

                                //Now, resolve this method
                                //TODO: Resolve the method properly by first
                                //1) resolve the type of the method
                                //2) search for the method in the type
                                //3) get method RVA

                                //For now, resolve it by name

                                DotNetMethod m = null;
                                foreach (var type in ContextTypes)
                                {
                                    foreach (var item in type.Types)
                                    {
                                        foreach (var meth in item.Methods)
                                        {
                                            if (meth.Name == funcName && meth.Parent.Name == typeName && meth.Parent.NameSpace == anamespace)
                                            {
                                                m = meth;
                                            }
                                        }
                                    }

                                }
                                uint rva = 0;
                                if (m != null)
                                    rva = m.RVA;
                                else
                                {
                                    //Ignore if it is system object constructor
                                    if (anamespace == "System" && typeName == "Object" && funcName == ".ctor")
                                    {

                                    }
                                    else
                                    {
                                        Console.WriteLine($"[ILDecompiler: WARN] Cannot resolve method RVA. Are you missing a call to AddReference()??. Method data: {anamespace}.{typeName}.{funcName}");
                                    }
                                }

                                ret.Size += 4;
                                ret.Operand = new InlineMethodOperandData()
                                {
                                    NameSpace = anamespace,
                                    ClassName = typeName,
                                    FunctionName = funcName,
                                    RVA = rva,
                                    GenericArg = genericArgType,
                                    Signature = DotNetMethod.ParseMethodSignature(c.Signature, mainFile, funcName).Signature
                                };
                                return ret;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                case OpCodeOperandType.InlineSig:
                    {
                        var numb2 = BitConverter.ToInt32(code, Offset + 1);

                        ret.Size += 4;
                        ret.Operand = numb2;
                        return ret;
                    }
                case OpCodeOperandType.InlineString:
                    {
                        byte first = code[Offset + 1]; //1st index
                        byte sec = code[Offset + 2];   //2nd
                        byte third = code[Offset + 3]; //3rd
                        byte forth = code[Offset + 4]; //string type
                        byte[] num = new byte[] { first, sec, third, 0 };
                        var numb = BitConverter.ToInt32(num, 0);

                        //Get the string
                        string s;

                        if (forth != 112)
                        {
                            //Will this ever be in the String Stream?
                            s = mainFile.Backend.ClrStringsStream.GetByOffset((uint)numb);
                        }
                        else
                        {
                            //US stream
                            s = mainFile.Backend.ClrUsStream.GetByOffset((uint)numb);
                        }
                        ret.Size += 4;
                        ret.Operand = s;
                        return ret;
                    }
                case OpCodeOperandType.InlineSwitch:
                    throw new NotImplementedException();
                case OpCodeOperandType.ShortInlineR:
                    {
                        var numb2 = BitConverter.ToSingle(code, Offset + 1);

                        ret.Size += 4;
                        ret.Operand = numb2;
                        return ret;
                    }
                case OpCodeOperandType.InlineType:
                    {
                        byte fi = code[Offset + 1];

                        ret.Size += 4;
                        ret.Operand = fi;
                        return ret;
                    }
                // 64 bit int
                case OpCodeOperandType.InlineI8:
                    {
                        var numb2 = BitConverter.ToInt64(code, Offset + 1);
                        ret.Size += 8;
                        ret.Operand = numb2;
                        return ret;
                    }
                case OpCodeOperandType.InlineR:
                    {
                        var numb2 = BitConverter.ToDouble(code, Offset + 1);
                        ret.Size += 8;
                        ret.Operand = numb2;
                        return ret;
                    }
                default:
                    break;
            }

            return null;
        }
        public static uint ParseNumber(BinaryReader r)
        {
            var b1 = r.ReadByte();
            if (b1 == 0xFF)
            {
                //NULL
                throw new Exception();
            }
            else if ((b1 & 0x80) == 0)
            {
                return b1;
            }
            var b2 = r.ReadByte();
            if ((b1 & 0x40) == 0)
            {
                return (uint)(((b1 & 0x3f) << 8) | b2);
            }
            // must be a 4 byte encoding

            if ((b1 & 0x20) != 0)
            {
                throw new Exception();
                // 4 byte encoding has this bit clear -- error if not
            }
            var b3 = r.ReadByte();
            var b4 = r.ReadByte();
            return (uint)(((b1 & 0x1f) << 24) | (b2 << 16) | (b3 << 8) | b4);
        }
        #region Decoding MemberRefParent
        private const uint MemberRefParrent = 0x7;
        private const uint MemberRefParrent_TYPEDEF = 0x0;
        private const uint MemberRefParrent_TYPEREF = 0x1;
        private const uint MemberRefParrent_MODULEREF = 0x2;
        private const uint MemberRefParrent_METHODDEF = 0x3;
        private const uint MemberRefParrent_TYPESPEC = 0x4;
        public static void DecodeMemberRefParent(uint index, out MemberRefParentType tableIndex, out uint row)
        {
            tableIndex = 0;
            switch (index & MemberRefParrent)
            {
                case MemberRefParrent_TYPEDEF:
                    tableIndex = MemberRefParentType.TypeDef;
                    break;

                case MemberRefParrent_TYPEREF:
                    tableIndex = MemberRefParentType.TypeRef;
                    break;

                case MemberRefParrent_MODULEREF:
                    tableIndex = MemberRefParentType.ModuleRef;
                    break;

                case MemberRefParrent_METHODDEF:
                    tableIndex = MemberRefParentType.MethodDef;
                    break;

                case MemberRefParrent_TYPESPEC:
                    tableIndex = MemberRefParentType.TypeSpec;
                    break;
            }
            row = index >> 3;
        }
        public static void DecodeTypeDefOrRef(uint num, out uint type, out uint index)
        {
            type = num & 0x3;
            index = (num >> 2);
        }
        internal const int RowIdBitCount = 24;
        internal const uint RIDMask = (1 << RowIdBitCount) - 1;
        internal const uint TagMask = 0x00000001;
        private const uint MethodDef = 0x06 << RowIdBitCount;
        private const uint MemberRef = 0x0A << RowIdBitCount;
        internal const uint TagToTokenTypeByteVector = MethodDef >> 24 | MemberRef >> 16;
        private static void DecodeMethodDefOrRef(uint methodDefOrRef, out uint type, out uint row)
        {
            type = (methodDefOrRef & (1 << 0));

            row = (methodDefOrRef >> 1);
        }
        public enum MemberRefParentType
        {
            TypeDef,
            TypeRef,
            ModuleRef,
            MethodDef,
            TypeSpec
        }
        #endregion
    }
}
