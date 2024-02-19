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
            Console.WriteLine("        .    :?                         Computer name:                " + Kernel.ComputerName);
            Console.WriteLine("        !?:   G5.                       Operating system name:        Aura");
            Console.WriteLine("         7BP!.!@#!                      Kernel name:                  Cosmos-devkit");
            Console.WriteLine("          :5@@7J@@G~                    .NET version:                 6.0");
            Console.WriteLine("            :5~!GG&@B7.                 Operating system version:     " + Kernel.Version);
            Console.WriteLine("       .^J55Y?!^B#GPGGY:                Operating system revision:    " + Kernel.Revision);
            Console.WriteLine("      :!JBGP&@@&BBG&B5??B^              Date and time:                " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
            Console.WriteLine("   .^?Y5G?GB!5&@@#:~&@G!5B~^            System Boot Time:             " + Kernel.BootTime);
            Console.WriteLine("   :^?&@BPP55!YGJ?.~:.^!^:~GP:          Total memory:                 " + Memory.TotalMemory + "MB");
            Console.WriteLine("   :7J?G#5G5.?#G~#^!5J~:J7~^?~          Used memory:                  " + Memory.GetUsedMemory() + "MB");
            Console.WriteLine(" ^5&@##BP!@?GY75!!7BJ?PB5??J&@5^:.      Free memory:                  " + Memory.GetFreeMemory() + "MB");
            Console.WriteLine(" .:JGB&P&7@?BYG7G&G?P#5!PB&@&&@&57J     Processor(s):                 " + CPU.GetCPUBrandString());
            Console.WriteLine("  ?@GB#GGBGJ?J7#@&#5^JGPJ?7!7JPB#P!     Graphic mode:                 " + Kernel.Canvas.Name());
            Console.WriteLine(" ^#&5BP&BGPJG!YB7.^5@&GP555PB@@G7:      Screen size:                  " + Kernel.Canvas.Mode.ToString());
            Console.WriteLine(" ..~P#BB#B?~@#P7 !BP?!~^^^^^:!7");
            Console.WriteLine("   J@@BPB#B7J@@@!P5^");
            Console.WriteLine("   7&P55BBBB?~?GYJ&@&G~");
            Console.WriteLine("   .. P@&#PGG7.:#PJJ5#@P^");
            Console.WriteLine("      ?#?~PYJ5Y!G@@&G5YGBJ");
            Console.WriteLine("       :  .   :^::~^^^^::~.");

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}