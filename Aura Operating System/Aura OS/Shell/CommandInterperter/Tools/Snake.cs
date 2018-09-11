/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Snake
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using Aura_OS.Apps.User;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.Tools
{
	public class Snake : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Runs the snake game";
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			PrgmSnake prgm = new PrgmSnake();
			prgm.Run();
		}

		public override string Name()
		{
			return "snake";
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
