using LibDotNetParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDotNetClr
{
    public static class MathOperations
    {
        public enum Operation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Remainder,
            Equal,
            NotEqual,
            GreaterThan,
            LessThan,
            GreaterThanEqual,
            LessThanEqual,
            Negate
        }

        private static MethodArgStack ConvertToInt32(MethodArgStack arg)
        {
            switch (arg.type)
            {
                case StackItemType.Int32: return arg;
                case StackItemType.Char: return MethodArgStack.Int32((int)(char)arg.value);
                default: throw new Exception("Unsupported type conversion");
            }
        }

        public static MethodArgStack Op(MethodArgStack arg, Operation op)
        {
            switch (arg.type)
            {
                case StackItemType.Float32: return OpWithFloat32(arg, arg, op);
                case StackItemType.Float64: return OpWithFloat64(arg, arg, op);
                case StackItemType.Int32: return OpWithInt32(arg, arg, op);
                case StackItemType.Int64: return OpWithInt64(arg, arg, op);
                default: throw new NotImplementedException();
            }
        }

        public static MethodArgStack Op(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            if (arg1.type == StackItemType.Int32 || arg2.type == StackItemType.Int32)
            {
                // int32 is a special case where types such as 'char' and 'boolean' can be converted to an int32 implicitely
                arg1 = ConvertToInt32(arg1);
                arg2 = ConvertToInt32(arg2);
            }



            if (arg1.type != arg2.type && arg2.type != StackItemType.ldnull) throw new Exception("Inconsistent type definitions");

            switch (arg1.type)
            {
                case StackItemType.Float32: return OpWithFloat32(arg1, arg2, op);
                case StackItemType.Float64: return OpWithFloat64(arg1, arg2, op);
                case StackItemType.Int32: return OpWithInt32(arg1, arg2, op);
                case StackItemType.Int64: return OpWithInt64(arg1, arg2, op);
                case StackItemType.UInt64: return OpWithUInt64(arg1, arg2, op);
                case StackItemType.ldnull: return OpWithLdNull(arg1, arg2, op);
                case StackItemType.Object: return OpWithObject(arg1, arg2, op);
                case StackItemType.Array: return OpWithArray(arg1, arg2, op);
                case StackItemType.String: return OpWithString(arg1, arg2, op);
                default: throw new NotImplementedException();
            }
        }
        private static MethodArgStack OpWithString(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            object v1 = arg1.value;
            object v2 = arg2.value;

            switch (op)
            {
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                default: throw new Exception("Invalid operation");
            }
        }
        private static MethodArgStack OpWithArray(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            object v1 = arg1.value;
            object v2 = arg2.value;

            switch (op)
            {
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                default: throw new Exception("Invalid operation");
            }
        }

        private static MethodArgStack OpWithObject(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            object v1 = arg1.value;
            object v2 = arg2.value;

            switch (op)
            {
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                default: throw new Exception("Invalid operation");
            }
        }

        public static MethodArgStack OpWithFloat32(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            float v1 = (float)arg1.value;
            float v2 = (float)arg2.value;

            switch (op)
            {
                case Operation.Add: return MethodArgStack.Float32(v1 + v2);
                case Operation.Subtract: return MethodArgStack.Float32(v1 - v2);
                case Operation.Multiply: return MethodArgStack.Float32(v1 * v2);
                case Operation.Divide: return MethodArgStack.Float32(v1 / v2);
                case Operation.Remainder: return MethodArgStack.Float32(v1 % v2);
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                case Operation.GreaterThan: return MethodArgStack.Int32(v1 > v2 ? 1 : 0);
                case Operation.LessThan: return MethodArgStack.Int32(v1 < v2 ? 1 : 0);
                case Operation.GreaterThanEqual: return MethodArgStack.Int32(v1 >= v2 ? 1 : 0);
                case Operation.LessThanEqual: return MethodArgStack.Int32(v1 <= v2 ? 1 : 0);
                case Operation.Negate: return MethodArgStack.Float32(-v1);
                default: throw new Exception("Invalid operation");
            }
        }

        public static MethodArgStack OpWithFloat64(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            double v1 = (double)arg1.value;
            double v2 = (double)arg2.value;

            switch (op)
            {
                case Operation.Add: return MethodArgStack.Float64(v1 + v2);
                case Operation.Subtract: return MethodArgStack.Float64(v1 - v2);
                case Operation.Multiply: return MethodArgStack.Float64(v1 * v2);
                case Operation.Divide: return MethodArgStack.Float64(v1 / v2);
                case Operation.Remainder: return MethodArgStack.Float64(v1 % v2);
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                case Operation.GreaterThan: return MethodArgStack.Int32(v1 > v2 ? 1 : 0);
                case Operation.LessThan: return MethodArgStack.Int32(v1 < v2 ? 1 : 0);
                case Operation.GreaterThanEqual: return MethodArgStack.Int32(v1 >= v2 ? 1 : 0);
                case Operation.LessThanEqual: return MethodArgStack.Int32(v1 <= v2 ? 1 : 0);
                case Operation.Negate: return MethodArgStack.Float64(-v1);
                default: throw new Exception("Invalid operation");
            }
        }

        public static MethodArgStack OpWithInt32(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            int v1 = (int)arg1.value;
            int v2 = (int)arg2.value;

            switch (op)
            {
                case Operation.Add: return MethodArgStack.Int32(v1 + v2);
                case Operation.Subtract: return MethodArgStack.Int32(v1 - v2);
                case Operation.Multiply: return MethodArgStack.Int32(v1 * v2);
                case Operation.Divide: return MethodArgStack.Int32(v1 / v2);
                case Operation.Remainder: return MethodArgStack.Int32(v1 % v2);
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                case Operation.GreaterThan: return MethodArgStack.Int32(v1 > v2 ? 1 : 0);
                case Operation.LessThan: return MethodArgStack.Int32(v1 < v2 ? 1 : 0);
                case Operation.GreaterThanEqual: return MethodArgStack.Int32(v1 >= v2 ? 1 : 0);
                case Operation.LessThanEqual: return MethodArgStack.Int32(v1 <= v2 ? 1 : 0);
                case Operation.Negate: return MethodArgStack.Int32(-v1);
                default: throw new Exception("Invalid operation");
            }
        }

        public static MethodArgStack OpWithInt64(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            long v1 = (long)arg1.value;
            long v2 = (long)arg2.value;

            switch (op)
            {
                case Operation.Add: return MethodArgStack.Int64(v1 + v2);
                case Operation.Subtract: return MethodArgStack.Int64(v1 - v2);
                case Operation.Multiply: return MethodArgStack.Int64(v1 * v2);
                case Operation.Divide: return MethodArgStack.Int64(v1 / v2);
                case Operation.Remainder: return MethodArgStack.Int64(v1 % v2);
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                case Operation.GreaterThan: return MethodArgStack.Int32(v1 > v2 ? 1 : 0);
                case Operation.LessThan: return MethodArgStack.Int32(v1 < v2 ? 1 : 0);
                case Operation.GreaterThanEqual: return MethodArgStack.Int32(v1 >= v2 ? 1 : 0);
                case Operation.LessThanEqual: return MethodArgStack.Int32(v1 <= v2 ? 1 : 0);
                case Operation.Negate: return MethodArgStack.Int64(-v1);
                default: throw new Exception("Invalid operation");
            }
        }


        public static MethodArgStack OpWithUInt64(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            ulong v1 = (ulong)arg1.value;
            ulong v2 = (ulong)arg2.value;

            switch (op)
            {
                case Operation.Add: return MethodArgStack.UInt64(v1 + v2);
                case Operation.Subtract: return MethodArgStack.UInt64(v1 - v2);
                case Operation.Multiply: return MethodArgStack.UInt64(v1 * v2);
                case Operation.Divide: return MethodArgStack.UInt64(v1 / v2);
                case Operation.Remainder: return MethodArgStack.UInt64(v1 % v2);
                case Operation.Equal: return MethodArgStack.UInt64(v1 == v2 ? (ulong)1 : (ulong)0);
                case Operation.GreaterThan: return MethodArgStack.UInt64(v1 > v2 ? (ulong)1 : (ulong)0);
                case Operation.LessThan: return MethodArgStack.UInt64(v1 < v2 ? (ulong)1 : (ulong)0);
                case Operation.GreaterThanEqual: return MethodArgStack.UInt64(v1 >= v2 ? (ulong)1 : (ulong)0);
                case Operation.LessThanEqual: return MethodArgStack.UInt64(v1 <= v2 ? (ulong)1 : (ulong)0);
                default: throw new Exception("Invalid operation");
            }
        }

        public static MethodArgStack OpWithLdNull(MethodArgStack arg1, MethodArgStack arg2, Operation op)
        {
            object v1 = arg1.value;
            object v2 = arg2.value;

            switch (op)
            {
                case Operation.Equal: return MethodArgStack.Int32(v1 == v2 ? 1 : 0);
                default: throw new Exception("Invalid operation");
            }
        }
    }
}
