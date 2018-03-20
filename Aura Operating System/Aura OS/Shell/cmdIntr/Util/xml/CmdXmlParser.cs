/*
* PROJECT:          Aura Operating System Development
* CONTENT:          XML Parser test command.
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Parser.XML;
using System.IO;
using System.Text;
using System;

namespace Aura_OS.Shell.cmdIntr.Util.xml
{
    public class CmdXmlParser
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
        public CmdXmlParser() { }

        /// <summary>
        /// c = command, c_CmdXmlParser
        /// </summary>
        public static void c_CmdXmlParser(string txt, short startIndex = 0, int count = 4)
        {
            txt = txt.Remove(startIndex, count);

            
            Console.WriteLine("1");
            Console.ReadKey();

            string strData = Encoding.UTF8.GetString(File.ReadAllBytes(txt));

            Console.WriteLine("2");
            Console.ReadKey();

            NanoXMLDocument xml = new NanoXMLDocument(strData);

            Console.WriteLine("3");
            Console.ReadKey();

            string myAttribute = xml.RootNode["app"].GetAttribute("text").Name;

            Console.WriteLine("4");
            Console.ReadKey();

            string myAttribute1 = xml.RootNode["app"].GetAttribute("text").Value;

            Console.WriteLine("5");
            Console.ReadKey();

            Console.WriteLine(myAttribute);

            Console.WriteLine("6");
            Console.ReadKey();

            Console.WriteLine(myAttribute1);
        }

    }
}