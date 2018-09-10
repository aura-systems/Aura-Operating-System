using System;
using System.Collections.Generic;
using System.Text;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.AuraConsole
{
	public class Echo : Command
	{
		public override string[] Aliases()
		{
			return new string[0];
		}

		public override string Description()
		{
			return "Prints a message to the console";
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{//Need to add in environmental varibles ~ Dont forget to update version
			if (args.IsEmpty())
			{
				Console.WriteLine();
			}
			//Do not use until next update.
			//else if (!(args.IsEmpty()) && (args.StartsWithArgument("@off") || args.StartsWithSwitch("off")))
			//{
			//    CommandProcessor.INSTANCE.EchoOff = true;
			//}
			//else if (!(args.IsEmpty()) && (args.StartsWithArgument("@on") || args.StartsWithSwitch("on")))
			//{
			//    CommandProcessor.INSTANCE.EchoOff = false;
			//}
			else
			{
				//Will print everything that's passed to it to the terminal.
				String m = GetAllArgs(args);
				Console.WriteLine(m);
			}
		}

		public override string Name()
		{
			return "echo";
		}

		public override string Syntax()
		{
			return "[message]";
		}

		public override CommandVersion Version()
		{
			return new CommandVersion(1, 0, 0, 0, "a");
		}

		private String GetAllArgs(CommandArgs args)
		{
			String s = "";
			for (int i = 0; i == args.Count(); i++)
			{
				var arg = args.GetArgAtPosition(i);
				if (!(arg == null || arg == ""))
				{
					//Add the argument to 's'.
					if (s == "" || s == null)
						s = arg;
					else
						s += " " + arg;
				}
			}
			if (!(s == null || s == "")) return s;
			return "";
		}
	}
}
