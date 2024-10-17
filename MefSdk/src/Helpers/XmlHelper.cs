using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MeF.Client.Helpers
{
    /// <summary>
    /// Helper class to serialize/deserialize specific MeF types to .NET friendly data
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Deserializes the specified x doc.
        /// </summary>
        /// <param name="xDoc">The x doc.</param>
        /// <param name="type">The type.</param>
        /// <param name="targetNamespace">The target namespace.</param>
        /// <returns>.</returns>
        public static object Deserialize(XmlDocument xDoc, Type type, string targetNamespace)
        {
            XmlSerializer serializer;
            if (targetNamespace == null)
            {
                serializer = new XmlSerializer(type);
            }
            else
            {
                serializer = new XmlSerializer(type, targetNamespace);
            }

            XmlNodeReader xnr = new XmlNodeReader(xDoc);
            object obj = serializer.Deserialize(xnr);

            return obj;
        }

        /// <summary>
        /// Serializes to X document.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="typeToSerializeInto">The type to serialize into.</param>
        /// <param name="targetNamespace">The target namespace.</param>
        /// <returns>An XDocument</returns>
        /*
        public static XDocument SerializeToXDocument(object objectToSerialize, Type typeToSerializeInto, string targetNamespace)
        {
            XmlSerializer serializer;
            //XmlAttributeOverrides xmlAttOver = new XmlAttributeOverrides();

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, "");

            XDocument xDoc;
            if (targetNamespace == null)
            {
                serializer = new XmlSerializer(typeToSerializeInto);
            }
            else
            {
                serializer = new XmlSerializer(typeToSerializeInto, targetNamespace);
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlTextWriter xtr = new XmlTextWriter(memStream, Encoding.UTF8);
                xtr.Namespaces = false;
                serializer.Serialize(xtr, objectToSerialize, namespaces);
                memStream.Position = 0;

                using (XmlReader reader = XmlReader.Create(memStream))
                {
                    xDoc = XDocument.Load(reader);
                }
            }

            return xDoc;
        }
        */

        /// <summary>
        /// Serializes to XML document.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="typeToSerializeInto">The type to serialize into.</param>
        /// <param name="targetNamespace">The target namespace.</param>
        /// <returns>An XmlDocument</returns>
        public static XmlDocument SerializeToXmlDocument(object objectToSerialize, Type typeToSerializeInto, string targetNamespace)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, ApiHelper.EfileNS);
            //namespaces.Add("schemaLocation", ApiHelper.EfileNS);
            XmlSerializer serializer;

            if (targetNamespace == null)
            {
                serializer = new XmlSerializer(typeToSerializeInto);
            }
            else
            {
                serializer = new XmlSerializer(typeToSerializeInto, targetNamespace);
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            using (MemoryStream memStream = new MemoryStream())
            {
                XmlWriter xtr = XmlWriter.Create(memStream, settings);

                serializer.Serialize(xtr, objectToSerialize, namespaces);
                memStream.Position = 0;
                xDoc.Load(memStream);
            }

            return xDoc;
        }

        /// <summary>
        /// Gets the XML doc from X doc.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        //public static XmlDocument GetXmlDocFromXDoc(XDocument doc)
        //{
        //    using (MemoryStream memStream = new MemoryStream())
        //    {
        //        XmlWriter xmlWriter = XmlWriter.Create(memStream);
        //        doc.Save(xmlWriter);
        //        //Close the writer so that the stream gets saved correctly
        //        xmlWriter.Close();

        //        //rewind the stream so it can be loaded into an XmlDocument
        //        memStream.Position = 0;
        //        XmlDocument xmlDoc = new XmlDocument();
        //        xmlDoc.Load(memStream);
        //        memStream.Flush();
        //        memStream.Close();
        //        return xmlDoc;
        //    }
        //}

        /// <summary>
        /// Extension method that returns a XElement constructed from an Xml Document
        /// </summary>
        /// <param name="xmlDoc">XmlDocument to be converted</param>
        /// <returns>XElement</returns>
        public static XElement GetXElement(this XmlDocument xmlDoc)
        {
            XDocument xDoc = new XDocument();

            using (XmlWriter xmlWriter = xDoc.CreateWriter())

                xmlDoc.WriteTo(xmlWriter);

            return xDoc.Root;
        }

        /// <summary>
        /// Gets the X element.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static XElement GetXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();

            using (XmlWriter xmlWriter = xDoc.CreateWriter())

                node.WriteTo(xmlWriter);

            return xDoc.Root;
        }

        /// <summary>
        /// Gets the XML node.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(xmlReader);

                return xmlDoc;
            }
        }

        /// <summary>
        /// Pretty Print the input XML string, such as adding indentations to each level of elements
        /// and carriage return to each line
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns>New formatted XML string</returns>
        //public static String FormatNicely(String xmlText)
        //{
        //    if (xmlText == null || xmlText.Trim().Length == 0)
        //        return "";

        //    String result = "";

        //    MemoryStream memStream = new MemoryStream();
        //    XmlTextWriter xmlWriter = new XmlTextWriter(memStream, Encoding.Unicode);
        //    XmlDocument xmlDoc = new XmlDocument();

        //    try
        //    {
        //        // Load the XmlDocument with the XML.
        //        xmlDoc.LoadXml(xmlText);

        //        xmlWriter.Formatting = Formatting.Indented;

        //        // Write the XML into a formatting XmlTextWriter
        //        xmlDoc.WriteContentTo(xmlWriter);
        //        xmlWriter.Flush();
        //        memStream.Flush();

        //        // Have to rewind the MemoryStream in order to read
        //        // its contents.
        //        memStream.Position = 0;

        //        // Read MemoryStream contents into a StreamReader.
        //        StreamReader streamReader = new StreamReader(memStream);

        //        // Extract the text from the StreamReader.
        //        String FormattedXML = streamReader.ReadToEnd();

        //        result = FormattedXML;
        //    }
        //    catch (Exception)
        //    {
        //        // Return the original unchanged.
        //        result = xmlText;
        //    }
        //    finally
        //    {
        //        memStream.Close();
        //        xmlWriter.Close();
        //    }
        //    return result;
        //}
    }
}