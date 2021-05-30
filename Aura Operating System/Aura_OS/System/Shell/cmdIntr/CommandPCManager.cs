using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr
{
    // PC Manager Module
    // PC管理模块
    // Author: masteryuan418
    class CommandPCManager : ICommand
    {
        string oldComputerName = Kernel.ComputerName;
        public CommandPCManager(string[] commandvalues) : base(commandvalues)
        {
            Description = "change or display your pc settings!";
        }
        public override ReturnInfo Execute(List<string> args)
        {
            if (args.Count < 2) //Check Arg Count
            {
                Console.WriteLine("Args too few!");return new ReturnInfo(this, ReturnCode.ERROR);
            }
            else if (args[0] == "-s" && args[1] == "comp-name")
            {
                Console.WriteLine("Your computer name: " + Kernel.ComputerName);return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (args[0] == "-c" && args.Count == 3 && args[2] != string.Empty && args[1] == "comp-name")
            {
                string name = args[2];
                Kernel.ComputerName = name;Console.WriteLine("Set successful to " + name + ".");return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (args[0] == "-s" && args[1] == "comp-lang")
            {
                Console.WriteLine("Your computer selected language: " + Kernel.langSelected); return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (args[0] == "-s" && args[1] == "comp-time")
            {
                string t = Time.YearString() + "/" + Time.MonthString() + "/" + Time.DayString();
                Console.WriteLine("Your computer time: " + t); return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (args[0] == "-c" && args[1] == "comp-name" && args.Count == 3 && args[2] != string.Empty && args[2] == "reset")
            {
                Kernel.ComputerName = oldComputerName;
                Console.WriteLine("Reset computerName successful,to: " + oldComputerName); return new ReturnInfo(this, ReturnCode.OK);
            }
            return new ReturnInfo(this, ReturnCode.ERROR);
        }
        // override /help command
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- pcm -s comp-name                 show your computer name");
            Console.WriteLine("- pcm -c comp-name <Name>          change your computer name");
            Console.WriteLine("- pcm -c comp-name reset           reset your computer name");
            Console.WriteLine("- pcm -s comp-lang                 show your computer lang");
            Console.WriteLine("- pcm -s comp-time                 show your computer time");
        }
    }
}
