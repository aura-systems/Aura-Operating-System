using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_Reboot : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "reboot";
        }

        public override string CommandDesc()
        {
            return "Reboots your system";
        }

        public override string CommandSynt()
        {
            return "";
        }

        public override string[] CommandAliases()
        {
            return new string[] { "reboot" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Cosmos.System.Power.Reboot();
        }
    }
}
