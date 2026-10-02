/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - SystemInfomation
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Core;
using System;
using System.Text;

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
            StringBuilder sb = new StringBuilder();

            // boot.bat runs before the canvas and the theme manager exist (a null dereference halts gen3).
            string graphicMode = Kernel.Canvas != null ? Kernel.Canvas.Name : "None";
            string screenSize = Kernel.Canvas != null ? Kernel.Canvas.Mode.ToString() : "None";
            string themeName = Kernel.ThemeManager != null ? Kernel.ThemeManager.GetThemeName() : "None";

            sb.AppendLine("        .    :?                         Computer name:                " + Kernel.ComputerName);
            sb.AppendLine("        !?:   G5.                       Operating system name:        Aura");
            sb.AppendLine("         7BP!.!@#!                      Kernel name:                  Cosmos gen3 " + Cosmos.Kernel.System.Kernel.VersionString);
            sb.AppendLine("          :5@@7J@@G~                    .NET version:                 10.0 (NativeAOT)");
            sb.AppendLine("            :5~!GG&@B7.                 Operating system version:     " + Kernel.Version);
            sb.AppendLine("       .^J55Y?!^B#GPGGY:                Operating system revision:    " + Kernel.Revision);
            sb.AppendLine("      :!JBGP&@@&BBG&B5??B^              Date and time:                " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
            sb.AppendLine("   .^?Y5G?GB!5&@@#:~&@G!5B~^            System Boot Time:             " + Kernel.BootTime);
            sb.AppendLine("   :^?&@BPP55!YGJ?.~:.^!^:~GP:          Total memory:                 " + Memory.TotalMemory + "MB");
            sb.AppendLine("   :7J?G#5G5.?#G~#^!5J~:J7~^?~          Used memory:                  " + Memory.GetUsedMemory() + "MB");
            sb.AppendLine(" ^5&@##BP!@?GY75!!7BJ?PB5??J&@5^:.      Free memory:                  " + Memory.GetFreeMemory() + "MB");
            sb.AppendLine(" .:JGB&P&7@?BYG7G&G?P#5!PB&@&&@&57J     Processor(s):                 " + CpuInfo.GetBrandString());
            sb.AppendLine("  ?@GB#GGBGJ?J7#@&#5^JGPJ?7!7JPB#P!     Graphic mode:                 " + graphicMode);
            sb.AppendLine(" ^#&5BP&BGPJG!YB7.^5@&GP555PB@@G7:      Screen size:                  " + screenSize);
            sb.AppendLine(" ..~P#BB#B?~@#P7 !BP?!~^^^^^:!7         Theme name:                   " + themeName);
            sb.AppendLine("   J@@BPB#B7J@@@!P5^");
            sb.AppendLine("   7&P55BBBB?~?GYJ&@&G~");
            sb.AppendLine("   .. P@&#PGG7.:#PJJ5#@P^");
            sb.AppendLine("      ?#?~PYJ5Y!G@@&G5YGBJ");
            sb.AppendLine("       :  .   :^::~^^^^::~.");

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}