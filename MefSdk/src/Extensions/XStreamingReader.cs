﻿/*--------------------------------------------------------------------------
* XStreamingReader
* ver 1.0.0.0 (Jul. 15th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://xstreamingreader.codeplex.com/
*--------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Xml.Linq
{
    /// <summary>
    ///
    /// </summary>
    public class XStreamingReader
    {
        /// <summary>
        /// Loads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static XStreamingReader Load(Stream stream)
        {
            return new XStreamingReader(() => XmlReader.Create(stream));
        }

        /// <summary>
        /// Loads the specified URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static XStreamingReader Load(string uri)
        {
            return new XStreamingReader(() => XmlReader.Create(uri));
        }

        /// <summary>
        /// Loads the specified text reader.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <returns></returns>
        public static XStreamingReader Load(TextReader textReader)
        {
            return new XStreamingReader(() => XmlReader.Create(textReader));
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public static XStreamingReader Load(XmlReader reader)
        {
            return new XStreamingReader(() => reader);
        }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static XStreamingReader Parse(string text)
        {
            return new XStreamingReader(() => XmlReader.Create(new StringReader(text)));
        }

        // instance

        readonly Func<XmlReader> readerFactory;

        private XStreamingReader(Func<XmlReader> readerFactory)
        {
            this.readerFactory = readerFactory;
        }

        private void MoveToNextElement(XmlReader reader)
        {
            while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }
        }

        private void MoveToNextFollowing(XmlReader reader)
        {
            var depth = reader.Depth;
            if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
            {
                while (reader.Read() && depth < reader.Depth) { }
            }
            MoveToNextElement(reader);
        }

        /// <summary>
        /// Attributes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public XAttribute Attribute(XName name)
        {
            return Attributes(name).FirstOrDefault();
        }

        /// <summary>
        /// Attributeses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<XAttribute> Attributes(XName name)
        {
            return Attributes().Where(x => x.Name == name);
        }

        /// <summary>
        /// Attributeses this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XAttribute> Attributes()
        {
            using (var reader = readerFactory())
            {
                reader.MoveToContent();
                while (reader.MoveToNextAttribute())
                {
                    XNamespace ns = reader.NamespaceURI;
                    XName name = ns + reader.Name.Split(':').Last();
                    yield return new XAttribute(name, reader.Value);
                }
            }
        }

        /// <summary>
        /// Elements the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public XElement Element(XName name)
        {
            return Elements(name).FirstOrDefault();
        }

        /// <summary>
        /// Elementses this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XElement> Elements()
        {
            using (var reader = readerFactory())
            {
                reader.MoveToContent();
                MoveToNextElement(reader);
                while (!reader.EOF)
                {
                    yield return XElement.Load(reader.ReadSubtree());
                    MoveToNextFollowing(reader);
                }
            }
        }

        /// <summary>
        /// Elementses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<XElement> Elements(XName name)
        {
            return Elements().Where(x => x.Name == name);
        }

        /// <summary>
        /// Descendantses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<XElement> Descendants(XName name)
        {
            using (var reader = readerFactory())
            {
                while (reader.ReadToFollowing(name.LocalName, name.NamespaceName))
                {
                    yield return XElement.Load(reader.ReadSubtree());
                }
            }
        }
    }
}