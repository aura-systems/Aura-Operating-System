using LibDotNetParser;
using LibDotNetParser.CILApi;
using LibDotNetParser.CILApi.IL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace libDotNetClr
{
    /// <summary>
    /// .NET code executor
    /// </summary>
    public partial class DotNetClr
    {
        private DotNetFile file;
        /// <summary>
        /// Is the CLR running?
        /// </summary>
        private bool Running = false;
        /// <summary>
        /// Loaded DLLS
        /// </summary>
        private Dictionary<string, DotNetFile> dlls = new Dictionary<string, DotNetFile>();
        /// <summary>
        /// Callstack
        /// </summary>
        private List<CallStackItem> CallStack = new List<CallStackItem>();
        /// <summary>
        /// List of custom internal methods
        /// </summary>
        private Dictionary<string, ClrCustomInternalMethod> CustomInternalMethods = new Dictionary<string, ClrCustomInternalMethod>();
        /// <summary>
        /// Should debug messages be printed to the console?
        /// </summary>
        public bool ShouldPrintDebugMessages { get; set; }

        public delegate byte[] AssemblyResolveCallback(string dll);
        private AssemblyResolveCallback _Cb;
        public DotNetClr(DotNetFile exe)
        {
            if (exe == null)
            {
                throw new ArgumentException(nameof(exe));
            }
            Init(exe);
        }
        private void Init(DotNetFile p)
        {
            file = p;
            dlls.Clear();
            dlls.Add("main_exe", p);

            RegisterAllInternalMethods();
        }

        /// <summary>
        /// Registers the callback for resolving assemblies
        /// </summary>
        /// <param name="cb"></param>
        public void RegisterResolveCallBack(AssemblyResolveCallback cb)
        {
            _Cb = cb;
        }

        /// <summary>
        /// Runs the entry point
        /// </summary>
        /// <param name="args">String array of arguments</param>
        public void Start(string[] args = null)
        {
            if (_Cb == null)
            {
                clrError("RegisterResolveCallBack() must be called to be able to start the CLR", "Internal Error");
                return;
            }
            try
            {
                if (file.EntryPoint == null)
                {
                    clrError("The entry point was not found.", "System.EntryPointNotFoundException");
                    file = null;
                    return;
                }
            }
            catch (Exception x)
            {
                clrError("The entry point was not found. Internal error: " + x.Message, "System.EntryPointNotFoundException");
                file = null;
                return;
            }
            InitAssembly(file, true);
            if (ShouldPrintDebugMessages)
            {
                Console.WriteLine();
                PrintColor("Jumping to entry point", ConsoleColor.DarkMagenta);
            }

            Running = true;

            //Run the entry point
            var Startparams = new CustomList<MethodArgStack>();
            if (args != null)
            {
                MethodArgStack[] itms = new MethodArgStack[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    itms[i] = MethodArgStack.String(args[i]);
                }

                var array = Arrays.AllocArray(itms.Length);
                array.Items = itms;
                Startparams.Add(new MethodArgStack() { type = StackItemType.Array, value = array.Index });
            }
            CallStack.Clear();
            RunMethod(file.EntryPoint, file, Startparams);
        }
        private void InitAssembly(DotNetFile file, bool InitCorLib)
        {
            if (InitCorLib)
                ResolveDLL("System.Private.CoreLib"); //Always resolve mscorlib, incase the exe uses .net core

            //Resolve all of the DLLS
            foreach (var item in file.Backend.Tabels.AssemblyRefTabel)
            {
                var fileName = file.Backend.ClrStringsStream.GetByOffset(item.Name);
                ResolveDLL(fileName);
            }
            Running = true;

            //Call all static contructors
            foreach (var t in file.Types)
            {
                foreach (var m in t.Methods)
                {
                    if (m.Name == ".cctor" && m.IsStatic)
                    {
                        if (ShouldPrintDebugMessages)
                        {
                            Console.WriteLine("Creating " + t.FullName + "." + m.Name);
                        }

                        RunMethod(m, file, new CustomList<MethodArgStack>(), false);
                    }
                }
            }
        }
        private void ResolveDLL(string fileName)
        {
            string fullPath = "";
            if (dlls.ContainsKey(fileName))
            {
                //already loaded
                return;
            }

            var assembly = _Cb(fileName);

            if (assembly != null)
            {
                var file2 = new DotNetFile(assembly);
                dlls.Add(fileName, file2);
                InitAssembly(file2, false);
                PrintColor("[OK] Loaded assembly: " + fileName, ConsoleColor.Green);
            }
            else
            {
                PrintColor("[ERROR] Load failed: " + fileName, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Runs a method
        /// </summary>
        /// <param name="m">The method</param>
        /// <param name="file">The file of the method</param>
        /// <param name="oldStack">Old stack</param>
        /// <returns>Returns the return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MethodArgStack RunMethod(DotNetMethod m, DotNetFile file, CustomList<MethodArgStack> parms, bool addToCallStack = true)
        {
            CustomList<MethodArgStack> stack = new CustomList<MethodArgStack>(m.MaxStackSize);
            if (!Running)
                return null;
            #region Internal methods
            //Make sure that RVA is not zero. If its zero, than its extern
            if (m.IsInternalCall | m.IsImplementedByRuntime)
            {
                string properName = m.Name;
                if (m.IsImplementedByRuntime)
                {
                    properName = m.Parent.FullName.Replace(".", "_") + "." + m.Name + "_impl";
                }
                foreach (var item in CustomInternalMethods)
                {
                    if (item.Key == properName)
                    {
                        MethodArgStack a = null;
                        item.Value.Invoke(parms.ToArray(), ref a, m);

                        //Don't forget to remove item parms
                        if (m.AmountOfParms == 0)
                        {
                            //no need to do anything
                        }
                        else
                        {
                            int StartParmIndex = -1;
                            int EndParmIndex = -1;
                            int paramsRead = 0;
                            bool foundEndIndex = false;
                            if (m.AmountOfParms > 0)
                            {
                                var endParam = m.SignatureInfo.Params[m.SignatureInfo.Params.Count - 1];
                                for (int i4 = stack.Count - 1; i4 >= 0; i4--)
                                {
                                    var stackitm = stack[i4];
                                    if (stackitm.type == endParam.type | stackitm.type == StackItemType.ldnull && StartParmIndex == -1 && !foundEndIndex)
                                    {
                                        if (endParam.IsClass)
                                        {
                                            if (endParam.ClassType != stackitm.ObjectType)
                                            {
                                                continue;
                                            }
                                        }
                                        EndParmIndex = i4;
                                        foundEndIndex = true;
                                        if (m.AmountOfParms == 1)
                                        {
                                            StartParmIndex = i4;
                                            break;
                                        }
                                    }
                                    if (EndParmIndex != -1 && StartParmIndex == -1)
                                    {
                                        paramsRead++;
                                    }
                                    if (EndParmIndex != -1 && paramsRead >= m.AmountOfParms)
                                    {
                                        StartParmIndex = i4;
                                        break;
                                    }
                                }
                            }
                            if (StartParmIndex == -1)
                            {
                                if (m.AmountOfParms == 1)
                                    return a;
                                else
                                {
                                    //clrError("Failed to find parameters after exectuting an internal method!", "internal CLR error");
                                    return a;
                                }
                            }

                            if (m.AmountOfParms == 1)
                            {
                                stack.RemoveAt(StartParmIndex);
                            }
                            else
                            {
                                stack.RemoveRange(StartParmIndex, EndParmIndex - StartParmIndex + 1);
                            }
                        }


                        if (m.Name.Contains("ToString"))
                        {
                            stack.RemoveAt(stack.Count - 1);
                        }
                        return a;
                    }
                }

                clrError("Cannot find internal method: " + properName, "internal CLR error");
                return null;
            }
            else if (m.RVA == 0)
            {
                clrError($"Cannot find the method body for {m.Parent.FullName}.{m.Name}", "System.Exception");
                return null;
            }
            #endregion
            MethodArgStack[] Localstack = new MethodArgStack[256];
            if (addToCallStack)
            {
                //Add this method to the callstack.
                CallStack.Add(new CallStackItem() { method = m });
            }

            //Now decompile the code and run it
            var decompiler = new IlDecompiler(m);
            foreach (var item in dlls)
            {
                decompiler.AddReference(item.Value);
            }
            var code = decompiler.Decompile();
            int i;
            for (i = 0; i < code.Length; i++)
            {
                if (!Running)
                    return null;
                var item = code[i];
                switch ((OpCodes.OpCodesList)item.OpCode)
                {
                    case OpCodes.OpCodesList.Add:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Add));
                            break;
                        }
                    case OpCodes.OpCodesList.Add_Ovf:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Add_Ovf_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.And:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            if (a.type != StackItemType.Int32 || b.type != StackItemType.Int32)
                            {
                                clrError($"Error in {item.OpCodeName} opcode: type is not int32", "Internal CLR error");
                                return null;
                            }

                            var numb1 = (int)a.value;
                            var numb2 = (int)b.value;
                            var result = numb1 & numb2;
                            stack.Add(MethodArgStack.Int32(result));
                            break;
                        }
                    case OpCodes.OpCodesList.Arglist:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Beq:
                    case OpCodes.OpCodesList.Beq_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.Equal, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Bge:
                    case OpCodes.OpCodesList.Bge_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.GreaterThanEqual, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Bge_Un:
                    case OpCodes.OpCodesList.Bge_Un_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.GreaterThan, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Bgt:
                    case OpCodes.OpCodesList.Bgt_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.GreaterThan, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Bgt_Un:
                    case OpCodes.OpCodesList.Bgt_Un_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.GreaterThan, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Ble:
                    case OpCodes.OpCodesList.Ble_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.LessThanEqual, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Ble_Un:
                    case OpCodes.OpCodesList.Ble_Un_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.LessThan, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Blt:
                    case OpCodes.OpCodesList.Blt_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.LessThan, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Blt_Un:
                    case OpCodes.OpCodesList.Blt_Un_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.NotEqual, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Bne_Un:
                    case OpCodes.OpCodesList.Bne_Un_S:
                        {
                            BranchWithOp(item, stack, decompiler, MathOperations.Operation.NotEqual, ref i);
                            break;
                        }
                    case OpCodes.OpCodesList.Box:
                        {
                        var a = stack.Pop();
                  
                        stack.Add(new MethodArgStack() { type = StackItemType.ObjectRef, value = a.value });
                        break;
                    }
                    case OpCodes.OpCodesList.Br:
                    case OpCodes.OpCodesList.Br_S:
                        {
                            //find the ILInstruction that is in this position
                            int i2 = item.Position + (int)item.Operand + 1;
                            ILInstruction inst = decompiler.GetInstructionAtOffset(i2, -1);

                            if (inst == null)
                                throw new Exception("Attempt to branch to null");
                            i = inst.RelPosition - 1;
                            break;
                        }
                    case OpCodes.OpCodesList.Break:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Brfalse:
                    case OpCodes.OpCodesList.Brfalse_S:
                        {
                            if (stack.Count == 0)
                            {
                                clrError("Do not know if I should branch, because there is nothing on the stack. Instruction: brfalse.s", "Internal");
                                return null;
                            }
                            var s = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            bool exec = false;
                            if (s.value == null)
                                exec = true;
                            else
                            {
                                try
                                {
                                    if ((int)s.value == 0)
                                        exec = true;
                                }
                                catch { }
                            }
                            if (exec)
                            {
                                // find the ILInstruction that is in this position
                                int i2 = item.Position + (int)item.Operand + 1;
                                ILInstruction inst = decompiler.GetInstructionAtOffset(i2, -1);

                                if (inst == null)
                                    throw new Exception("Attempt to branch to null");
                                i = inst.RelPosition - 1;
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Brtrue:
                    case OpCodes.OpCodesList.Brtrue_S:
                        {
                            if (stack[stack.Count - 1].value == null)
                            {
                                stack.RemoveAt(stack.Count - 1);
                                continue;
                            }
                            bool exec = false;
                            if (stack[stack.Count - 1].type != StackItemType.Int32)
                            {
                                if (stack[stack.Count - 1].type != StackItemType.ldnull)
                                {
                                    exec = true;
                                }
                            }
                            else
                            {
                                if ((int)stack[stack.Count - 1].value == 1)
                                {
                                    exec = true;
                                }
                            }
                            if (exec)
                            {
                                // find the ILInstruction that is in this position
                                int i2 = item.Position + (int)item.Operand + 1;
                                ILInstruction inst = decompiler.GetInstructionAtOffset(i2, -1);

                                if (inst == null)
                                    throw new Exception("Attempt to branch to null");
                                stack.RemoveAt(stack.Count - 1);
                                i = inst.RelPosition - 1;
                            }
                            else
                            {
                                stack.RemoveAt(stack.Count - 1);
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Call:
                        {
                            var call = (InlineMethodOperandData)item.Operand;
                            if (!InternalCallMethod(call, m, addToCallStack, false, false, stack))
                            {
                                return null;
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Calli:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Callvirt:
                        {
                            var call = (InlineMethodOperandData)item.Operand;
                            //todo: why is callvirt false?
                            if (!InternalCallMethod(call, m, addToCallStack, true, false, stack))
                            {
                                return null;
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Castclass:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ceq:
                        {
                            if (stack.Count < 2)
                            {
                                clrError($"There has to be 2 or more items on the stack for {item.OpCodeName} instruction to work!", "Internal CLR error");
                                return null;
                            }

                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Equal));
                            break;
                        }
                    case OpCodes.OpCodesList.Cgt:
                        {
                            if (stack.Count < 2)
                            {
                                clrError($"There has to be 2 or more items on the stack for {item.OpCodeName} instruction to work!", "Internal CLR error");
                                return null;
                            }

                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.GreaterThan));
                            break;
                        }
                    case OpCodes.OpCodesList.Cgt_Un:
                        {
                            if (stack.Count < 2)
                            {
                                clrError($"There has to be 2 or more items on the stack for {item.OpCodeName} instruction to work!", "Internal CLR error");
                                return null;
                            }

                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.GreaterThan));
                            break;
                        }
                    case OpCodes.OpCodesList.Ckfinite:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Clt:
                        {
                            if (stack.Count < 2)
                            {
                                clrError($"There has to be 2 or more items on the stack for {item.OpCodeName} instruction to work!", "Internal CLR error");
                                return null;
                            }

                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.LessThan));
                            break;
                        }
                    case OpCodes.OpCodesList.Clt_Un:
                        {
                            if (stack.Count < 2)
                            {
                                clrError($"There has to be 2 or more items on the stack for {item.OpCodeName} instruction to work!", "Internal CLR error");
                                return null;
                            }

                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.LessThan));
                            break;
                        }
                    case OpCodes.OpCodesList.Constrained:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_I:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_I1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_I2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_I4:
                        {
                            if (ThrowIfStackIsZero(stack, "conv.i4")) return null;

                            var numb = stack[stack.Count - 1];
                            if (numb.type == StackItemType.Int32)
                            {
                                //We don't need to do anything because it's already int32
                            }
                            else if (numb.type == StackItemType.Float32)
                            {
                                stack.RemoveAt(stack.Count - 1);
                                stack.Add(MethodArgStack.Int32((int)(float)numb.value));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Conv_I8:
                        {
                            var itm = stack[stack.Count - 1];
                            if (itm.type == StackItemType.Int32)
                            {
                                itm.value = (long)(int)itm.value;
                                itm.type = StackItemType.Int64;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }

                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(itm);
                            break;
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I1_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I2_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I4_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_I8_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U1_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U2_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U4_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_Ovf_U8_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_R_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_R4:
                        {
                            if (ThrowIfStackIsZero(stack, "conv.r4")) return null;

                            var numb = stack[stack.Count - 1];
                            if (numb.type == StackItemType.Int32)
                            {
                                stack.RemoveAt(stack.Count - 1);
                                stack.Add(MethodArgStack.Float32((int)numb.value));
                            }
                            else if (numb.type == StackItemType.Float32)
                            {
                                //We don't need to do anything because it's already float32
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Conv_R8:
                        {
                            if (ThrowIfStackIsZero(stack, "conv.u8")) return null;

                            var stk = stack.Pop();
                            if (stk.type == StackItemType.Int32)
                            {
                                stack.Add(MethodArgStack.UInt64((ulong)(int)stk.value));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Conv_U:
                        {
                            if (ThrowIfStackIsZero(stack, "conv.u8")) return null;

                            var stk = stack.Pop();

                            //Convert uint to uint pointer
                            throw new NotImplementedException("Converting uint to uint* not yet supported");
                        }
                    case OpCodes.OpCodesList.Conv_U1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_U2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_U4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Conv_U8:
                        {
                            if (ThrowIfStackIsZero(stack, "conv.u8")) return null;

                            var stk = stack.Pop();
                            if (stk.type == StackItemType.Int32)
                            {
                                stack.Add(MethodArgStack.UInt64((ulong)(int)stk.value));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Cpblk:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Cpobj:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Div:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Divide));
                            break;
                        }
                    case OpCodes.OpCodesList.Div_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Dup:
                        {
                            stack.Add(stack[stack.Count - 1]);
                            break;
                        }
                    case OpCodes.OpCodesList.Endfilter:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Endfinally:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Initblk:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Initobj:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Isinst:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Jmp:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldarg:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldarg_0:
                        {
                            if (parms.Count == 0)
                                continue;
                            stack.Add(parms[0]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarg_1:
                        {
                            if (parms.Count == 0)
                                continue;
                            stack.Add(parms[1]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarg_2:
                        {
                            if (parms.Count == 0)
                                continue;
                            stack.Add(parms[2]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarg_3:
                        {
                            if (parms.Count == 0)
                                continue;
                            stack.Add(parms[3]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarg_S:
                        {
                            if (parms.Count == 0)
                                continue;

                            stack.Add(parms[(byte)item.Operand]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarga:
                        {
                            if (parms.Count == 0)
                                continue;

                            stack.Add(parms[(byte)item.Operand]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldarga_S:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldc_I4:
                        {
                            //Puts an int32 onto the arg stack
                            stack.Add(MethodArgStack.Int32((int)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_0:
                        {
                            //Puts an 0 onto the arg stack
                            stack.Add(MethodArgStack.Int32(0));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_1:
                        {
                            //Puts an int32 with value 1 onto the arg stack
                            stack.Add(MethodArgStack.Int32(1));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_2:
                        {
                            //Puts an int32 with value 2 onto the arg stack
                            stack.Add(MethodArgStack.Int32(2));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_3:
                        {
                            //Puts an int32 with value 3 onto the arg stack
                            stack.Add(MethodArgStack.Int32(3));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_4:
                        {
                            //Puts an int32 with value 4 onto the arg stack
                            stack.Add(MethodArgStack.Int32(4));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_5:
                        {
                            //Puts an int32 with value 5 onto the arg stack
                            stack.Add(MethodArgStack.Int32(5));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_6:
                        {
                            //Puts an int32 with value 6 onto the arg stack
                            stack.Add(MethodArgStack.Int32(6));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_7:
                        {
                            //Puts an int32 with value 7 onto the arg stack
                            stack.Add(MethodArgStack.Int32(7));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_8:
                        {
                            //Puts an int32 with value 8 onto the arg stack
                            stack.Add(MethodArgStack.Int32(8));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_M1:
                        {
                            //Puts an int32 with value -1 onto the arg stack
                            stack.Add(MethodArgStack.Int32(-1));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I4_S:
                        {
                            //Push an int32 onto the stack
                            stack.Add(MethodArgStack.Int32((int)(sbyte)(byte)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_I8:
                        {
                            //Push int64
                            stack.Add(MethodArgStack.Int64((long)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_R4:
                        {
                            //Puts an float32 with value onto the arg stack
                            stack.Add(MethodArgStack.Float32((float)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldc_R8:
                        {
                            //Puts an float64 with value onto the arg stack
                            stack.Add(MethodArgStack.Float64((double)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldelem:
                        {
                            var index = stack[stack.Count - 1];
                            var array = stack[stack.Count - 2];
                            if (array.type != StackItemType.Array)
                            {
                                clrError("Expected array, but got something else. Fault instruction name: ldelem.ref", "Internal CLR error"); return null;
                            }
                            if (index.type != StackItemType.Int32)
                            {
                                clrError("Expected Int32, but got something else. Fault instruction name: ldelem.ref", "Internal CLR error");
                                return null;
                            }
                            var idx = (int)index.value;
                            stack.RemoveAt(stack.Count - 1); //Remove index
                            stack.RemoveAt(stack.Count - 1); //Remove array

                            var i2 = Arrays.GetIndexFromRef(array);
                            stack.Add(Arrays.ArrayRefs[i2].Items[idx]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldelem_I:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_I1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_I2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_I4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_I8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_R4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_R8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_Ref:
                        {
                            var index = stack[stack.Count - 1];
                            var array = stack[stack.Count - 2];
                            if (array.type != StackItemType.Array)
                            {
                                clrError("Expected array, but got something else. Fault instruction name: ldelem.ref", "Internal CLR error"); return null;
                            }
                            if (index.type != StackItemType.Int32)
                            {
                                clrError("Expected Int32, but got something else. Fault instruction name: ldelem.ref", "Internal CLR error");
                                return null;
                            }
                            var idx = (int)index.value;
                            stack.RemoveAt(stack.Count - 1); //Remove index
                            stack.RemoveAt(stack.Count - 1); //Remove array

                            stack.Add(Arrays.ArrayRefs[Arrays.GetIndexFromRef(array)].Items[idx]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldelem_U1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_U2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelem_U4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldelema:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldfld:
                        {
                            //read value from field.
                            FieldInfo info = item.Operand as FieldInfo;
                            DotNetField f2 = null;
                            foreach (var f in m.Parent.File.Types)
                            {
                                foreach (var tttt in f.Fields)
                                {
                                    if (tttt.IndexInTabel == info.IndexInTabel && tttt.Name == info.Name)
                                    {
                                        f2 = tttt;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(info.Class) && f2 == null)
                            {
                                throw new Exception("Could not find the field");
                            }
                            if (f2 == null)
                            {
                                foreach (var dll in dlls)
                                {
                                    foreach (var type in dll.Value.Types)
                                    {
                                        foreach (var f in type.Fields)
                                        {
                                            if (f.Name == info.Name && type.Name == info.Class && type.NameSpace == info.Namespace)
                                            {
                                                f2 = f;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (f2 == null)
                            {
                                clrError("Failed to resolve field for writing.", "");
                                return null;
                            }
                            MethodArgStack obj = stack[stack.Count - 1];

                            if (obj == null)
                            {
                                clrError("Object to read from not found!", "CLR internal error");
                                return null;
                            }
                            if (obj.type == StackItemType.ldnull)
                            {
                                clrError($"ldfld instruction: Attempted to write to field {f2.Name} in type {f2.ParrentType.FullName}, however the instance of the type is null", "System.NullReferenceException");
                                return null;
                            }

                            stack.RemoveAt(stack.Count - 1);
                            var data = Objects.ObjectRefs[(int)obj.value];
                            if (data.Fields.ContainsKey(f2.Name))
                            {
                                stack.Add(data.Fields[f2.Name]);
                            }
                            else
                            {
                                clrError("Reading from non existant field on object " + obj.ObjectType.FullName + ", field name is " + f2.Name, "CLR internal error");
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Ldflda:
                        {
                            var obj = stack.Pop();
                            var type = obj.ObjectType;
                            var fld = (FieldInfo)item.Operand;
                            if (fld.Name == null)
                            {
                                throw new NotImplementedException();
                            }
                            DotNetField theField = null;
                            int idx = 0;
                            foreach (var f in type.Fields)
                            {
                                if (f.Name == fld.Name)
                                {
                                    theField = f;
                                }
                                idx++;
                            }
                            if (theField == null)
                                throw new NotImplementedException();


                            stack.Add(MethodArgStack.Int32(idx));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldftn:
                        {
                            var call = item.Operand as InlineMethodOperandData; //a method
                            var obj2 = stack[stack.Count - 1]; //the compiler generated object to call the method on
                            DotNetMethod m2 = null;

                            if (call.RVA == 0) throw new NotImplementedException();
                            else
                            {
                                foreach (var item2 in dlls)
                                {
                                    foreach (var item3 in item2.Value.Types)
                                    {
                                        foreach (var meth in item3.Methods)
                                        {
                                            var fullName = call.NameSpace + "." + call.ClassName;
                                            if (string.IsNullOrEmpty(call.NameSpace))
                                                fullName = call.ClassName;

                                            if (meth.RVA == call.RVA && meth.Name == call.FunctionName && meth.Signature == call.Signature && meth.Parent.FullName == fullName)
                                            {
                                                m2 = meth;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (m2 == null)
                                {
                                    clrError($"Cannot resolve virtual called method: {call.NameSpace}.{call.ClassName}.{call.FunctionName}(). Function signature is {call.Signature}", "");
                                    return null;
                                }
                            }

                            var ptr = CreateType("System", "IntPtr");
                            Objects.ObjectRefs[(int)ptr.value].Fields.Add("PtrToMethod", new MethodArgStack() { value = m2, type = StackItemType.MethodPtr });
                            stack.Add(ptr);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_I:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_I1:
                        {
                            // throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_I2:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_I4:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_I8:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_R4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldind_R8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldind_Ref:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldind_U1:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_U2:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldind_U4:
                        {
                            //throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                            break;
                        }
                    case OpCodes.OpCodesList.Ldlen:
                        {
                            MethodArgStack array = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(MethodArgStack.Int32(Arrays.ArrayRefs[Arrays.GetIndexFromRef(array)].Length));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloc:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldloc_0:
                        {
                            var oldItem = Localstack[0];
                            stack.Add(oldItem);
                            // Localstack[0] = null;
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloc_1:
                        {
                            var oldItem = Localstack[1];
                            stack.Add(oldItem);
                            // Localstack[0] = null;
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloc_2:
                        {
                            var oldItem = Localstack[2];
                            stack.Add(oldItem);
                            // Localstack[0] = null;
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloc_3:
                        {
                            var oldItem = Localstack[3];
                            stack.Add(oldItem);
                            // Localstack[0] = null;
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloc_S:
                        {
                            var oldItem = Localstack[(byte)item.Operand];
                            stack.Add(oldItem);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldloca:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldloca_S:
                        {
                            var oldItem = Localstack[(byte)item.Operand];

                            if (oldItem == null)
                            {
                                //TODO: Read the method flags to determine how to initialize the local variables
                                //See https://www.codeproject.com/Articles/12585/The-NET-File-Format#Blob
                                //and https://www.codeproject.com/Articles/12585/The-NET-File-Format#Methods

                                Localstack[(byte)item.Operand] = MethodArgStack.Null();
                                stack.Add(Localstack[(byte)item.Operand]);
                            }
                            else
                            {
                                stack.Add(oldItem);
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Ldnull:
                        {
                            stack.Add(MethodArgStack.Null());
                            break;
                        }
                    case OpCodes.OpCodesList.Ldobj:
                        {
                            stack.Add(stack[0]);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldsfld:
                        {
                            //get value from static field
                            DotNetField f2 = null;
                            FieldInfo info = item.Operand as FieldInfo;
                            foreach (var f in m.Parent.File.Types)
                            {
                                foreach (var tttt in f.Fields)
                                {
                                    if (tttt.IndexInTabel == info.IndexInTabel && tttt.Name == info.Name)
                                    {
                                        f2 = tttt;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(info.Class) && f2 == null)
                            {
                                throw new Exception("Could not find the field");
                            }
                            if (f2 == null)
                            {
                                foreach (var dll in dlls)
                                {
                                    foreach (var type in dll.Value.Types)
                                    {
                                        foreach (var f in type.Fields)
                                        {
                                            if (f.Name == info.Name && type.Name == info.Class && type.NameSpace == info.Namespace)
                                            {
                                                f2 = f;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (f2 == null)
                                throw new Exception("Cannot find the static field to read from.");

                            StaticField f3 = null;
                            foreach (var f in StaticFieldHolder.staticFields)
                            {
                                if (f.theField.Name == f2.Name && f.theField.ParrentType.FullName == f2.ParrentType.FullName)
                                {
                                    f3 = f;
                                    break;
                                }
                            }
                            if (f3 == null)
                            {
                                stack.Add(MethodArgStack.ldnull);
                            }
                            else
                            {
                                stack.Add(f3.value);
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Ldsflda:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ldstr:
                        {
                            stack.Add(MethodArgStack.String((string)item.Operand));
                            break;
                        }
                    case OpCodes.OpCodesList.Ldtoken:
                        {
                            var index = (FieldInfo)item.Operand;
                            DotNetType t2 = null;
                            if (index.IsInFieldTabel)
                            {
                                foreach (var t in m.Parent.File.Types)
                                {
                                    if (t.Name == index.Name && t.NameSpace == index.Namespace)
                                    {
                                        t2 = t;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var dll in dlls)
                                {
                                    foreach (var t in dll.Value.Types)
                                    {
                                        if (t.Name == index.Name && t.NameSpace == index.Namespace)
                                        {
                                            t2 = t;
                                        }
                                    }
                                }
                            }
                            if (t2 == null)
                            {
                                clrError("Failed to resolve token. OpCode: ldtoken. Type: " + index.Namespace + "." + index.Name, "Internal CLR error");
                                return null;
                            }

                            var handle = CreateType("System", "RuntimeTypeHandle");
                            WriteStringToType(handle, "_name", t2.Name);
                            WriteStringToType(handle, "_namespace", t2.NameSpace);
                            stack.Add(handle);
                            break;
                        }
                    case OpCodes.OpCodesList.Ldvirtftn:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Leave:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Leave_S:
                        {
                            // find the ILInstruction that is in this position
                            int i2 = item.Position + (int)item.Operand + 1;
                            ILInstruction inst = decompiler.GetInstructionAtOffset(i2, -1);

                            if (inst == null)
                                throw new Exception("Attempt to branch to null");
                            stack.RemoveAt(stack.Count - 1);
                            i = inst.RelPosition - 1;
                            break;
                        }
                    case OpCodes.OpCodesList.Localloc:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Mkrefany:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Mul:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Multiply));
                            break;
                        }
                    case OpCodes.OpCodesList.Mul_Ovf:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Mul_Ovf_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Neg:
                        {
                            var a = stack.Pop();

                            stack.Add(MathOperations.Op(a, MathOperations.Operation.Negate));
                            break;
                        }
                    case OpCodes.OpCodesList.Newarr:
                        {
                            var arrayLen = stack[stack.Count - 1];
                            var array = Arrays.AllocArray((int)arrayLen.value);

                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(new MethodArgStack() { type = StackItemType.Array, value = array.Index });
                            break;
                        }
                    case OpCodes.OpCodesList.Newobj:
                        {
                            var call = (InlineMethodOperandData)item.Operand;
                            //Local/Defined method
                            DotNetMethod m2 = null;
                            foreach (var item2 in dlls)
                            {
                                foreach (var item3 in item2.Value.Types)
                                {
                                    foreach (var meth in item3.Methods)
                                    {
                                        if (meth.RVA == call.RVA && meth.Name == call.FunctionName && meth.Signature == call.Signature && item3.Name == call.ClassName)
                                        {
                                            if (call.ParamListIndex != 0)
                                            {
                                                if (meth.ParamListIndex == call.ParamListIndex)
                                                {
                                                    m2 = meth;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                m2 = meth;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (m2 == null)
                            {
                                clrError($"Cannot find the called constructor: {call.NameSpace}.{call.ClassName}.{call.FunctionName}(). Function signature is {call.Signature}", "CLR internal error");
                                return null;
                            }
                            var a = CreateObject(m2, stack);

                            stack.Add(a);
                            break;
                        }
                    case OpCodes.OpCodesList.Nop:
                        {
                            //Don't do anything
                            break;
                        }
                    case OpCodes.OpCodesList.Not:
                        {
                            var a = stack.Pop();

                            if (a.type != StackItemType.Int32)
                            {
                                clrError($"Error in {item.OpCodeName} opcode: type is not int32", "Internal CLR error");
                                return null;
                            }

                            var numb1 = (int)a.value;
                            var result = ~numb1;
                            stack.Add(MethodArgStack.Int32(result));
                            break;
                        }
                    case OpCodes.OpCodesList.Or:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            if (a.type != StackItemType.Int32 || b.type != StackItemType.Int32)
                            {
                                clrError($"Error in {item.OpCodeName} opcode: type is not int32", "Internal CLR error");
                                return null;
                            }

                            var numb1 = (int)a.value;
                            var numb2 = (int)b.value;
                            var result = numb1 | numb2;
                            stack.Add(MethodArgStack.Int32(result));
                            break;
                        }
                    case OpCodes.OpCodesList.Pop:
                        {
                            stack.Pop();
                            break;
                        }
                    case OpCodes.OpCodesList.Prefix1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix3:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix5:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix6:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefix7:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Prefixref:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Readonly:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Refanytype:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Refanyval:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Rem:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Remainder));
                            break;
                        }
                    case OpCodes.OpCodesList.Rem_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Ret:
                        {
                            //Return from function
                            MethodArgStack a = null;
                            if (stack.Count != 0 && m.HasReturnValue)
                            {
                                a = stack[stack.Count - 1];
                                if (addToCallStack)
                                    CallStack.RemoveAt(CallStack.Count - 1);


                                stack.RemoveAt(stack.Count - 1);
                            }

                            return a;
                        }
                    case OpCodes.OpCodesList.Rethrow:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Shl:
                        {
                            var a = stack.Pop(); //number of bits to shift (always int32)
                            var b = stack.Pop();

                            if (b.type == StackItemType.Int32)
                            {
                                var numb1 = (int)a.value;
                                var numb2 = (int)b.value;
                                var result = numb2 << numb1;
                                stack.Add(MethodArgStack.Int32(result));
                            }
                            else if (b.type == StackItemType.Int64)
                            {
                                var numb1 = (int)a.value;
                                var numb2 = (long)b.value;
                                var result = numb2 << numb1;
                                stack.Add(MethodArgStack.Int64(result));
                            }
                            else if (b.type == StackItemType.UInt64)
                            {
                                var numb1 = (int)a.value;
                                var numb2 = (ulong)b.value;
                                var result = numb2 << numb1;
                                stack.Add(MethodArgStack.UInt64(result));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Shr:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            if (a.type != StackItemType.Int32 || b.type != StackItemType.Int32)
                            {
                                clrError($"Error in {item.OpCodeName} opcode: type is not int32", "Internal CLR error");
                                return null;
                            }

                            var numb1 = (int)a.value;
                            var numb2 = (int)b.value;
                            var result = numb2 >> numb1;
                            stack.Add(MethodArgStack.Int32(result));
                            break;
                        }
                    case OpCodes.OpCodesList.Shr_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Sizeof:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Starg:
                    case OpCodes.OpCodesList.Starg_S:
                        {
                            var val = stack[stack.Count - 1];
                            var a = (byte)item.Operand;
                            if (stack.Count == 1 && a == 1)
                            {
                                stack.Add(val);
                            }
                            else
                            {
                                stack[a] = val;
                            }

                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stelem:
                    case OpCodes.OpCodesList.Stelem_I:
                    case OpCodes.OpCodesList.Stelem_I1:
                    case OpCodes.OpCodesList.Stelem_I2:
                    case OpCodes.OpCodesList.Stelem_I4:
                    case OpCodes.OpCodesList.Stelem_I8:
                    case OpCodes.OpCodesList.Stelem_R4:
                    case OpCodes.OpCodesList.Stelem_R8:
                        {
                            var val = stack[stack.Count - 1];
                            var index = stack[stack.Count - 2];
                            var array = stack[stack.Count - 3];
                            //if (array.type != StackItemType.Array) { clrError("Expected array, but got something else. Fault instruction name: stelem", "Internal CLR error"); return null; }
                            var idx = Arrays.GetIndexFromRef(array);
                            Arrays.ArrayRefs[idx].Items[(int)index.value] = val;

                            stack.RemoveAt(stack.Count - 1); //Remove value
                            stack.RemoveAt(stack.Count - 1); //Remove index
                            stack.RemoveAt(stack.Count - 1); //Remove array ref
                            break;
                        }
                    case OpCodes.OpCodesList.Stelem_Ref:
                        {
                            var val = stack[stack.Count - 1];
                            var index = stack[stack.Count - 2];
                            var array = stack[stack.Count - 3];
                            //if (array.type != StackItemType.Array) { clrError("Expected array, but got something else. Fault instruction name: stelem.ref", "Internal CLR error"); return null; }
                            if (index.type != StackItemType.Int32) { clrError("Expected Int32, but got something else. Fault instruction name: stelem.ref", "Internal CLR error"); return null; }

                            Arrays.ArrayRefs[Arrays.GetIndexFromRef(array)].Items[(int)index.value] = val;

                            stack.RemoveAt(stack.Count - 1); //Remove value
                            stack.RemoveAt(stack.Count - 1); //Remove index
                            stack.RemoveAt(stack.Count - 1); //Remove array ref
                            break;
                        }
                    case OpCodes.OpCodesList.Stfld:
                        {
                            //write value to field.
                            FieldInfo info = item.Operand as FieldInfo;
                            DotNetField f2 = null;
                            foreach (var f in m.Parent.File.Types)
                            {
                                foreach (var tttt in f.Fields)
                                {
                                    if (tttt.IndexInTabel == info.IndexInTabel && tttt.Name == info.Name)
                                    {
                                        f2 = tttt;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(info.Class) && f2 == null)
                            {
                                throw new Exception("Could not find the field");
                            }
                            if (f2 == null)
                            {
                                foreach (var dll in dlls)
                                {
                                    foreach (var type in dll.Value.Types)
                                    {
                                        foreach (var f in type.Fields)
                                        {
                                            if (f.Name == info.Name && type.Name == info.Class && type.NameSpace == info.Namespace)
                                            {
                                                f2 = f;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (f2 == null)
                            {
                                clrError("Failed to resolve field for writing.", "");
                                return null;
                            }
                            MethodArgStack obj = stack[stack.Count - 2];
                            if (obj == null)
                            {
                                clrError("Failed to find correct type in the stack", "");
                                return null;
                            }

                            if (obj.type == StackItemType.ldnull)
                            {
                                clrError($"stflfd instruction: Attempted to write to field {f2.Name} in type {f2.ParrentType.FullName}, however the instance of the type is null", "System.NullReferenceException");
                                return null;
                            }

                            var data = Objects.ObjectRefs[(int)obj.value];
                            if (data.Fields.ContainsKey(f2.Name))
                            {
                                Objects.ObjectRefs[(int)obj.value].Fields[f2.Name] = stack[stack.Count - 1];
                            }
                            else
                            {
                                Objects.ObjectRefs[(int)obj.value].Fields.Add(f2.Name, stack[stack.Count - 1]);
                            }
                            stack.RemoveAt(stack.Count - 1);
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stind_I:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_I1:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_I2:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_I4:
                        {
                            var val = stack[stack.Count - 1];
                            var ptr = stack[stack.Count - 2];
                            if (ptr.type != StackItemType.Int32) throw new InvalidOperationException("Invaild pointer!");

                            ptr.value = val.value;
                            stack[stack.Count - 2] = val; //just in case
                            stack.RemoveAt(stack.Count - 1); //remove value
                            break;
                        }
                    case OpCodes.OpCodesList.Stind_I8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_R4:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_R8:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stind_Ref:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stloc:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stloc_0:
                        {
                            if (ThrowIfStackIsZero(stack, "stloc.0")) return null;

                            var oldItem = stack[stack.Count - 1];
                            Localstack[0] = oldItem;
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stloc_1:
                        {
                            if (ThrowIfStackIsZero(stack, "stloc.1")) return null;
                            var oldItem = stack[stack.Count - 1];

                            Localstack[1] = oldItem;
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stloc_2:
                        {
                            if (ThrowIfStackIsZero(stack, "stloc.2")) return null;
                            var oldItem = stack[stack.Count - 1];

                            Localstack[2] = oldItem;
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stloc_3:
                        {
                            if (ThrowIfStackIsZero(stack, "stloc.3")) return null;
                            var oldItem = stack[stack.Count - 1];

                            Localstack[3] = oldItem;
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stloc_S:
                        {
                            if (ThrowIfStackIsZero(stack, "stloc.s")) return null;

                            var oldItem = stack[stack.Count - 1];
                            Localstack[(byte)item.Operand] = oldItem;
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Stobj:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Stsfld:
                        {
                            FieldInfo info = item.Operand as FieldInfo;
                            //write value to field.
                            DotNetField f2 = null;
                            foreach (var f in m.Parent.File.Types)
                            {
                                foreach (var tttt in f.Fields)
                                {
                                    if (tttt.IndexInTabel == info.IndexInTabel && tttt.Name == info.Name)
                                    {
                                        f2 = tttt;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(info.Class) && f2 == null)
                            {
                                throw new Exception("Could not find the field");
                            }
                            if (f2 == null)
                            {
                                foreach (var dll in dlls)
                                {
                                    foreach (var type in dll.Value.Types)
                                    {
                                        foreach (var f in type.Fields)
                                        {
                                            if (f.Name == info.Name && type.Name == info.Class && type.NameSpace == info.Namespace)
                                            {
                                                f2 = f;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (f2 == null)
                            {
                                throw new InvalidOperationException("Cannot find the field to write to.");
                            }
                            StaticField f3 = null;
                            foreach (var f in StaticFieldHolder.staticFields)
                            {
                                if (f.theField.Name == f2.Name && f.theField.ParrentType.FullName == f2.ParrentType.FullName)
                                {
                                    f3 = f;

                                    f.value = stack[stack.Count - 1];
                                    break;
                                }
                            }
                            if (stack.Count == 0)
                            {
                                throw new Exception("stsfld instuction: nothing to write to field (stack is empty). The field is " + f2.ParrentType.FullName + "." + f2.Name);
                            }

                            if (f3 == null)
                            {
                                //create field
                                StaticFieldHolder.staticFields.Add(new StaticField() { theField = f2, value = stack[stack.Count - 1] });
                            }
                            if (f2 == null)
                                throw new Exception("Cannot find the field.");
                            f2.Value = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            break;
                        }
                    case OpCodes.OpCodesList.Sub:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            stack.Add(MathOperations.Op(b, a, MathOperations.Operation.Subtract));
                            break;
                        }
                    case OpCodes.OpCodesList.Sub_Ovf:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Sub_Ovf_Un:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Switch:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Tailcall:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Throw:
                        {
                            //Throw Exception
                            var exp = stack[stack.Count - 1];

                            if (exp.type == StackItemType.ldnull)
                            {
                                clrError("Object reference not set to an instance of an object.", "System.NullReferenceException");
                                return null;
                            }
                            else if (exp.type == StackItemType.Object)
                            {
                                var obj = Objects.ObjectRefs[(int)exp.value];
                                clrError((string)obj.Fields["_message"].value, "System.Exception");
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;
                        }
                    case OpCodes.OpCodesList.Unaligned:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Unbox:
                        {
                        throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Unbox_Any:
                        {
                            var aV = stack.Pop();
                            if (aV.type != StackItemType.ObjectRef) {
                                clrError($"Error in {item.OpCodeName} opcode: type is not objectref", "Internal CLR error");
                                return null;
                            }

                        if (aV.value is Int64 i64)
                            stack.Add(MethodArgStack.Int64(i64));
                        else if (aV.value is Int32 i32)
                            stack.Add(MethodArgStack.Int32(i32));
                        else if (aV.value is UInt64 ui64)
                            stack.Add(MethodArgStack.UInt64(ui64));
                        else if (aV.value is UInt32 ui32)
                            stack.Add(MethodArgStack.UInt32(ui32));
                        else if (aV.value is float f32)
                            stack.Add(MethodArgStack.Float32(f32));
                        else if (aV.value is double f64)
                            stack.Add(MethodArgStack.Float64(f64));
                        else
                            clrError($"Error in {item.OpCodeName} opcode: object ref type is not supported", "Internal CLR error");
                            break;
                        }
                    case OpCodes.OpCodesList.Volatile:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                    case OpCodes.OpCodesList.Xor:
                        {
                            var a = stack.Pop();
                            var b = stack.Pop();

                            if (a.type != StackItemType.Int32 || b.type != StackItemType.Int32)
                            {
                                clrError($"Error in {item.OpCodeName} opcode: type is not int32", "Internal CLR error");
                                return null;
                            }

                            var numb1 = (int)a.value;
                            var numb2 = (int)b.value;
                            var result = numb1 ^ numb2;
                            stack.Add(MethodArgStack.Int32(result));
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException($"OpCode {item.OpCodeName} not implemented");
                        }
                }
            }
            return null;
        }
        /// <summary>
        /// Creates an instance of an object
        /// </summary>
        /// <param name="Constructor">The class's constructor method</param>
        /// <param name="Constructorparamss">The parameters of the constructor. Can be an empty list if there are no parameters</param>
        /// <returns>A ready object to be pushed to the stack</returns>
        /// <exception cref="NotImplementedException"></exception>
        public MethodArgStack CreateObject(DotNetMethod Constructor, CustomList<MethodArgStack> Constructorparamss)
        {
            var objAddr = Objects.AllocObject().Index;
            MethodArgStack a = new MethodArgStack() { ObjectContructor = Constructor, ObjectType = Constructor.Parent, type = StackItemType.Object, value = objAddr };
            if (Constructor.Parent.FullName == "System.Decimal")
            {
                //HACK HACK HACK
                a.type = StackItemType.Decimal;
            }
            //init fields
            foreach (var f in Constructor.Parent.Fields)
            {
                switch (f.ValueType.type)
                {
                    case StackItemType.String:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Null());
                        break;
                    case StackItemType.Int32:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Int32(0));
                        break;
                    case StackItemType.Int64:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Int64(0));
                        break;
                    case StackItemType.Float32:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Float32(0));
                        break;
                    case StackItemType.Float64:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Float64(0));
                        break;
                    case StackItemType.Object:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Null());
                        break;
                    case StackItemType.Array:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Null());
                        break;
                    case StackItemType.Any:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Null());
                        break;
                    case StackItemType.Boolean:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.Bool(false));
                        break;
                    case StackItemType.UInt32:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.UInt32(0));
                        break;
                    case StackItemType.UInt64:
                        Objects.ObjectRefs[objAddr].Fields.Add(f.Name, MethodArgStack.UInt64(0));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            //Call the contructor
            if (!InternalCallMethod(new InlineMethodOperandData() { ClassName = Constructor.Parent.Name, FunctionName = Constructor.Name, NameSpace = Constructor.Parent.NameSpace, Signature = Constructor.Signature, RVA = Constructor.RVA }, null, true, true, true, Constructorparamss, a))
            {
                Console.WriteLine("Error occured, returning null.");
                return null; //error occured
            }
            return a;
        }

        /// <summary>
        /// Branches to a target instruction (short form) if a given operation returns true.
        /// Pops two elements off the stack as part of the comparison.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="op"></param>
        private void BranchWithOp(ILInstruction instruction, CustomList<MethodArgStack> stack, IlDecompiler decompiler, MathOperations.Operation op, ref int i)
        {
            var a = stack.Pop();
            var b = stack.Pop();

            bool invert = false;
            if (op == MathOperations.Operation.NotEqual)
            {
                op = MathOperations.Operation.Equal;
                invert = true;
            }

            var result = MathOperations.Op(b, a, op);
            if (invert) result.value = (int)result.value == 1 ? 0 : 1;

            if ((int)result.value == 1)
            {
                int i2 = instruction.Position + (int)instruction.Operand + 1;
                ILInstruction inst = decompiler.GetInstructionAtOffset(i2, -1);

                if (inst == null)
                    throw new Exception("Attempt to branch to null");
                i = inst.RelPosition - 1;
            }
        }
        /// <summary>
        /// Returns true if there is a problem
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private bool ThrowIfStackIsZero(CustomList<MethodArgStack> stack, string instruction)
        {
            if (stack.Count == 0)
            {
                clrError("Fatal error: The " + instruction + " requires more than 1 items on the stack.", "Internal");
                return true;
            }
            return false;
        }
        #region Utils
        private void PrintColor(string s, ConsoleColor c)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(s);
            Console.ForegroundColor = old;
        }
        private void clrError(string message, string errorType)
        {
            //Running = false;
            PrintColor($"A {errorType} has occured in {file.Backend.ClrStringsStream.GetByOffset(file.Backend.Tabels.ModuleTabel[0].Name)}. The error is: {message}", ConsoleColor.Red);

            CallStack.Reverse();
            string stackTrace = "";
            foreach (var itm in CallStack)
            {
                stackTrace += "at " + itm.method.Parent.NameSpace + "." + itm.method.Parent.Name + "." + itm.method.Name + "()\n";
            }
            if (stackTrace.Length > 0)
            {
                stackTrace = stackTrace.Substring(0, stackTrace.Length - 1); //Remove last \n
                //PrintColor(stackTrace, ConsoleColor.Red);
            }
        }
        public void RunMethodInDLL(string NameSpace, string TypeName, string MethodName)
        {
            foreach (var dll in dlls)
            {
                foreach (var type in dll.Value.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (type.NameSpace == NameSpace && type.Name == TypeName && method.Name == MethodName)
                        {
                            RunMethod(method, dll.Value, new CustomList<MethodArgStack>());
                            break;
                        }
                    }
                }
            }
            throw new Exception("Cannot find the method!");
        }

        internal bool InternalCallMethod(InlineMethodOperandData call, DotNetMethod m, bool addToCallStack, bool IsVirt, bool isConstructor, CustomList<MethodArgStack> stack, MethodArgStack constructorObj = null)
        {
            MethodArgStack returnValue;
            DotNetMethod m2 = null;
            if (call.RVA != 0)
            {
                //Local/Defined method
                foreach (var item2 in dlls)
                {
                    foreach (var item3 in item2.Value.Types)
                    {
                        foreach (var meth in item3.Methods)
                        {
                            var s = call.NameSpace + "." + call.ClassName;
                            if (string.IsNullOrEmpty(call.NameSpace))
                                s = call.ClassName;

                            if (meth.RVA == call.RVA && meth.Name == call.FunctionName && meth.Signature == call.Signature && meth.Parent.FullName == s)
                            {
                                m2 = meth;
                                break;
                            }
                        }
                    }
                }

                if (m2 == null)
                {
                    Console.WriteLine($"Cannot resolve called method: {call.NameSpace}.{call.ClassName}.{call.FunctionName}(). Function signature is {call.Signature}");
                    return false;
                }
            }
            else
            {
                if (call.NameSpace == "System" && call.ClassName == "Object" && call.FunctionName == ".ctor")
                {
                    return true; //Ignore
                }
                //Attempt to resolve it
                foreach (var item2 in dlls)
                {
                    foreach (var item3 in item2.Value.Types)
                    {
                        foreach (var meth in item3.Methods)
                        {
                            if (meth.Name == call.FunctionName && meth.Parent.Name == call.ClassName && meth.Parent.NameSpace == call.NameSpace && meth.Signature == call.Signature)
                            {
                                m2 = meth;
                                break;
                            }
                        }
                    }
                }
                if (m2 == null)
                {
                    clrError($"Cannot resolve method: {call.NameSpace}.{call.ClassName}.{call.FunctionName}. Method signature is {call.Signature}", "System.MethodNotFound");
                    return false;
                }
            }


            if (m2.AmountOfParms > stack.Count)
            {
                throw new Exception("Error: wrong amount of parameters supplied to function: " + m2.Name);
            }

            //for interfaces
            if (m2.RVA == 0 && !m2.IsImplementedByRuntime && !m2.IsInternalCall && m2.Parent.IsInterface)
            {
                //TODO: make sure that the object implements the interface
                var obj = stack[stack.Count - 1];
                if (obj.type != StackItemType.Object) throw new InvalidOperationException();

                m2 = null;
                foreach (var item in obj.ObjectType.Methods)
                {
                    if (item.Name == call.FunctionName)
                    {
                        m2 = item;
                    }
                }
                if (m2 == null)
                {
                    clrError("Cannot resolve method that should be implemented by the interface", "Internal CLR error");
                    return false;
                }
            }

            //Extract the params
            int StartParmIndex = stack.Count - m2.AmountOfParms;
            int EndParmIndex = stack.Count - 1;
            if (m2.AmountOfParms == 0)
            {
                StartParmIndex = -1;
                EndParmIndex = -1;
            }
            if ((EndParmIndex - StartParmIndex + 1) != m2.AmountOfParms && StartParmIndex != -1 && m2.AmountOfParms != 1)
            {
                throw new InvalidOperationException("Fatal error: an attempt was made to read " + (EndParmIndex - StartParmIndex + 1) + " parameters before calling the method, but it only needs " + m2.AmountOfParms);
            }
            CustomList<MethodArgStack> newParms = new CustomList<MethodArgStack>();
            //Find the object that we are calling it on (if any)
            MethodArgStack objectToCallOn = null;
            int objectToCallOnIndex = -1;
            if (!isConstructor && !m2.IsStatic)
            {
                if (m2.AmountOfParms >= 0)
                {
                    var idx = stack.Count - m2.AmountOfParms - 1;
                    if (idx >= 0)
                    {
                        objectToCallOn = stack[idx];
                        objectToCallOnIndex = idx;
                        bool a = false;
                        if (objectToCallOn.type == StackItemType.Object)
                        {
                            if (objectToCallOn.ObjectType != m2.Parent)
                            {
                                a = true;
                            }
                        }
                        //TODO: remove this hack
                        if (objectToCallOn.type != StackItemType.Object && idx != 0 && !IsSpecialType(objectToCallOn, m2) && a)
                        {
                            objectToCallOn = stack[idx - 1];
                            objectToCallOnIndex = idx - 1;
                            if (objectToCallOn.type == StackItemType.Object)
                            {
                                if (objectToCallOn.ObjectType != m2.Parent)
                                {
                                    objectToCallOn = null;
                                    objectToCallOnIndex = -1;
                                }
                            }
                        }
                    }
                }
            }
            if (objectToCallOn == null && IsVirt && !isConstructor)
            {
                //Try to find it
                for (int i = stack.Count - 1; i >= 0; i--)
                {
                    var itm = stack[i];
                    if (itm.type == StackItemType.Object)
                    {
                        if (itm.ObjectType == m2.Parent)
                        {
                            objectToCallOn = itm;
                            objectToCallOnIndex = i;
                            break;
                        }
                    }
                }

                if (objectToCallOn == null && !m2.IsStatic)
                {
                    throw new Exception("An attempt was made to call a virtual method with no object");
                }
            }
            if (objectToCallOn != null && m2.HasThis)
            {
                newParms.Add(objectToCallOn);
            }
            if (isConstructor)
            {
                newParms.Add(constructorObj);
            }
            if (m2.AmountOfParms == 0)
            {
                StartParmIndex = -1;
                EndParmIndex = -1;
            }
            if (StartParmIndex != -1)
            {
                for (int i5 = StartParmIndex; i5 < EndParmIndex + 1; i5++)
                {
                    var itm5 = stack[i5];
                    newParms.Add(itm5);
                }
                if (StartParmIndex == 1 && EndParmIndex == 1)
                {
                    newParms.Add(stack[StartParmIndex]);
                }
            }
            if (StartParmIndex == 0 && EndParmIndex == 0 && m2.AmountOfParms == 1)
            {
                newParms.Add(stack[0]);
            }

            //Call it
            var oldStack = stack.backend.GetRange(0, stack.Count);

            if (m != null)
                returnValue = RunMethod(m2, m.Parent.File, newParms, addToCallStack);
            else
                returnValue = RunMethod(m2, m2.Parent.File, newParms, addToCallStack);
            stack.backend = oldStack;

            //Remove the parameters once we are finished
            if (StartParmIndex != -1 && EndParmIndex != -1)
            {
                stack.RemoveRange(StartParmIndex, EndParmIndex - StartParmIndex + 1);
            }
            if (returnValue != null)
            {
                stack.Add(returnValue);
            }
            if (objectToCallOnIndex != -1)
            {
                stack.RemoveAt(objectToCallOnIndex);
            }
            return true;
        }

        private bool IsSpecialType(MethodArgStack obj, DotNetMethod m)
        {
            if (m.Parent.FullName == "System.String" && obj.type == StackItemType.String)
            {
                return true;
            }
            if (m.Parent.FullName == "System.IntPtr" && obj.type == StackItemType.IntPtr)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Byte" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.SByte" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.UInt16" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Int16" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.UInt32" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Int32" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.UInt64" && obj.type == StackItemType.Int64)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Int64" && obj.type == StackItemType.Int64)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Char" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Object" && obj.type == StackItemType.Object)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Type" && obj.type == StackItemType.Object)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Boolean" && obj.type == StackItemType.Int32)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Decimal" && obj.type == StackItemType.Object)
            {
                return true;
            }
            if (m.Parent.FullName == "System.Decimal" && obj.type == StackItemType.Decimal)
            {
                return true;
            }
            return false;
        }
        #endregion
    }

    internal static class Arrays
    {
        public static List<ArrayRef> ArrayRefs = new List<ArrayRef>();
        private static int CurrentIndex = 0;
        public static int GetIndexFromRef(MethodArgStack r)
        {
            return (int)r.value;
        }
        public static ArrayRef AllocArray(int arrayLen)
        {
            var r = new ArrayRef();
            r.Length = arrayLen;
            r.Items = new MethodArgStack[arrayLen];
            for (int i = 0; i < arrayLen; i++)
            {
                r.Items[i] = MethodArgStack.Null();
            }
            r.Index = CurrentIndex;
            ArrayRefs.Add(r);
            CurrentIndex++;
            return ArrayRefs[CurrentIndex - 1];
        }
    }


    internal static class Objects
    {
        public static List<ObjectRef> ObjectRefs = new List<ObjectRef>();
        private static int CurrentIndex = 0;
        public static int GetIndexFromRef(MethodArgStack r)
        {
            return (int)r.value;
        }
        public static ObjectRef AllocObject()
        {
            var r = new ObjectRef();
            r.Index = CurrentIndex;
            ObjectRefs.Add(r);

            CurrentIndex++;
            return ObjectRefs[CurrentIndex - 1];
        }
    }
    internal class ObjectRef
    {
        public Dictionary<string, MethodArgStack> Fields = new Dictionary<string, MethodArgStack>();
        public int Index { get; internal set; }
    }
}
