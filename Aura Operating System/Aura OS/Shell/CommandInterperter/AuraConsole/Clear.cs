/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Clear
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.AuraConsole
{
	public class Clear : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Clears console window";
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			Console.Clear();
		}

		public override string Name()
		{
			return "clear";
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
