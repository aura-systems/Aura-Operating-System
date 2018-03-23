using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WMCommandFramework
{
    static class Util
    {
        public static CommandArgs ParseArguments(string value, string[] rawIndex)
        {
            if ((value == null || value == "") || (rawIndex == null || rawIndex.Length == 0 || rawIndex == new string[0]))
                return new CommandArgs();
            List<string> x = rawIndex.ToList();
            var index = x.IndexOf(value);
            x.RemoveAt(index);
            return new CommandArgs(x.ToArray());
        }
    }
}
