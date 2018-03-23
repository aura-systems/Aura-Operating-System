using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_Snake : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "snake";
        }

        public override string CommandDesc()
        {
            return "Runs a snake themed game";
        }

        public override string CommandSynt()
        {
            return "";
        }

        public override string[] CommandAliases()
        {
            return new string[] { "snake" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Apps.User.PrgmSnake prgm = new Apps.User.PrgmSnake();
            prgm.Run();
        }
    }
}
