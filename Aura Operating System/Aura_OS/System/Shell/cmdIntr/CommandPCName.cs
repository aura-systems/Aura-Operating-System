using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr
{
    // Change & Display Computer Name Module
    // 更改或显示计算机名
    // Author: masteryuan418
    class CommandPCName : ICommand
    {
        public CommandPCName(string[] commandvalues) : base(commandvalues)
        {
            Description = "change or display your pc name!";
        }
        public override ReturnInfo Execute(List<string> args)
        {
            if (args.Count < 1) //Check Arg Count
            {
                Console.WriteLine("Args too few!");return new ReturnInfo(this, ReturnCode.ERROR);
            }
            else if (args[0] == "-s")
            {
                Console.WriteLine("Your computer name: " + Global.ComputerName);return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (args[0] == "-c" && args.Count == 2 && args[1] != string.Empty)
            {
                string name = args[1];
                Global.ComputerName = name;Console.WriteLine("Set successful to " + name + ".");return new ReturnInfo(this, ReturnCode.OK);
            }
            return new ReturnInfo(this, ReturnCode.ERROR);
        }
        // override /help command
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- pcn -s            show your computer name");
            Console.WriteLine("- pcn -c            change your computer name");
        }
    }
}
