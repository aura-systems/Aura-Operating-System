/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - SystemInfomation
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.Interpreter;
using Cosmos.Core;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
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
            Kernel.console.WriteLine("Computer name:             " + Kernel.ComputerName);
            Kernel.console.WriteLine("Operating system name:     Aura");
            Kernel.console.WriteLine("Kernel name:               Cosmos");
            Kernel.console.WriteLine(".NET version:              6.0");
            Kernel.console.WriteLine("Operating system version:  " + Kernel.Version);
            Kernel.console.WriteLine("Operating system revision: " + Kernel.Revision);
            Kernel.console.WriteLine("Date and time:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
            Kernel.console.WriteLine("System Boot Time:          " + Kernel.BootTime);
            Kernel.console.WriteLine("Total memory:              " + Memory.TotalMemory + "MB");
            Kernel.console.WriteLine("Used memory:               " + Memory.GetUsedMemory() + "MB");
            Kernel.console.WriteLine("Free memory:               " + Memory.GetFreeMemory() + "MB");
            Kernel.console.WriteLine("Processor(s):              " + CPU.GetCPUBrandString());
            Kernel.console.WriteLine("Console mode:              " + Kernel.console.GetConsoleInfo());
            Kernel.console.WriteLine("Screen size:               " + Kernel.canvas.Mode.Columns + "x" + Kernel.canvas.Mode.Rows);

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}