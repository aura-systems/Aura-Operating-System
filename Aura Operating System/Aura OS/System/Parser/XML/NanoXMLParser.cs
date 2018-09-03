/*
* PROJECT:          Aura Operating System Development
* CONTENT:          NANOXML Parser
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   John Welsh <djlw78@gmail.com>
* LICENSE:          LICENSES\NANOXML\LICENSE.md
*/


using System;
using System.Collections.Generic;

namespace Aura_OS.System.Parser.XML
{
  /// <summary>
  /// Base class containing usefull features for all XML classes
  /// </summary>
  class NanoXMLBase
  {
    protected static bool IsSpace(char c)
    {
      return c == ' ' || c == '\t' || c == '\n' || c == '\r';
    }

    protected static void SkipSpaces(string str, ref int i)
    {
      while (i < str.Length)
      {
        if (!IsSpace(str[i]))
        {
          if (str[i] == '<' && i + 4 < str.Length && str[i + 1] == '!' && str[i + 2] == '-' && str[i + 3] == '-')
          {
            i += 4; // skip <!--

            while (i + 2 < str.Length && !(str[i] == '-' && str[i + 1] == '-'))
              i++;

            i += 2; // skip --
          }
          else
            break;
        }

        i++;
      }
    }

    protected static string GetValue(string str, ref int i, char endChar, char endChar2, bool stopOnSpace)
    {
      int start = i;
      while ((!stopOnSpace || !IsSpace(str[i])) && str[i] != endChar && str[i] != endChar2) i++;

      return str.Substring(start, i - start);
    }

    protected static bool IsQuote(char c)
    {
      return c == '"' || c == '\'';
    }

    // returns name
    protected static string ParseAttributes(string str, ref int i, List<NanoXMLAttribute> attributes, char endChar, char endChar2)
    {
      SkipSpaces(str, ref i);
      string name = GetValue(str, ref i, endChar, endChar2, true);

      SkipSpaces(str, ref i);

      while (str[i] != endChar && str[i] != endChar2)
      {
        string attrName = GetValue(str, ref i, '=', '\0', true);

        SkipSpaces(str, ref i);
        i++; // skip '='
        SkipSpaces(str, ref i);

        char quote = str[i];
        if (!IsQuote(quote))
          throw new XMLParsingException("Unexpected token after " + attrName);

        i++; // skip quote
        string attrValue = GetValue(str, ref i, quote, '\0', false);
        i++; // skip quote

        attributes.Add(new NanoXMLAttribute(attrName, attrValue));

        SkipSpaces(str, ref i);
      }

      return name;
    }
  }

  /// <summary>
  /// Class representing whole DOM XML document
  /// </summary>
  class NanoXMLDocument: NanoXMLBase
  {
    private NanoXMLNode rootNode;
    private List<NanoXMLAttribute> declarations = new List<NanoXMLAttribute>();
    /// <summary>
    /// Public constructor. Loads xml document from raw string
    /// </summary>
    /// <param name="xmlString">String with xml</param>
    public NanoXMLDocument(string xmlString)
    {
      int i = 0;

      while (true)
      {
        SkipSpaces(xmlString, ref i);

        if (xmlString[i] != '<')
          throw new XMLParsingException("Unexpected token");

        i++; // skip <

        if (xmlString[i] == '?') // declaration
        {
          i++; // skip ?
          ParseAttributes(xmlString, ref i, declarations, '?', '>');
          i++; // skip ending ?
          i++; // skip ending >

          continue;
        }

        if (xmlString[i] == '!') // doctype
        {
          while (xmlString[i] != '>') // skip doctype
            i++;

          i++; // skip >

          continue;
        }

        rootNode = new NanoXMLNode(xmlString, ref i);
        break;
      }
    }
    /// <summary>
    /// Root document element
    /// </summary>
    public NanoXMLNode RootNode
    {
      get { return rootNode; }
    }
    /// <summary>
    /// List of XML Declarations as <see cref="NanoXMLAttribute"/>
    /// </summary>
    public IEnumerable<NanoXMLAttribute> Declarations
    {
      get { return declarations; }
    }
  }

