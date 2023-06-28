using LibDotNetParser.CILApi;
using LibDotNetParser.CILApi.IL;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser
{
    /// <summary>
    /// Converts ILInstruction[] to a string
    /// </summary>
    public class ILFormater
    {
        ILInstruction[] insts;
        public ILFormater(ILInstruction[] insts)
        {
            this.insts = insts;
        }

        public string Format()
        {
            string output = "";
            foreach (var item in insts)
            {
                output += $"IL_{item.Position.ToString("X4")}: {item.OpCodeName}";
                if (item.Operand is string @string)
                {
                    output += $" {@string}\"\n";
                }
                else if (item.Operand is InlineMethodOperandData me)
                {
                    output += $" {me.NameSpace}.{me.ClassName}.{me.FunctionName}()\n";
                }
                else if (item.Operand is int i)
                {
                    output += $" {i}\n";
                }
                else if (item.Operand is byte b)
                {
                    output += $" {b}\n";
                }
                else
                {
                    output += $"\n";
                }
            }
            return output;
        }
    }
}
