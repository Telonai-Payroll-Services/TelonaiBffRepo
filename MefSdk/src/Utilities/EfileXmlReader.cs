using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MeF.Client.Util
{
    /// <summary>
    /// Provides a translation from xml to T.
    /// </summary>
    public abstract class EfileXmlReader<T>
    {
        private string elementName;

        /// <summary>
        /// Constructs a new instance of the EfileXmlReader class.
        /// </summary>
        /// <param name="elementName">The name of the elements from which the objects will be populated.</param>
        public EfileXmlReader(string elementName)
        {
            this.elementName = elementName;
        }

        /// <summary>
        /// Translates xml into a collection of T.
        /// </summary>
        /// <param name="xml">The xml to translate.</param>
        /// <returns>a collection of T.</returns>
        public IEnumerable<T> Read(string xml)
        {
            using (StringReader xmlReader = new StringReader(xml))
            {
                XDocument xDoc = XDocument.Load(xmlReader);
                var objects = xDoc.Descendants(elementName).Select(x => ReadElement(x)).ToList();
                return objects;
            }
        }

        /// <summary>
        /// Reads an XElement into an object.
        /// </summary>
        /// <param name="xml">The XElement to translate.</param>
        /// <returns>a populated object.</returns>
        protected abstract T ReadElement(XElement xmlElement);
    }
}