using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZetko.Xml
{
    public class XmlParserException : Exception
    {
        public XmlParserException() : base() {
        }

        public XmlParserException(string message) : base(message) { 
        }
    }
}
