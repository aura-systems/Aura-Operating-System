using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Aura_OS.System.Interpreter.ABS
{
    public enum Token
    {
        Unknown,
        Identifer,
        Value,
        //Keywords
        Print,
        If,
        EndIf,
        Then,
        Else,
        For,
        To,
        Next,
        Input,
        Let,
        Gosub,
        Return,
        Rem,
        End,

        NewLine, // \n
        Colon, // :
        Semicolon, // ;
        Comma, // ,

        Plus, // +
        Minus, // -
        Slash, // "/"
        Asterisk, //*
        Caret,
        Equal, //=
        Less,
        More,
        NotEqual, //!= 
        LessEqual,
        MoreEqual,
        Or, //| (||) or
        And, //&& (&) and
        Not, //! not

        LParen,
        RParen,

        EOF = -1 //End of file
    }

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
                        this.Real = Int32.Parse(this.String);
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

    public class Lexer
    {
        private readonly string source;
        private Marker sourceMarker;
        private char lastChar;

        public Marker TokenMarker { get; set; }

        public string Identifer { get; set; }
        public Value Value { get; set; }

        public Lexer(string input)
        {
            source = input;
            sourceMarker = new Marker(0, 1, 1);
            lastChar = source[0];
        }

        public void GoTo(Marker marker)
        {
            sourceMarker = marker;
        }

        char GetChar()
        {
            sourceMarker.Column++;
            sourceMarker.Pointer++;

            if (sourceMarker.Pointer >= source.Length)
                return lastChar = (char)0;

            if ((lastChar = source[sourceMarker.Pointer]) == '\n')
            {
                sourceMarker.Column = 1;
                sourceMarker.Line++;
            }

            return lastChar;
        }

        public Token GetToken()
        {
            while (lastChar == ' ' || lastChar == '\t' || lastChar == '\r')
                GetChar();

            TokenMarker = sourceMarker;

            if (char.IsLetter(lastChar))
            {
                Identifer = lastChar.ToString();
                while (char.IsLetterOrDigit(GetChar()))
                    Identifer += lastChar;
                switch (CharImpl.ToUpper(Identifer))
                {
                    case "PRINT": return Token.Print;
                    case "IF": return Token.If;
                    case "ENDIF": return Token.EndIf;
                    case "THEN": return Token.Then;
                    case "ELSE": return Token.Else;
                    case "FOR": return Token.For;
                    case "TO": return Token.To;
                    case "NEXT": return Token.Next;
                    case "INPUT": return Token.Input;
                    case "LET": return Token.Let;
                    case "GOSUB": return Token.Gosub;
                    case "RETURN": return Token.Return;
                    case "END": return Token.End;
                    case "OR": return Token.Or;
                    case "AND": return Token.And;
                    case "REM":
                        while (lastChar != '\n') GetChar();
                        GetChar();
                        return GetToken();
                    default:
                        return Token.Identifer;
                }
            }

            if (char.IsDigit(lastChar))
            {
                string num = "";
                do { num += lastChar; } while (char.IsDigit(GetChar()) || lastChar == '.');

                //double real;
                //if (!double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out real))
                //    throw new Exception("ERROR while parsing number");
                //Value = new Value(real);
                return Token.Value;
            }

            Token tok = Token.Unknown;
            switch (lastChar)
            {
                case '\n': tok = Token.NewLine; break;
                case ':': tok = Token.Colon; break;
                case ';': tok = Token.Semicolon; break;
                case ',': tok = Token.Comma; break;
                case '=': tok = Token.Equal; break;
                case '+': tok = Token.Plus; break;
                case '-': tok = Token.Minus; break;
                case '/': tok = Token.Slash; break;
                case '*': tok = Token.Asterisk; break;
                case '^': tok = Token.Caret; break;
                case '(': tok = Token.LParen; break;
                case ')': tok = Token.RParen; break;
                case '\'':
                    while (lastChar != '\n') GetChar();
                    GetChar();
                    return GetToken();
                case '<':
                    GetChar();
                    if (lastChar == '>') tok = Token.NotEqual;
                    else if (lastChar == '=') tok = Token.LessEqual;
                    else return Token.Less;
                    break;
                case '>':
                    GetChar();
                    if (lastChar == '=') tok = Token.MoreEqual;
                    else return Token.More;
                    break;
                case '"':
                    string str = "";
                    while (GetChar() != '"')
                    {
                        if (lastChar == '\\')
                        {
                            //switch (char.ToLower(GetChar())) PLUGGED
                            switch (CharImpl.ToLower(GetChar()))
                            {
                                case 'n': str += '\n'; break;
                                case 't': str += '\t'; break;
                                case '\\': str += '\\'; break;
                                case '"': str += '"'; break;
                            }
                        }
                        else
                        {
                            str += lastChar;
                        }
                    }
                    Value = new Value(str);
                    tok = Token.Value;
                    break;
                case (char)0:
                    return Token.EOF;
            }

            GetChar();
            return tok;
        }
    }

    public class Interpreter
    {

        public bool HasPrint { get; set; } = true;
        public bool HasInput { get; set; } = true;

        private Lexer lex;
        private Token prevToken;
        private Token lastToken;

        private Dictionary<string, Value> vars;
        private Dictionary<string, Marker> loops;

        public delegate Value AuraBasicFunction(Interpreter interpreter, ref List<Value> args);
        private Dictionary<string, AuraBasicFunction> funcs;

        private int ifcounter;
        private Marker lineMarker;

        public bool exit;

        public Interpreter(string input)
        {
            this.lex = new Lexer(input);
            this.vars = new Dictionary<string, Value>();
            this.loops = new Dictionary<string, Marker>();
            this.funcs = new Dictionary<string, AuraBasicFunction>();
            this.ifcounter = 0;
            BuiltInFunctions.InstallAll(this);
        }

        public Value GetVar(string name)
        {
            if (!vars.ContainsKey(name))
                throw new Exception("Variable with name " + name + " does not exist.");
            return vars[name];
        }

        public void SetVar(string name, Value val)
        {
            if (!vars.ContainsKey(name)) vars.Add(name, val);
            else vars[name] = val;
        }

        public void AddFunction(ref string name, ref AuraBasicFunction function)
        {
            if (!funcs.ContainsKey(name)) funcs.Add(name, function);
            else funcs[name] = function;
        }

        void Error(string text)
        {
            throw new Exception(text + " at line: " + lineMarker.Line);
        }

        void Match(Token tok)
        {
            if (lastToken != tok)
                Error("Expect " + tok.ToString() + " got " + lastToken.ToString());
        }

        public void Exec()
        {
            exit = false;
            GetNextToken();
            while (!exit) Line();
        }

        Token GetNextToken()
        {
            prevToken = lastToken;
            lastToken = lex.GetToken();

            if (lastToken == Token.EOF && prevToken == Token.EOF)
                Error("Unexpected end of file");

            return lastToken;
        }

        void Line()
        {
            while (lastToken == Token.NewLine) GetNextToken();

            if (lastToken == Token.EOF)
            {
                exit = true;
                return;
            }

            lineMarker = lex.TokenMarker;
            Statment();

            if (lastToken != Token.NewLine && lastToken != Token.EOF)
                Error("Expect new line got " + lastToken.ToString());
        }

        void Statment()
        {
            Token keyword = lastToken;
            GetNextToken();
            switch (keyword)
            {
                case Token.Print: Print(); break;
                case Token.Input: Input(); break;
                case Token.If: If(); break;
                case Token.Else: Else(); break;
                case Token.EndIf: break;
                case Token.For: For(); break;
                case Token.Next: Next(); break;
                case Token.Let: Let(); break;
                case Token.End: End(); break;
                case Token.Identifer:
                    if (lastToken == Token.Equal) Let();
                    else goto default;
                    break;
                case Token.EOF:
                    exit = true;
                    break;
                default:
                    Error("Expect keyword got " + keyword.ToString());
                    break;
            }
            if (lastToken == Token.Colon)
            {
                GetNextToken();
                Statment();
            }
        }

        void Print()
        {
            if (!HasPrint)
                Error("Print command not allowed");

            Console.WriteLine(Expr().ToString());
        }

        void Input()
        {
            if (!HasInput)
                Error("Input command not allowed");

            while (true)
            {
                Match(Token.Identifer);

                if (!vars.ContainsKey(lex.Identifer)) vars.Add(lex.Identifer, new Value());

                string input = Console.ReadLine();
                //double d;
                //if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                //    vars[lex.Identifer] = new Value(d);
               // else
                    vars[lex.Identifer] = new Value(input);

                GetNextToken();
                if (lastToken != Token.Comma) break;
                GetNextToken();
            }
        }

        void If()
        {
            bool result = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            Match(Token.Then);
            GetNextToken();

            if (result)
            {
                int i = ifcounter;
                while (true)
                {
                    if (lastToken == Token.If)
                    {
                        i++;
                    }
                    else if (lastToken == Token.Else)
                    {
                        if (i == ifcounter)
                        {
                            GetNextToken();
                            return;
                        }
                    }
                    else if (lastToken == Token.EndIf)
                    {
                        if (i == ifcounter)
                        {
                            GetNextToken();
                            return;
                        }
                        i--;
                    }
                    GetNextToken();
                }
            }
        }

        void Else()
        {
            int i = ifcounter;
            while (true)
            {
                if (lastToken == Token.If)
                {
                    i++;
                }
                else if (lastToken == Token.EndIf)
                {
                    if (i == ifcounter)
                    {
                        GetNextToken();
                        return;
                    }
                    i--;
                }
                GetNextToken();
            }
        }

        void End()
        {
            exit = true;
        }

        void Let()
        {
            if (lastToken != Token.Equal)
            {
                Match(Token.Identifer);
                GetNextToken();
                Match(Token.Equal);
            }

            string id = lex.Identifer;

            GetNextToken();

            SetVar(id, Expr());
        }

        void For()
        {
            Match(Token.Identifer);
            string var = lex.Identifer;

            GetNextToken();
            Match(Token.Equal);

            GetNextToken();
            Value v = Expr();

            if (loops.ContainsKey(var))
            {
                loops[var] = lineMarker;
            }
            else
            {
                SetVar(var, v);
                loops.Add(var, lineMarker);
            }

            Match(Token.To);

            GetNextToken();
            v = Expr();

            if (vars[var].BinOp(v, Token.More).Real == 1)
            {
                while (true)
                {
                    while (!(GetNextToken() == Token.Identifer && prevToken == Token.Next)) ;
                    if (lex.Identifer == var)
                    {
                        loops.Remove(var);
                        GetNextToken();
                        Match(Token.NewLine);
                        break;
                    }
                }
            }

        }

        void Next()
        {
            Match(Token.Identifer);
            string var = lex.Identifer;
            vars[var] = vars[var].BinOp(new Value(1), Token.Plus);
            lex.GoTo(new Marker(loops[var].Pointer - 1, loops[var].Line, loops[var].Column - 1));
            lastToken = Token.NewLine;
        }

        Value Expr()
        {
            Dictionary<Token, int> prec = new Dictionary<Token, int>()
            {
                { Token.Or, 0 }, { Token.And, 0 },
                { Token.Equal, 1 }, { Token.NotEqual, 1 },
                { Token.Less, 1 }, { Token.More, 1 }, { Token.LessEqual, 1 },  { Token.MoreEqual, 1 },
                { Token.Plus, 2 }, { Token.Minus, 2 },
                { Token.Asterisk, 3 }, {Token.Slash, 3 },
                { Token.Caret, 4 }
            };

            Stack<Value> stack = new Stack<Value>();
            Stack<Token> operators = new Stack<Token>();

            int i = 0;
            while (true)
            {
                if (lastToken == Token.Value)
                {
                    stack.Push(lex.Value);
                }
                else if (lastToken == Token.Identifer)
                {
                    if (vars.ContainsKey(lex.Identifer))
                    {
                        stack.Push(vars[lex.Identifer]);
                    }
                    else if (funcs.ContainsKey(lex.Identifer))
                    {
                        string name = lex.Identifer;
                        List<Value> args = new List<Value>();
                        GetNextToken();
                        Match(Token.LParen);

                        start:
                        if (GetNextToken() != Token.RParen)
                        {
                            args.Add(Expr());
                            if (lastToken == Token.Comma)
                                goto start;
                        }

                        Interpreter variable = null;
                        stack.Push(funcs[name](variable, ref args));
                    }
                    else
                    {
                        Error("Undeclared variable " + lex.Identifer);
                    }
                }
                else if (lastToken == Token.LParen)
                {
                    GetNextToken();
                    stack.Push(Expr());
                    Match(Token.RParen);
                }
                else if (lastToken >= Token.Plus && lastToken <= Token.Not)
                {
                    if ((lastToken == Token.Minus || lastToken == Token.Minus) && (i == 0 || prevToken == Token.LParen))
                    {
                        stack.Push(new Value(0));
                        operators.Push(lastToken);
                    }
                    else
                    {
                        while (operators.Count > 0 && prec[lastToken] <= prec[operators.Peek()])
                            Operation(ref stack, operators.Pop());
                        operators.Push(lastToken);
                    }
                }
                else
                {
                    if (i == 0)
                        Error("Empty expression");
                    break;
                }

                i++;
                GetNextToken();
            }

            while (operators.Count > 0)
                Operation(ref stack, operators.Pop());

            return stack.Pop();
        }

        void Operation(ref Stack<Value> stack, Token token)
        {
            Value b = stack.Pop();
            Value a = stack.Pop();
            Value result = a.BinOp(b, token);
            stack.Push(result);
        }

    }

    class BuiltInFunctions
    {
        public static void InstallAll(Interpreter interpreter)
        {
            string str = "str";
            string num = "num";
            string abs = "abs";
            string min = "min";
            string max = "max";
            string not = "not";
            interpreter.AddFunction(ref  str, Str);
            interpreter.AddFunction(ref  num, Num);
            interpreter.AddFunction(ref  abs, Abs);
            interpreter.AddFunction(ref  min, Min);
            interpreter.AddFunction(ref  max, Max);
            interpreter.AddFunction(ref  not, Not);
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

        public static Value Min(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                throw new ArgumentException();

            return new Value(Math.Min(args[0].Real, args[1].Real));
        }

        public static Value Max(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(Math.Max(args[0].Real, args[1].Real));
        }

        public static Value Not(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return new Value(args[0].Real == 0 ? 1 : 0);
        }
    }
}
