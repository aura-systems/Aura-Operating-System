/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - AuraBasicRun
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using System;
using System.IO;
using Aura_OS.AuraBasic;
namespace Aura_OS.Shell.cmdIntr.c_Console
{
    public class AuraBasicRun
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public AuraBasicRun() { }

        /// <summary>
        /// c = Command, c_AuraBasicRun
        /// </summary>
        /// <param name="txt">The string you wish to pass in. (to be echoed)</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_AuraBasicRun(string file, short startIndex = 0, int count = 6)
        {
            file = file.Remove(startIndex, count);

            if (!file.Contains(".abs"))
                Console.WriteLine("It appears you don't have our extention for this file, it won't execute you know?");

            Interpreter AuraBasic = new Interpreter(File.ReadAllText(file));

            try
            {
                AuraBasic.Exec();
            }
            catch (Exception e)
            {
                Console.WriteLine("BAD"); //mostly debug, not needed - really.
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("OK"); //mostly debug, not needed - really.
        }
    }
}
