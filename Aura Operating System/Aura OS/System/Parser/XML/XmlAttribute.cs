using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZetko.Xml
{
    public class XmlAttribute
    {
        private string _attributeName = String.Empty;
        private string _content = String.Empty;
        private XmlElement _parentNode;

        public XmlAttribute(string attributeName, string content, XmlElement parentNode)
        {
            _attributeName = attributeName;
            _content = content;
            _parentNode = parentNode;
        }

        public string Name
        {
            get
            {
                return _attributeName;
            }
        }

        public string Content
        {
            get
            {
                return _content;
            }
        }

        public XmlElement ParentNode
        {
            get
            {
                return _parentNode;
            }
        }
    }
}
