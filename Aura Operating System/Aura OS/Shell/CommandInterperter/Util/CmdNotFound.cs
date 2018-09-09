using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.Util
{
	public class CmdNotFound : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Command not found exception";
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			System.Translation.Text.Display("UnknownCommand");
			Console.ForegroundColor = ConsoleColor.White;
		}

		public override string Name()
		{
			return "cmdnotfound";
		}

		public override string Syntax()
		{
			return new "";
		}

		public override CommandVersion Version()
		{
			return new CommandVersion(1, 0, 0, 0, "a");
		}
	}
}
