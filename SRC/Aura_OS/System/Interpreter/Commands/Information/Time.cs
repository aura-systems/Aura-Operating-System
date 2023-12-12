/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Time
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Aura_OS.Interpreter;
using System;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class CommandTime : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandTime(string[] commandvalues) : base(commandvalues)
        {
            Description = "to get time and date";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            Console.WriteLine("The current time is:  " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}