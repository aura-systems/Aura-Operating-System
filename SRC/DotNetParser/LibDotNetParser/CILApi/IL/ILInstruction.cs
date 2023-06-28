using LibDotNetParser.CILApi.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.CILApi
{
    /// <summary>
    /// Represenets an IL Instruction
    /// </summary>
    public class ILInstruction
    {
        /// <summary>
        /// The opcode
        /// </summary>
        public ushort OpCode { get; set; }
        /// <summary>
        /// The operand
        /// </summary>
        public object Operand { get; set; }
        /// <summary>
        /// OpCode Name
        /// </summary>
        public string OpCodeName { get; set; }

        public OpCodeOperandType OperandType { get; set; }
        /// <summary>
        /// The position of the Opcode in the method body bytes
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Releative postion based on IlInstruction[] array.
        /// </summary>
        public int RelPosition { get; set; }
        /// <summary>
        /// The size of the instruction.
        /// </summary>
        public int Size { get; set; }
        public override string ToString()
        {
            return OpCodeName + " At: " + Position;
        }
    }
}
