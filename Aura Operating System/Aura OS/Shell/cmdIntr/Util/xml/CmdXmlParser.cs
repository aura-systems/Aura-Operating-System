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

            string strData = Encoding.UTF8.GetString(File.ReadAllBytes(txt));

            NanoXMLDocument xml = new NanoXMLDocument(strData);

            NanoXMLNode myAttribute = xml.RootNode["text"];

            if (myAttribute == null)
            {
                Console.WriteLine("[Error] myAttribute null");
            }

            Console.WriteLine(myAttribute.Name);
            Console.WriteLine(myAttribute.Value);
            Console.WriteLine(myAttribute.GetAttribute("size").Value);

        }

    }
}