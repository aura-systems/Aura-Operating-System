using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework;
namespace Aura_OS.Shell.AuraConsole.subsystems.Aura
{
    public class Aura_Logout : Command
    {

        CommandVersion version = new CommandVersion(1, 0, 0, 0, "RELEASE");

        public override string CommandName()
        {
            return "logout";
        }

        public override string CommandDesc()
        {
            return "Logs you out";
        }

        public override string CommandSynt()
        {
            return "";
        }


        public override string[] CommandAliases()
        {
            return new string[] { "logout" };
        }

        public override CommandVersion CommandVersion()
        {
            return version;
        }

        public override void OnCommandInvoked(CommandInvoker invoker, CommandArgs args)
        {
            Kernel.Logged = false;
            Kernel.userLevelLogged = "";
            Kernel.userLogged = "";
            Directory.SetCurrentDirectory(Kernel.current_directory);
            Kernel.current_directory = @"0:\";
            Console.Clear();
        }
    }
}
