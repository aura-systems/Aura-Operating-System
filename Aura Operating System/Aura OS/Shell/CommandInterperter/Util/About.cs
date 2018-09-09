using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.Util
{
	public class About : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Prints about infomation to console"
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			System.Translation.List_Translation.About();
		}

		public override string Name()
		{
			return "about";
		}

		public override string Syntax()
		{
			return "";
		}

		public override CommandVersion Version()
		{
			return new CommandVersion(1, 0, 0, 0, "a");
		}
	}
}
