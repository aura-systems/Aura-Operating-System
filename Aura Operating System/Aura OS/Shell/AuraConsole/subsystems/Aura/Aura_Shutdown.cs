using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_Shutdown : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "shutdown";
        }

        public override string CommandDesc()
        {
            return "Shuts down the system";
        }

        public override string CommandSynt()
        {
            return "";
        }

        public override string[] CommandAliases()
        {
            return new string[] { "shutdown" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Cosmos.System.Power.Shutdown();
        }
    }
}