  /// <summary>
  /// Element node of document
  /// </summary>
  class NanoXMLNode: NanoXMLBase
  {
    private string value;
    private string name;

    private List<NanoXMLNode> subNodes = new List<NanoXMLNode>();
    private List<NanoXMLAttribute> attributes = new List<NanoXMLAttribute>();

    internal NanoXMLNode(string str, ref int i)
    {
      name = ParseAttributes(str, ref i, attributes, '>', '/');

      if (str[i] == '/') // if this node has nothing inside
      {
        i++; // skip /
        i++; // skip >
        return; 
      }

      i++; // skip >

      // temporary. to include all whitespaces into value, if any
      int tempI = i;

      SkipSpaces(str, ref tempI);

      if (str[tempI] == '<')
      {
        i = tempI;

        while (str[i + 1] != '/') // parse subnodes
        {
          i++; // skip <
          subNodes.Add(new NanoXMLNode(str, ref i));

          SkipSpaces(str, ref i);

          if (i >= str.Length)
            return; // EOF

          if (str[i] != '<')
            throw new XMLParsingException("Unexpected token");
        }

        i++; // skip <
      }
      else // parse value
      {
        value = GetValue(str, ref i, '<', '\0', false);
        i++; // skip <

        if (str[i] != '/')
          throw new XMLParsingException("Invalid ending on tag " + name);
      }

      i++; // skip /
      SkipSpaces(str, ref i);

      string endName = GetValue(str, ref i, '>', '\0', true);
      if (endName != name)
        throw new XMLParsingException("Start/end tag name mismatch: " + name + " and " + endName);
      SkipSpaces(str, ref i);

      if (str[i] != '>')
        throw new XMLParsingException("Invalid ending on tag " + name);

      i++; // skip >
    }
    /// <summary>
    /// Element value
    /// </summary>
    public string Value
    {
      get { return value; }
    }
    /// <summary>
    /// Element name
    /// </summary>
    public string Name
    {
      get { return name; }
    }
    /// <summary>
    /// List of subelements
    /// </summary>
    public IEnumerable<NanoXMLNode> SubNodes
    {
      get { return subNodes; }
    }
    /// <summary>
    /// List of attributes
    /// </summary>
    public IEnumerable<NanoXMLAttribute> Attributes
    {
      get { return attributes; }
    }
    /// <summary>
    /// Returns subelement by given name
    /// </summary>
    /// <param name="nodeName">Name of subelement to get</param>
    /// <returns>First subelement with given name or NULL if no such element</returns>
    public NanoXMLNode this[string nodeName]
    {
      get
      {
        foreach (NanoXMLNode nanoXmlNode in subNodes)
          if (nanoXmlNode.name == nodeName)
            return nanoXmlNode;

        return null;
      }
    }
    /// <summary>
    /// Returns attribute by given name
    /// </summary>
    /// <param name="attributeName">Attribute name to get</param>
    /// <returns><see cref="NanoXMLAttribute"/> with given name or null if no such attribute</returns>
    public NanoXMLAttribute GetAttribute(string attributeName)
    {
            foreach (NanoXMLAttribute nanoXmlAttribute in attributes)
            {
                if (nanoXmlAttribute.Name == attributeName)
                { 
                    return nanoXmlAttribute;
                }
            }
            return null;
    }
  }

  /// <summary>
  /// XML element attribute
  /// </summary>
  class NanoXMLAttribute
  {
    private string name;
    private string value;
    /// <summary>
    /// Attribute name
    /// </summary>
    public string Name
    {
      get { return name; }
    }
    /// <summary>
    /// Attribtue value
    /// </summary>
    public string Value
    {
      get { return value; }
    }

    internal NanoXMLAttribute(string name, string value)
    {
      this.name = name;
      this.value = value;
    }
  }

  class XMLParsingException: Exception
  {
    public XMLParsingException(string message) : base(message)
    {
    }
  }
}