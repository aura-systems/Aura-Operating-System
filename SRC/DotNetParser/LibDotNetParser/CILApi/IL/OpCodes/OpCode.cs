using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.CILApi.IL
{
    /// <summary>
    /// Represents CLR Opcode
    /// </summary>
    public class OpCode
    {
        public string Name { get; internal set; }
        public ushort Value { get; internal set; }
        public OpCodeOperandType OpCodeOperandType { get; internal set; }
        public bool IsExtended { get; internal set; }
        public OpCode(string name, ushort value, OpCodeOperandType type)
        {
            this.Name = name;
            this.Value = value;
            this.OpCodeOperandType = type;
            this.IsExtended = false;
        }

        public OpCode(string name, ushort value, OpCodeOperandType type,bool IsExtended)
        {
            this.Name = name;
            this.Value = value;
            this.OpCodeOperandType = type;
            this.IsExtended = IsExtended;
        }
    }
}
