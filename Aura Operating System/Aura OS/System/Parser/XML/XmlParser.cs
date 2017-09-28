using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DZetko.Xml
{
    public class XmlParser
    {
        private string _input = String.Empty;
        private Stream _inputStream;
        private XmlDocument _xmlDocument;
        private InputType _inputType;

        public enum InputType
        {
            Text,
            File
        };

        public XmlParser(InputType inputType, string input)
        {
            _inputType = inputType;
            _input = input;
        }

        private Stream CreateStreamFromString(string content)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public XmlDocument Parse()
        {
            switch (_inputType)
            {
                case InputType.File:
                    {
                        _inputStream = new FileStream(_input, FileMode.Open, FileAccess.Read);
                        break;
                    }
                case InputType.Text:
                    {

                        _inputStream = CreateStreamFromString(_input);
                        break;
                    }
                default:
                    {
                        throw new XmlParserException("No input type selected"); 
                    }
            }

            using (StreamReader rdr = new StreamReader(_inputStream))
            {
                _xmlDocument = new XmlDocument();
                char character = Convert.ToChar(rdr.Read());
                if ((character == '<') && (rdr.Peek() == '?'))
                {
                    rdr.ReadLine();
                }

                int currentRead;
                XmlElement currentParent = null;
                while ((currentRead = rdr.Read()) >= 0)
                {
                    if ((!char.IsWhiteSpace(Convert.ToChar(currentRead)) && ((currentRead == '<') || char.IsLetterOrDigit(Convert.ToChar(currentRead)))))
                    {

                        if (char.IsLetterOrDigit(Convert.ToChar(currentRead)))
                        {
                            string elementContent = Convert.ToChar(currentRead).ToString();
                            while (rdr.Peek() != '<')
                            {
                                elementContent += Convert.ToChar(rdr.Read());
                            }
                            currentParent.Content = elementContent;
                        }
                        else
                        {
                            if (rdr.Peek() == '/')
                            {
                                rdr.Read();
                                //closing tag
                                string innerNode = ReadAlphanumericalName(rdr);

                                if (innerNode != currentParent.Name)
                                {
                                    throw new XmlParserException("Tag not matching");
                                }

                                currentParent = currentParent.Parent;
                            }
                            else if (rdr.Peek() == '!')
                            {
                                bool hasCommentEnded = false;
                                int dashCount = 0;
                                while (!hasCommentEnded)
                                {
                                    while (rdr.Peek() != '>')
                                    {
                                        if (rdr.Read() == '-') dashCount++;
                                        else dashCount = 0;
                                    }

                                    if (dashCount >= 2)
                                    {
                                        hasCommentEnded = true;
                                    }

                                    rdr.Read();
                                }
                            }
                            else
                            {
                                string innerNode = ReadAlphanumericalName(rdr);
                                XmlElement newElement = new XmlElement(innerNode, currentParent);
                                bool isClosingTag = false;

                                char nextChar = Convert.ToChar(rdr.Peek());
                                while ((nextChar != '/') && (nextChar != '>'))
                                {
                                    if (!char.IsWhiteSpace(nextChar))
                                    {
                                        XmlAttribute attribute = ReadAttribute(rdr, newElement);
                                        newElement.AddAttribute(attribute);
                                    }
                                    else
                                    {
                                        rdr.Read();
                                    }

                                    nextChar = Convert.ToChar(rdr.Peek());
                                }

                                if (rdr.Peek() == '/')
                                {
                                    isClosingTag = true;
                                }

                                rdr.Read();

                                if (currentParent == null)
                                {
                                    _xmlDocument.RootNode = newElement;
                                }
                                else
                                {
                                    currentParent.AddChild(newElement);
                                }

                                if (!isClosingTag)
                                {
                                    currentParent = newElement;
                                }
                            }
                        }
                    }
                }
                return _xmlDocument;
            }
        }

        public string ReadAlphanumericalName(StreamReader reader)
        {
            string name = String.Empty;
            while (char.IsLetterOrDigit(Convert.ToChar(reader.Peek())) || reader.Peek() == '_')
            {
                char currentChar = Convert.ToChar(reader.Read());
                name += currentChar;
            }

            return name;
        }

        public string ReadName(StreamReader reader, char stopSign)
        {
            string name = String.Empty;
            while (Convert.ToChar(reader.Peek()) != stopSign)
            {
                char currentChar = Convert.ToChar(reader.Read());
                name += currentChar;
            }

            return name;
        }

        public XmlAttribute ReadAttribute(StreamReader reader, XmlElement element)
        {
            string attributeName = ReadAlphanumericalName(reader);
            if (reader.Peek() != '=')
            {
                throw new XmlParserException("Invalid XML attribute syntax.");
            }
            reader.Read();
            if (reader.Peek() != '"')
            {
                throw new XmlParserException("Invalid XML attribute syntax.");
            }
            reader.Read();
            string attributeContent = ReadName(reader, '"');
            reader.Read();

            XmlAttribute attribute = new XmlAttribute(attributeName, attributeContent, element);
            return attribute;
        }
    }
}
