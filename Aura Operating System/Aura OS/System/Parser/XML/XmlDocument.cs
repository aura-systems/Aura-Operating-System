using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZetko.Xml
{
    public class XmlDocument
    {
        private XmlElement _rootNode;

        public XmlElement RootNode
        {
            get
            {
                return _rootNode;
            }

            set
            {
                _rootNode = value;
            }
        }
    }
}
