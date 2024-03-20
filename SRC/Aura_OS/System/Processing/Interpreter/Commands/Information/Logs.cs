/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Logs
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.SystemInfomation
{
    class CommandLogs : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandLogs(string[] commandvalues) : base(commandvalues)
        {
            Description = "to display Cosmos kernel logs";
        }

        /// <summary>
        /// Logs Command
        /// </summary>
        public override ReturnInfo Execute()
        {
            StringBuilder sb = new StringBuilder();

            foreach (LogEntry entry in Logs.LogList)
            {
                if (entry.Level == LogLevel.Kernel)
                {
                    sb.AppendLine(entry.DateTime.ToString() + " - [Cosmos] - " + entry.Log);
                }
                else
                {
                    sb.AppendLine(entry.DateTime.ToString() + " - [AuraOS] - " + entry.Log);
                }
            }

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Logs Command
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0] == "/k")
            {
                ShowLogs(LogLevel.Kernel);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            else if (arguments[0] == "/o")
            {
                ShowLogs(LogLevel.OS);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }
        }

        public void ShowLogs(LogLevel level)
        {
            StringBuilder sb = new StringBuilder();

            foreach (LogEntry entry in Logs.LogList)
            {
                if (entry.Level == level)
                {
                    if (entry.Level == LogLevel.Kernel)
                    {
                        sb.AppendLine(entry.DateTime.ToString() + " - [Cosmos] - " + entry.Log);
                    }
                    else
                    {
                        sb.AppendLine(entry.DateTime.ToString() + " - [AuraOS] -  " + entry.Log);
                    }
                }
            }

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - logs        Show all logs");
            Console.WriteLine(" - logs /k     Show Cosmos logs");
            Console.WriteLine(" - logs /o     Show AuraOS logs");
        }
    }
}