/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - SystemInfomation
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Core;
using Cosmos.Core;
using System;

namespace Aura_OS.System.Processing.Interpreter.Commands.SystemInfomation
{
    class CommandSystemInfo : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandSystemInfo(string[] commandvalues) : base(commandvalues)
        {
            Description = "to display system information";
        }

        /// <summary>
        /// System Info Command
        /// </summary>
        public override ReturnInfo Execute()
        {
            Console.WriteLine("Computer name:             " + Kernel.ComputerName);
            Console.WriteLine("Operating system name:     Aura");
            Console.WriteLine("Kernel name:               Cosmos-devkit");
            Console.WriteLine(".NET version:              6.0");
            Console.WriteLine("Operating system version:  " + Kernel.Version);
            Console.WriteLine("Operating system revision: " + Kernel.Revision);
            Console.WriteLine("Date and time:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
            Console.WriteLine("System Boot Time:          " + Kernel.BootTime);
            Console.WriteLine("Total memory:              " + Memory.TotalMemory + "MB");
            Console.WriteLine("Used memory:               " + Memory.GetUsedMemory() + "MB");
            Console.WriteLine("Free memory:               " + Memory.GetFreeMemory() + "MB");
            Console.WriteLine("Processor(s):              " + CPU.GetCPUBrandString());
            Console.WriteLine("Graphic mode:              " + Kernel.Canvas.Name());
            Console.WriteLine("Screen size:               " + Kernel.Canvas.Mode.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}