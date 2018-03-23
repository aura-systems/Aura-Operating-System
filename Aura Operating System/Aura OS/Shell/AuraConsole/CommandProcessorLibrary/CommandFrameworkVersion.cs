using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework
{
    public class CommandFrameworkVersion
    {
        private CommandVersion version;
        public CommandFrameworkVersion()
        {
            version = new CommandVersion(0, 1, 5);
        }

        public CommandVersion GetVersion()
        {
            return version;
        }
    }
}
