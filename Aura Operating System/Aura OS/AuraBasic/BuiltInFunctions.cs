using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.AuraBasic
{
    class BuiltInFunctions
    {

        //    [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        //  public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        static Random random;

        public static void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("rnd", Rnd);

            #region InstalledMath
            interpreter.AddFunction("str", Str);
            interpreter.AddFunction("num", Num);
            interpreter.AddFunction("abs", Abs);
            interpreter.AddFunction("cos", Cos);
            interpreter.AddFunction("cosh", Cosh);
            interpreter.AddFunction("exp", Exp);
            interpreter.AddFunction("pow", Pow);
            interpreter.AddFunction("log", Log);
            interpreter.AddFunction("log10", Log10);
            interpreter.AddFunction("sin", Sin);
            interpreter.AddFunction("sinh", Sinh);
            interpreter.AddFunction("sign", Sign);

            interpreter.AddFunction("round", Round);
            interpreter.AddFunction("ceiling", Ceiling);
            interpreter.AddFunction("floor", Floor);
            interpreter.AddFunction("truncate", Truncate);

            interpreter.AddFunction("acos", Acos);
            interpreter.AddFunction("asin", Asin);
            interpreter.AddFunction("atan", Atan);
            interpreter.AddFunction("atan2", Atan2);

            interpreter.AddFunction("ieeeremainder", IEEERemainder);

            interpreter.AddFunction("tan", Tan);
            //tanh
            interpreter.AddFunction("max", Max);
            interpreter.AddFunction("min", Min);
            interpreter.AddFunction("not", Not);

            interpreter.AddFunction("PI", m_PI);
            interpreter.AddFunction("E", m_E);
            #endregion InstalledMath

        }
        //also add hex calculation
        #region Math
        public static Value Rnd(Interpreter interpreter, List<Value> args)
        {
            random = new Random();

            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(random.Next((int)args[0].Real, (int)args[1].Real));//random.Next());
        }

        public static Value Str(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return args[0].Convert(ValueType.String);
        }

        public static Value Num(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return args[0].Convert(ValueType.Real);
        }

        public static Value Abs(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Abs(args[0].Real));
        }

        public static Value Cos(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();
            return new Value(Math.Cos(args[0].Real));
        }

        public static Value Cosh(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();
            return new Value(Math.Cosh(args[0].Real));
        }

        public static Value Exp(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Exp(args[0].Real));
        }

        public static Value Pow(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.Pow(args[0].Real, args[1].Real));
        }

        public static Value Log(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Log(args[0].Real));
        }

        public static Value Log10(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Log10(args[0].Real));
        }

        public static Value Sin(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Sin(args[0].Real));
        }

        public static Value Sinh(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Sinh(args[0].Real));
        }

        public static Value Tan(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Tan(args[0].Real));
        }

        public static Value Tanh(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Tanh(args[0].Real));
        }

        public static Value Round(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Round(args[0].Real));
        }

        public static Value Ceiling(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Ceiling(args[0].Real));
        }

        public static Value Floor(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Floor(args[0].Real));
        }

        public static Value Truncate(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Truncate(args[0].Real));
        }

        public static Value Sign(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Sign(args[0].Real));
        }

        public static Value Acos(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Acos(args[0].Real));
        }

        public static Value Asin(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Asin(args[0].Real));
        }

        public static Value Atan(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Atan(args[0].Real));
        }

        public static Value Atan2(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.Atan2(args[0].Real, args[1].Real));
        }

        public static Value IEEERemainder(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.IEEERemainder(args[0].Real, args[1].Real));
        }

        public static Value Max(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.Max(args[0].Real, args[1].Real));
        }

        public static Value Min(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.Min(args[0].Real, args[1].Real));
        }

        public static Value Not(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(args[0].Real == 0 ? 1 : 0);
        }

        public static Value m_PI(Interpreter interpreter, List<Value> args)
        {

            return new Value(Math.PI);
        }

        public static Value m_E(Interpreter interpreter, List<Value> args)
        {

            return new Value(Math.E);
        }

        #endregion Math

    }
}
