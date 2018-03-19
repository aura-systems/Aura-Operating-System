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
        public static void c_CmdXmlParser(string txt, short startIndex = 0, int count = 5)
        {
            txt = txt.Remove(startIndex, count);

            FileStream fs = new FileStream(txt, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, (int)fs.Length);
            fs.Close();

            string strData = Encoding.UTF8.GetString(data);
            NanoXMLDocument xml = new NanoXMLDocument(strData);

            string myAttribute = 

            Console.WriteLine(myAttribute);

        }

    }
}