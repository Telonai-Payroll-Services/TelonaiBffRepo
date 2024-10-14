using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using MeF.Client.Helpers;
using MeF.Client.Logging;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Base class for EfileAttachment Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class EntityBase<T>
    {
        private static XmlSerializer serializer;

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializer(typeof(T));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize

        /// <summary>
        /// Serializes current EntityBase object into an XML document
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup into an EntityBase object
        /// </summary>
        /// <param name="xml">string workflow markup to deserialize</param>
        /// <param name="obj">Output EntityBase object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string xml, out T obj, out System.Exception exception)
        {
            exception = null;
            obj = default(T);
            try
            {
                obj = Deserialize(xml);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static bool Deserialize(string xml, out T obj)
        {
            System.Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static T Deserialize(string xml)
        {
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, ApiHelper.EfileNS);

            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(xml);
                return ((T)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        /// <summary>
        /// Serializes current EntityBase object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        protected virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        /// <summary>
        /// Saves to file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Saves to file as.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="xDoc">The x doc.</param>
        public virtual void SaveToFileAs(string fileName, XmlDocument xDoc)
        {
            XmlDeclaration xmldecl;
            xmldecl = xDoc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            xmldecl.Standalone = "yes";
            XmlElement root = xDoc.DocumentElement;
            xDoc.InsertBefore(xmldecl, root);
            xDoc.PreserveWhitespace = true;
            xDoc.Normalize();
            using (TextWriter writer = new StreamWriter(fileName, false))
            {
                xDoc.Save(writer);
                writer.Close();
            }
        }

        /// <summary>
        /// Saves the fragment to file as.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="xDoc">The x doc.</param>
        protected virtual void SaveFragmentToFileAs(string fileName, XmlDocument xDoc)
        {
            XElement root = xDoc.DocumentElement.GetXElement();

            var loaded = new XDocument();
            if (xDoc.DocumentElement != null)
                if (xDoc.DocumentElement.NamespaceURI != String.Empty)
                {
                    xDoc.LoadXml(xDoc.OuterXml.Replace(xDoc.DocumentElement.NamespaceURI, ""));
                    xDoc.DocumentElement.RemoveAllAttributes();
                    loaded = XDocument.Parse(xDoc.OuterXml);
                }

            using (TextWriter writer = new StreamWriter(fileName, false))
            {
                loaded.Save(writer);
                writer.Close();
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an EntityBase object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output EntityBase object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
        protected static bool LoadFromFile(string fileName, out T obj, out System.Exception exception)
        {
            exception = null;
            obj = default(T);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        private static bool LoadFromFile(string fileName, out T obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>EntityBase<typeparamref name="T"/>"/></returns>
        protected static T LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            catch (Exception e)
            {
                MessageBuilder.ProcessUnexpectedException(e);
                throw;
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }

        protected static T LoadFromBytes(byte[] unzippedcontent)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            string xmlString = enc.GetString(unzippedcontent);
            return Deserialize(xmlString);
        }

        #endregion Serialize/Deserialize

        private static Encoding GetEncoding()
        {
            Encoding Utf8 = new UTF8Encoding(false);
            return Utf8;
        }
    }
}