using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;

namespace Aura_OS.Shell.AuraConsole.subsystems.Windows
{
    public class Windows_CLS : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "cls";
        }

        public override string CommandDesc()
        {
            return "Clears console screen";
        }

        public override string CommandSynt()
        {
            return "";
        }

        public override string[] CommandAliases()
        {
            return new string[] { "cls" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Console.Clear();
        }
    }
}
