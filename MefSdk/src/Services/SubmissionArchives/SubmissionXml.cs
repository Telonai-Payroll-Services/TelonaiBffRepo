using System.IO;
using MeF.Client.Exceptions;
using MeF.Client.Logging;
using System.Text;
using MeF.Client.Util;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Contains properties and methods related to handling SubmissionXml.
    /// </summary>
    public class SubmissionXml : ISubmissionArchiveItem
    {
        private MeFLog Log = MeFLog.Instance();
        private string fileName;
        private string xmlData;
        private static readonly Encoding LocalEncoding = Encoding.UTF8;
        private bool isFileBased = true;                      

        private FileStream file;        

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionXml"/> class.
        /// </summary>
        /// <param name="filenamewithextension">Name of the file with extension</param>
        /// /// <param name="xmlstring">Submission xml as string</param>
        public SubmissionXml(string filenamewithextension, string xmlstring)
        {
            if (!ZipStreamReader.isValidPath(Path.GetFileName(filenamewithextension)))
            {
                string msg = "File Name is invalid; Contains invalid characters";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }

            this.isFileBased = false;
            this.fileName = filenamewithextension;
            this.xmlData = xmlstring;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionXml"/> class.
        /// </summary>
        /// <param name="filenamewithextension">Name of the file with extension</param>
        /// /// <param name="inStream">Submission xml as memory Stream</param>
        public SubmissionXml(string filenamewithextension, MemoryStream inStream)
        {
            if (inStream == null)
            {
                string msg = "MeFClientSDK000004:  Memory stream is null";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            if (!ZipStreamReader.isValidPath(Path.GetFileName(filenamewithextension)))
            {
                string msg = "File Name is invalid; Contains invalid characters";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            this.isFileBased = false;
            this.fileName = filenamewithextension;
            this.xmlData = LocalEncoding.GetString(inStream.ToArray()); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionXml"/> class.
        /// </summary>
        /// <param name="fileNamewithpath">Name of the file.</param>
        public SubmissionXml(string fileNamewithpath)
        {
            if (!File.Exists(fileNamewithpath))
            {
                
                string msg = "MeFClientSDK000004:  Cannot access file" +
                        "; SubmissionXML; " + fileName;
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            if (!ZipStreamReader.isValidPath(Path.GetFileName(fileNamewithpath)))
            {
                string msg = "File Name is invalid; Contains invalid characters";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            this.fileName = fileNamewithpath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionXml"/> class.
        /// </summary>
        /// <param name="aFile">A file.</param>
        public SubmissionXml(FileStream aFile)
        {
            if (!File.Exists(aFile.Name))
            {
                
                string msg = "MeFClientSDK000004:  Cannot access file" +
                   "; SubmissionXML; " + (aFile == null ? "" : aFile.Name);
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            this.fileName = aFile.Name;
            this.file = aFile;
        }


        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <returns></returns>
        public string getFileName()
        {
            return this.fileName;
        }

        public void setFileName(string filenametoset)
        {
            this.fileName = filenametoset;
        }

        public FileStream GetFile()
        {
            if (!isFileBased)
            {
                string msg = "MeFClientSDK000051:  File based method called on in-memory based object instance." +
                        "; SubmissionManifest; " + fileName;
                MeFLog.LogError("MeFClientSDK000051", msg, new IOException());
                throw new ToolkitException(msg);
            }

            FileStream aFile = new FileStream(fileName, FileMode.Open);
            this.file = aFile;
            return this.file;
        }

        public string GetXMLString()
        {
            if (isFileBased)
            {
                System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
                xdoc.Load(this.fileName);
                return xdoc.OuterXml;
            }
            else
            {
                return this.xmlData;
            }
            
        }          

        public MemoryStream getXmlDataAsStream()
        {
            if (isFileBased)
            {
                string msg = "MeFClientSDK000052:  Memory based method called on file based object instance." +
                        "; SubmissionManifest; " + fileName;
                MeFLog.LogError("MeFClientSDK000052", msg, new IOException());
                throw new ToolkitException(msg);
            }

            return new MemoryStream(LocalEncoding.GetBytes(this.xmlData));

        }
    }
}