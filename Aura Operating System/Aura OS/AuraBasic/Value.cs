using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura_OS.AuraBasic
{
    public enum ValueType
    {
        Real,
        String
    }

    public struct Value
    {
        public static readonly Value Zero = new Value(0);
        public ValueType Type { get; set; }

        public double Real { get; set; }
        public string String { get; set; }

        public Value(double real) : this()
        {
            this.Type = ValueType.Real;
            this.Real = real;
        }

        public Value(string str) : this()
        {
            this.Type = ValueType.String;
            this.String = str;
        }

        public Value Convert(ValueType type)
        {
            if (this.Type != type)
            {
                switch (type)
                {
                    case ValueType.Real:
                        this.Real = double.Parse(this.String);
                        this.Type = ValueType.Real;
                        break;
                    case ValueType.String:
                        this.String = this.Real.ToString();
                        this.Type = ValueType.String;
                        break;

                }

            }
            return this;
        }

        public Value BinOp(Value bin, Token tok)
        {
            Value attrib = this;
            if (attrib.Type != bin.Type)
            {
                if (attrib.Type > bin.Type)
                    bin = bin.Convert(attrib.Type);
                else
                    attrib = attrib.Convert(bin.Type);
            }

            if (tok == Token.Plus)
            {
                if (attrib.Type == ValueType.Real)
                    return new Value(attrib.Real + bin.Real);
                else
                    return new Value(attrib.String + bin.String);
            }
            else if (tok == Token.Equal)
            {
                if (attrib.Type == ValueType.Real)
                    return new Value(attrib.Real == bin.Real ? 1 : 0);
                else
                    return new Value(attrib.String == bin.String ? 1 : 0);
            }
            else if (tok == Token.NotEqual)
            {
                if (attrib.Type == ValueType.Real)
                    return new Value(attrib.Real == bin.Real ? 0 : 1);
                else
                    return new Value(attrib.String == bin.String ? 0 : 1);
            }
            else
            {
                if (attrib.Type == ValueType.String)
                    throw new Exception("Cannot do binop on strings except ' +). ' ");

                switch (tok)
                {
                    case Token.Minus: return new Value(attrib.Real - bin.Real);
                    case Token.Asterisk: return new Value(attrib.Real * bin.Real);
                    case Token.Slash: return new Value(attrib.Real / bin.Real);
                    case Token.Caret: return new Value(Math.Pow(attrib.Real, bin.Real));
                    case Token.Less: return new Value(attrib.Real < bin.Real ? 1 : 0);
                    case Token.More: return new Value(attrib.Real > bin.Real ? 1 : 0);
                    case Token.LessEqual: return new Value(attrib.Real <= bin.Real ? 1 : 0);
                    case Token.MoreEqual: return new Value(attrib.Real >= bin.Real ? 1 : 0);
                }
            }
            throw new Exception("Unknow binop");
        }

        public override string ToString()
        {
            if (this.Type == ValueType.Real)
                return this.Real.ToString();
            return this.String;
        }
    }

    public struct Marker
    {
        public int Pointer { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Marker(int pointer, int line, int column) : this()
        {
            Pointer = pointer;
            Line = line;
            Column = column; //use to be bug. Patched.
        }

    }
}
