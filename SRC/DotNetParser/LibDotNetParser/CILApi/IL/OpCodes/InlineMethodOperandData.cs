namespace LibDotNetParser.CILApi.IL
{
    public class InlineMethodOperandData
    {
        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public string FunctionName { get; set; }
        public uint RVA { get; set; }
        public string Signature { get; set; }
        public string GenericArg { get; internal set; }
        public uint ParamListIndex { get; set; }
    }
}
