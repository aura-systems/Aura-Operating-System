using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;

namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_Export : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "export";
        }

        public override string CommandDesc()
        {
            return "Environment variables handler";
        }

        public override string CommandSynt()
        {
            return "[args]";
        }

        public override string[] CommandAliases()
        {
            return new string[] { "export" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
          
        }
    }
}
