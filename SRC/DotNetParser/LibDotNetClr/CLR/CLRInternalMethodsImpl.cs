using LibDotNetParser;
using LibDotNetParser.CILApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace libDotNetClr
{
    public partial class DotNetClr
    {
        private void RegisterAllInternalMethods()
        {
            //Register internal methods
            RegisterCustomInternalMethod("ReadLine", InternalMethod_Console_ReadLine);
            RegisterCustomInternalMethod("WriteLine", InternalMethod_Console_Writeline);
            RegisterCustomInternalMethod("Write", InternalMethod_Console_Write);
            RegisterCustomInternalMethod("ConsoleClear", InternalMethod_Console_Clear);
            RegisterCustomInternalMethod("Concat", InternalMethod_String_Concat);
            RegisterCustomInternalMethod("ByteToString", InternalMethod_Byte_ToString);
            RegisterCustomInternalMethod("SByteToString", Internal__System_SByte_ToString);
            RegisterCustomInternalMethod("UInt16ToString", Internal__System_UInt16_ToString);
            RegisterCustomInternalMethod("Int16ToString", Internal__System_Int16_ToString);
            RegisterCustomInternalMethod("Int32ToString", Internal__System_Int32_ToString);
            RegisterCustomInternalMethod("UInt32ToString", Internal__System_UInt32_ToString);
            RegisterCustomInternalMethod("UInt64ToString", Internal__System_UInt64_ToString);
            RegisterCustomInternalMethod("Int64ToString", Internal__System_Int64_ToString);


            RegisterCustomInternalMethod("CharToString", Internal__System_Char_ToString);


            RegisterCustomInternalMethod("op_Equality", InternalMethod_String_op_Equality);
            RegisterCustomInternalMethod("DebuggerBreak", DebuggerBreak);
            RegisterCustomInternalMethod("strLen", Internal__System_String_Get_Length);
            RegisterCustomInternalMethod("String_get_Chars_1", Internal__System_String_get_Chars_1);
            RegisterCustomInternalMethod("GetObjType", GetObjType);
            RegisterCustomInternalMethod("Type_FromReference", GetTypeFromReference);
            RegisterCustomInternalMethod("GetAssemblyFromType", GetAssemblyFromType);
            RegisterCustomInternalMethod("InternalAddItemToList", ListAddItem);
            RegisterCustomInternalMethod("String_ToUpper", String_ToUpper);
            RegisterCustomInternalMethod("String_ToLower", String_ToLower);

            RegisterCustomInternalMethod("System_Action..ctor_impl", ActionCtorImpl);
            for (int i = 1; i < 10; i++)
            {
                RegisterCustomInternalMethod($"System_Action`{i}..ctor_impl", ActionCtorImpl);
            }

            RegisterCustomInternalMethod("System_Action.Invoke_impl", ActionInvokeImpl);
            RegisterCustomInternalMethod("System_Action`1.Invoke_impl", Action1InvokeImpl);
            RegisterCustomInternalMethod("System_Action`2.Invoke_impl", Action2InvokeImpl);
            RegisterCustomInternalMethod("System_Action`3.Invoke_impl", Action3InvokeImpl);
            RegisterCustomInternalMethod("System_Action`4.Invoke_impl", Action4InvokeImpl);
            RegisterCustomInternalMethod("System_Action`5.Invoke_impl", Action5InvokeImpl);

            for (int i = 1; i < 10; i++)
            {
                RegisterCustomInternalMethod($"System_Func`{i}..ctor_impl", FuncCtorImpl);
            }

            RegisterCustomInternalMethod("System_Func`1.Invoke_impl", Func1InvokeImpl);
            RegisterCustomInternalMethod("System_Func`2.Invoke_impl", Func2InvokeImpl);

            RegisterCustomInternalMethod("Boolean_GetValue", Boolean_GetValue);
            RegisterCustomInternalMethod("InternalGetFields", InternalGetFields);
            RegisterCustomInternalMethod("InternalGetField", InternalGetField);
            RegisterCustomInternalMethod("List_AddItem", List_AddItem);
            //RegisterCustomInternalMethod("File__Exists", FileExists);
            RegisterCustomInternalMethod("String_IndexOf", String_IndexOf);
            RegisterCustomInternalMethod("String_EndsWith", String_EndsWith);
            RegisterCustomInternalMethod("String_SubString", String_SubString);

            RegisterCustomInternalMethod("GetMethod", Type_GetMethod);
            RegisterCustomInternalMethod("FieldInfo_SetValue", FieldInfo_SetValue);
            RegisterCustomInternalMethod("Split", String_Split);
            RegisterCustomInternalMethod("get_Rank", Array_GetRank);
            RegisterCustomInternalMethod("GetLowerBound", Array_GetLowerBound);
            RegisterCustomInternalMethod("get_Length", Array_GetLength);
        }

        private void Array_GetLength(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var array = Stack[0];
            if (array.type != StackItemType.Array) throw new Exception();

            returnValue = MethodArgStack.Int32(Arrays.ArrayRefs[(int)array.value].Length);
        }

        private void Array_GetLowerBound(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var array = Stack[0];
            var bound = Stack[1];

            if (array.type != StackItemType.Array) throw new Exception();

            //todo
            returnValue = MethodArgStack.Int32(0);
        }

        private void Array_GetRank(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            //We only support 1d arrays
            returnValue = MethodArgStack.Int32(1);
        }

        private void String_Split(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var toSplit = (string)Stack[0].value;
            var seperators = Arrays.ArrayRefs[Arrays.GetIndexFromRef(Stack[1])];
            List<char> chars = new List<char>();
            foreach (var item in seperators.Items)
            {
                chars.Add((char)(int)item.value);
            }

            var r = toSplit.Split(chars.ToArray());
            var arr= Arrays.AllocArray(r.Length);
            for (int i = 0; i < r.Length; i++)
            {
                arr.Items[i] = MethodArgStack.String(r[i]);
            }
            returnValue = MethodArgStack.Array(arr);
            ;
        }

        private void FieldInfo_SetValue(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var field = Stack[0];
            var objToSetOn = Stack[1];
            var value = Stack[2];

            var fieldType = (DotNetType)Objects.ObjectRefs[(int)field.value].Fields["ParrentObjectID"].value;
            var Thefield = (DotNetField)Objects.ObjectRefs[(int)field.value].Fields["InternalField"].value;

            if (objToSetOn == MethodArgStack.ldnull)
            {
                //set on static field
                int idx = -1;
                foreach (var item in StaticFieldHolder.staticFields)
                {
                    if (item.theField == Thefield)
                    {
                        break;
                    }
                    idx++;
                }

                StaticFieldHolder.staticFields[idx].value = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void String_SubString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = (string)Stack[0].value;
            var startIndex = (int)Stack[1].value;
            var len = (int)Stack[2].value;

            returnValue = MethodArgStack.String(str.Substring(startIndex, len));
        }

        private void String_EndsWith(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = (string)Stack[0].value;
            var cmp = (string)Stack[1].value;
            if (str.EndsWith(cmp))
            {
                returnValue = MethodArgStack.Int32(1);
            }
            else
            {
                returnValue = MethodArgStack.Int32(0);
            }
        }

        private void Func2InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Func");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(Stack[1]); //Is this needed?
            returnValue = RunMethod(toCallMethod, toCallMethod.File, parms);
        }

        private void Func1InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Func");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            returnValue = RunMethod(toCallMethod, toCallMethod.File, parms);
        }

        private void FuncCtorImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var theFunc = Stack[0];
            var obj = Stack[1];
            var methodPtr = Stack[2];
            if (theFunc.type != StackItemType.Object) throw new InvalidOperationException();
            if (methodPtr.type != StackItemType.Object) throw new InvalidOperationException();

            //store the method in a secret field
            var d = (int)theFunc.value;
            Objects.ObjectRefs[d].Fields.Add("__internal_method", methodPtr);
        }

        private void Type_GetMethod(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var obj = Objects.ObjectRefs[(int)Stack[0].value];
            var type = (DotNetType)obj.Fields["internal__type"].value;
            var methodName = (string)Stack[1].value;

            foreach (var item in type.Methods)
            {
                if (item.Name == methodName)
                {
                    var field2 = CreateType("System.Reflection", "MethodInfo");
                    WriteStringToType(field2, "_internalName", methodName);
                    Objects.ObjectRefs[(int)field2.value].Fields.Add("internal__type", new MethodArgStack() { type = StackItemType.None, value = type });
                    returnValue = field2;
                    return;
                }
            }
            returnValue = MethodArgStack.Null();
        }

        private void String_IndexOf(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = (string)Stack[0].value;
            var c = (char)(int)Stack[1].value;
            returnValue = MethodArgStack.Int32(str.IndexOf(c));
        }

        private void List_AddItem(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var list = Stack[0];
            var index = Stack[1];
            var val = Stack[2];
            var list2 = Objects.ObjectRefs[Objects.GetIndexFromRef(list)];
            var arr = list2.Fields["_items"];
            var arrIdx = Arrays.GetIndexFromRef(arr);
            var arr2 = Arrays.ArrayRefs[arrIdx].Items;
            //Resize the array
            Array.Resize(ref arr2, arr2.Length + 1);
            //Set the value
            arr2[(int)index.value] = val;

            //Save everything
            Arrays.ArrayRefs[arrIdx].Items = arr2;
            list2.Fields["_items"].value = arrIdx;
            Objects.ObjectRefs[Objects.GetIndexFromRef(list)] = list2;
        }

        private void FileExists(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            throw new NotImplementedException();
        }

        private void InternalGetField(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var val = Stack[0];
            var type = (DotNetType)Objects.ObjectRefs[(int)val.value].Fields["internal__type"].value;

            DotNetField f = null;
            foreach (var item in type.Fields)
            {
                if (item.Name == (string)Stack[Stack.Length - 1].value)
                {
                    f = item;
                    break;
                }
            }
            if (f == null) throw new InvalidOperationException();

            var field2 = CreateType("System.Reflection", "FieldInfo");
            WriteStringToType(field2, "_internalName", f.Name);
            Objects.ObjectRefs[(int)field2.value].Fields.Add("ParrentObjectID", new MethodArgStack() { type = StackItemType.Object, value = type });
            Objects.ObjectRefs[(int)field2.value].Fields.Add("InternalField", new MethodArgStack() { type = StackItemType.Object, value = f });

            returnValue = field2;
        }


        private void InternalGetFields(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var val = Stack[0];
            var type = (DotNetType)Objects.ObjectRefs[(int)val.value].Fields["internal__type"].value;

            var array = Arrays.AllocArray((int)type.Fields.Count);
            int i = 0;
            foreach (var item in type.Fields)
            {
                var field2 = CreateType("System.Reflection", "FieldInfo");
                WriteStringToType(field2, "_internalName", item.Name);
                Objects.ObjectRefs[(int)field2.value].Fields.Add("ParrentObjectID", new MethodArgStack() { type = StackItemType.Object, value = type });
                Objects.ObjectRefs[(int)field2.value].Fields.Add("InternalField", new MethodArgStack() { type = StackItemType.Object, value = item });
                Arrays.ArrayRefs[array.Index].Items[i] = field2;
                i++;
            }
            returnValue = new MethodArgStack() { type = StackItemType.Array, value = array.Index };
        }

        private void Boolean_GetValue(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var val = Stack[0];
            returnValue = val;
        }
        #region Actions
        private void Action5InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            parms.Add(Stack[1]);
            parms.Add(Stack[2]);
            parms.Add(Stack[3]);
            parms.Add(Stack[4]);
            parms.Add(Stack[5]);
            RunMethod(toCallMethod, toCallMethod.File, parms);
            //stack.RemoveRange(stack.Count - 5, 5);
        }
        private void Action4InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            parms.Add(Stack[1]);
            parms.Add(Stack[2]);
            parms.Add(Stack[3]);
            parms.Add(Stack[4]);
            RunMethod(toCallMethod, toCallMethod.File, parms);
            //stack.RemoveRange(stack.Count - 4, 4);
        }
        private void Action3InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            parms.Add(Stack[1]);
            parms.Add(Stack[2]);
            parms.Add(Stack[3]);
            RunMethod(toCallMethod, toCallMethod.File, parms);
            //stack.RemoveRange(stack.Count - 3, 3);
        }
        private void Action2InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            parms.Add(Stack[1]);
            parms.Add(Stack[2]);
            RunMethod(toCallMethod, toCallMethod.File, parms);
            //stack.RemoveRange(stack.Count - 2, 2);
        }
        private void Action1InvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.Object) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)Objects.ObjectRefs[(int)toCall.value].Fields["PtrToMethod"].value;
            var parms = new CustomList<MethodArgStack>();
            parms.Add(obj); //Is this needed?
            parms.Add(Stack[1]);
            RunMethod(toCallMethod, toCallMethod.File, parms);
        }
        private void ActionInvokeImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            MethodArgStack obj = Stack[0];
            var d = Objects.ObjectRefs[(int)obj.value];
            if (!d.Fields.ContainsKey("__internal_method")) throw new Exception("Invaild instance of Action");
            var toCall = d.Fields["__internal_method"];
            if (toCall.type != StackItemType.MethodPtr) throw new InvalidOperationException();

            var toCallMethod = (DotNetMethod)toCall.value;
            RunMethod(toCallMethod, toCallMethod.File, new CustomList<MethodArgStack>());
        }
        private void ActionCtorImpl(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            // do nothing
            var theAction = Stack[0];
            var obj = Stack[1];
            var methodPtr = Stack[2];
            if (theAction.type != StackItemType.Object) throw new InvalidOperationException();
            if (methodPtr.type != StackItemType.Object) throw new InvalidOperationException();

            //store the method in a secret field
            var d = (int)theAction.value;
            Objects.ObjectRefs[d].Fields.Add("__internal_method", methodPtr);
        }
        #endregion
        private void ListAddItem(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var list = Stack[Stack.Length - 2];
            var item = Stack[Stack.Length - 1];

            ;
        }
        #region Reflection
        private void GetAssemblyFromType(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var type = Stack[Stack.Length - 1];
            if (type.type != StackItemType.Object) throw new InvalidOperationException();
            var dotNetType = type.ObjectType;

            var assembly = CreateType("System.Reflection", "Assembly");


            returnValue = assembly;
        }
        private void GetTypeFromReference(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var re = Stack[Stack.Length - 1];
            if (re.type != StackItemType.Object) throw new InvalidOperationException();

            var type = CreateType("System", "Type");
            var typeToRead = CreateType(ReadStringFromType(re, "_namespace"), ReadStringFromType(re, "_name"));
            WriteStringToType(type, "internal__fullname", typeToRead.ObjectType.FullName);
            WriteStringToType(type, "internal__name", typeToRead.ObjectType.Name);
            WriteStringToType(type, "internal__namespace", typeToRead.ObjectType.NameSpace);

            Objects.ObjectRefs[(int)type.value].Fields.Add("internal__type", new MethodArgStack() { type = StackItemType.None, value = typeToRead.ObjectType });

            returnValue = type;
        }
        private void GetObjType(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var obj = Stack[Stack.Length - 1];

            //TODO: Remove this hack
            if (obj.type != StackItemType.Object) obj = Stack[0];
            if (obj.type != StackItemType.Object) throw new InvalidOperationException();
            //Create the type object

            MethodArgStack a = CreateType("System", "Type");
            WriteStringToType(a, "internal__fullname", obj.ObjectType.FullName);
            WriteStringToType(a, "internal__name", obj.ObjectType.Name);
            WriteStringToType(a, "internal__namespace", obj.ObjectType.NameSpace);

            Objects.ObjectRefs[(int)obj.value].Fields.Add("internal__type", new MethodArgStack() { type = StackItemType.None, value = obj.ObjectType });

            returnValue = a;
        }
        #endregion
        #region Making custom internal methods
        /// <summary>
        /// Represents a custom internal method.
        /// </summary>
        /// <param name="Stack">The CLR stack.</param>
        /// <returns>Return value. Return null if function returns void.</returns>
        public delegate void ClrCustomInternalMethod(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method);
        /// <summary>
        /// Registers a custom internal method.
        /// </summary>
        /// <param name="name">The name of the internal method</param>
        /// <param name="method">The method.</param>
        public void RegisterCustomInternalMethod(string name, ClrCustomInternalMethod method)
        {
            if (CustomInternalMethods.ContainsKey(name))
                throw new Exception("Internal method already registered!");

            CustomInternalMethods.Add(name, method);
        }
        #endregion
        #region Implementation for various ToString methods
        private void Internal__System_Char_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var c = (char)Stack[Stack.Length - 1].value;
            returnValue = MethodArgStack.String(c.ToString());
        }

        private void Internal__System_String_get_Chars_1(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = (string)Stack[0].value;
            var index = (int)Stack[1].value;

            returnValue = new MethodArgStack() { type = StackItemType.Char, value = str[index] };
        }

        private void Internal__System_String_Get_Length(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var stringToRead = Stack[Stack.Length - 1];
            var str = (string)stringToRead.value;
            returnValue = new MethodArgStack() { type = StackItemType.Int32, value = str.Length };
        }
        private void Internal__System_Int64_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((long)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_UInt64_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((ulong)(long)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_UInt32_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((uint)(int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_Int32_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_Int16_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_UInt16_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((ushort)(int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void Internal__System_SByte_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((sbyte)(int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        private void InternalMethod_Byte_ToString(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = ((int)Stack[Stack.Length - 1].value).ToString();
            returnValue = str;
        }
        #endregion
        #region Console class
        private void InternalMethod_Console_ReadLine(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            string line = Console.ReadLine();

            var str = new MethodArgStack();
            str.type = StackItemType.String;
            str.value = line;
            returnValue = str;
        }
        private void InternalMethod_Console_Writeline(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            if (Stack.Length == 0)
            {
                Console.WriteLine();
            }
            else
            {
                var s = Stack[0];
                string val = "<NULL>";
                if (s.type == StackItemType.Int32)
                {
                    val = ((int)s.value).ToString();
                }
                else if (s.type == StackItemType.Int64)
                {
                    val = ((long)s.value).ToString();
                }
                else if (s.type == StackItemType.String)
                {
                    val = (string)s.value;
                }
                Console.WriteLine(val);
            }
        }
        private void InternalMethod_Console_Write(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var s = Stack[0];
            string val = "<NULL>";
            if (s.type == StackItemType.Int32)
            {
                val = ((int)s.value).ToString();
            }
            else if (s.type == StackItemType.Int64)
            {
                val = ((long)s.value).ToString();
            }
            else if (s.type == StackItemType.String)
            {
                val = (string)s.value;
            }
            Console.Write(val);
        }
        private void InternalMethod_Console_Clear(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            Console.Clear();
        }
        #endregion
        #region String class
        private void InternalMethod_String_Concat(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            string returnVal = "";
            for (int i = Stack.Length - method.AmountOfParms; i < Stack.Length; i++)
            {
                if (Stack[i].type != StackItemType.String)
                {
                    clrError("Fatal error see InternalMethod_String_Concat method (stack corruption)", "Internal CLR error");
                    return;
                }
                returnVal += (string)Stack[i].value;
            }

            returnValue = MethodArgStack.String((string)returnVal);
        }
        private void InternalMethod_String_op_Equality(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var a = Stack[0].value;
            var b = Stack[1].value;
            string first;
            string second;
            if (a is string)
            {
                first = (string)a;
            }
            else if (a is int)
            {
                first = ((char)(int)a).ToString();
            }
            else
            {
                returnValue = MethodArgStack.Int32(0);
                return;
            }

            if (b is string)
            {
                second = (string)b;
            }
            else if (b is int)
            {
                second = ((char)(int)b).ToString();
            }
            else
            {
                returnValue = MethodArgStack.Int32(0);
                return;
            }

            if (first == second)
            {
                returnValue = MethodArgStack.Int32(1);
            }
            else
            {
                returnValue = MethodArgStack.Int32(0);
            }
        }
        private void String_ToUpper(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = Stack[0];
            if (str.type != StackItemType.String) throw new InvalidOperationException();
            var oldVal = (string)str.value;
            returnValue = MethodArgStack.String(oldVal.ToUpper());
        }
        private void String_ToLower(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var str = Stack[0];
            if (str.type != StackItemType.String) throw new InvalidOperationException();
            var oldVal = (string)str.value;
            returnValue = MethodArgStack.String(oldVal.ToLower());
        }

        #endregion
        #region Misc
        private void DebuggerBreak(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            Debugger.Break();
        }
        #endregion
    }
}
