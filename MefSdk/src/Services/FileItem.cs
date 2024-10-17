using System.IO;
using System.Xml;

namespace MeF.Client.Services
{
    /// <summary>
    /// Contains properties and extension methods for working different types of data.
    /// </summary>
    public class FileItem
    {
        #region Private member fields

        private string _Filename; //Fullpath
        private byte[] bytes;

        #endregion Private member fields

        public byte[] getBytes()
        {
            return bytes;
        }

        #region Constructors and cleanup methods

        /// <summary>
        /// Initializes a new instance of the <see cref="FileItem"/> class.
        /// </summary>
        public FileItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileItem"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public FileItem(string fileName)
        {
            _Filename = fileName;
        }

        public FileItem(byte[] bytes)
        {
            this.bytes = bytes;

        }

        #endregion Constructors and cleanup methods

        #region Class attributes

        /// <summary>
        /// Gets or sets the Filename
        /// </summary>
        public string Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        #endregion Class attributes

        /// <summary>
        /// Gets the XML string.
        /// </summary>
        /// <returns>A string representation of the XmlDocument</returns>
        public string GetXMLString()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(this.Filename);
            return xdoc.OuterXml;
        }

        /// <summary>
        /// Gets the XML element.
        /// </summary>
        /// <returns>The root element from the <see cref="XmlDocument"/>.</returns>
        public XmlElement GetXMLElement()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(this.Filename);
            return xdoc.DocumentElement;
        }

        /// <summary>
        /// Gets the XML document.
        /// </summary>
        /// <returns>The <see cref="XmlDocument"/> for the FileItem</returns>
        public XmlDocument GetXMLDocument()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(this.Filename);
            return xdoc;
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <returns>A <see cref="StreamReader"/> for the FileItem.</returns>
        public StreamReader GetStream()
        {
            StreamReader ioFile = new StreamReader(Filename);
            return ioFile;
        }
    }
}