using LibDotNetParser.CILApi;

namespace libDotNetClr
{
    public class CallStackItem
    {
        public DotNetMethod method;

        public override string ToString()
        {
            return method.ToString();
        }
    }
}