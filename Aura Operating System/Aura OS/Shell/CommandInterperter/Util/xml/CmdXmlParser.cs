/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command XML Parser
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using System.IO;
using System.Text;
using Aura_OS.System.Parser.XML;
using WMCommandFramework.COSMOS;
namespace Aura_OS.Shell.CommandInterperter.Util.xml
{
	public class CmdXmlParser : Command
	{
		public override string[] Aliases()
		{
			throw new NotImplementedException();
		}

		public override string Description()
		{
			throw new NotImplementedException();
		}

		public override void Invoke(CommandInvoker invoker, CommandArgs args)
		{
			string strData = Encoding.UTF8.GetString(File.ReadAllBytes(args.GetArgAtPosition(0)));

			NanoXMLDocument xml = new NanoXMLDocument(strData);

			NanoXMLNode myAttribute = xml.RootNode["text"];

			if (myAttribute == null)
			{
				Console.WriteLine("[Error] myAttribute null");
			}

			Console.WriteLine(myAttribute.Name);
			Console.WriteLine(myAttribute.Value);
			Console.WriteLine(myAttribute.GetAttribute("size").Value);

		}

		public override string Name()
		{
			throw new NotImplementedException();
		}

		public override string Syntax()
		{
			throw new NotImplementedException();
		}

		public override CommandVersion Version()
		{
			throw new NotImplementedException();
		}
	}
}
