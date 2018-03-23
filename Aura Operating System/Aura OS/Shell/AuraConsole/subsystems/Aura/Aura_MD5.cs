using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_MD5 : Command
    {
        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "md5";
        }

        public override string CommandDesc()
        {
            return "Hashes a string";
        }

        public override string CommandSynt()
        {
            return "[args]";
        }


        public override string[] CommandAliases()
        {
            return new string[] { "md5" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Apps.User.MD5.Hash(args.GetArgAtPosition(0));
        }
    }
}
