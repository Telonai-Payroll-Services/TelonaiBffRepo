using System;
using System.Collections.Generic;
using System.IO;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Util;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Contains properties and methods related to handling SubmissionArchives.
    /// </summary>
    public class SubmissionArchive
    {
        public DateTime Timestamp;
        public Boolean isFileBased = true ;

        /// <summary>
        /// Gets or sets the archive output location.
        /// </summary>
        /// <value>The archive output location.</value>
        public string archiveOutputLocation { get; set; }

        /// <summary>
        /// Gets or sets the name of the zip file.
        /// </summary>
        /// <value>The name of the zip file.</value>
        public string zipFileName { get; set; }

        /// <summary>
        /// Gets or sets the submission id.
        /// </summary>
        /// <value>The submission id.</value>
        public string submissionId { get; set; }

        /// <summary>
        /// Gets or sets the submission manifest.
        /// </summary>
        /// <value>The submission manifest.</value>
        public SubmissionManifest submissionManifest { get; set; }

        /// <summary>
        /// Gets or sets the submission XML.
        /// </summary>
        /// <value>The submission XML.</value>
        public SubmissionXml submissionXml { get; set; }

        /// <summary>
        /// Gets or sets the binary attachments.
        /// </summary>
        /// <value>The binary attachments.</value>
        public List<BinaryAttachment> binaryAttachments { get; set; }

        /// <summary>
        /// Gets the zip file.
        /// </summary>
        /// <returns></returns>
        public FileStream GetZipFile()
        {
            FileStream zipFile = new FileStream(zipFileName, FileMode.Open);
            return zipFile;
        }

        /// <summary>
        /// Gets the zip file bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetZipFileBytes()
        {
            return StreamingHelper.Chunk(zipFileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionArchive"/> class.
        /// </summary>
        public SubmissionArchive()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionArchive"/> class.
        /// </summary>
        /// <param name="SubmissionID">The submission ID.</param>
        /// <param name="SubmissionManifest">The submission manifest.</param>
        /// <param name="SubmissionXML">The submission XML.</param>
        /// <param name="BinaryAttachments">The binary attachments.</param>
        /// <param name="ArchiveOutputLocation">The archive output location.</param>
        public SubmissionArchive(string SubmissionID, SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML, List<BinaryAttachment> BinaryAttachments, string ArchiveOutputLocation)
        {
            submissionManifest = SubmissionManifest;
            submissionXml = SubmissionXML;
            binaryAttachments = BinaryAttachments;
            archiveOutputLocation = ArchiveOutputLocation;
            this.submissionId = SubmissionID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionArchive"/> class.
        /// </summary>
        /// <param name="SubmissionID">The submission ID.</param>
        /// <param name="SubmissionManifest">The submission manifest.</param>
        /// <param name="SubmissionXML">The submission XML.</param>
        /// <param name="BinaryAttachments">The binary attachments.</param>
        public SubmissionArchive(string SubmissionID, SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML, List<BinaryAttachment> BinaryAttachments)
        {
            submissionManifest = SubmissionManifest;
            submissionXml = SubmissionXML;
            binaryAttachments = BinaryAttachments;            
            this.submissionId = SubmissionID;
            this.isFileBased = false;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionArchive"/> class.
        /// </summary>
        /// <param name="preCreatedZipArchiveFile">The pre created zip archive file.</param>
        public SubmissionArchive(FileStream preCreatedZipArchiveFile)
        {
            if (preCreatedZipArchiveFile == null)
            {
                string msg = "File is empty or null" +
                "; SubmissionArchive; zip file";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            if (preCreatedZipArchiveFile.Name.EndsWith(".zip"))
            {
                submissionId = GetFilename(preCreatedZipArchiveFile.Name).Replace(".zip", "");
            }
            else
            {
                submissionId = GetFilename(preCreatedZipArchiveFile.Name);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionArchive"/> class.
        /// </summary>
        /// <param name="preCreatedZipArchiveFilename">The pre created zip archive filename.</param>
        public SubmissionArchive(string preCreatedZipArchiveFilename)
        {
            if (string.IsNullOrEmpty(preCreatedZipArchiveFilename))
            {
                string msg = "Filename is empty or null" +
                "; SubmissionArchive; zip filename";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException()); ;
                throw new ToolkitException(msg);
            }
            if (preCreatedZipArchiveFilename.EndsWith(".zip"))
            {
                submissionId = GetFilename(preCreatedZipArchiveFilename).Replace(".zip", "");
            }
            else
            {
                string msg = "Filename extension is not a Zip" +
                "; SubmissionArchive; zip filename";
                MeFLog.Write(msg);
                throw new ToolkitException(msg);
            }
        }

        private string GetFilename(string fullPath)
        {
            int index = fullPath.LastIndexOf(@"\");
            if (index == -1)
                index = fullPath.LastIndexOf(@"/");
            if (index == -1)
                return fullPath.ToLower();
            else
                return fullPath.Substring(index + 1).ToLower();
        }

        public byte[] getBytes() 
        {

		if(this.isFileBased){
            if (this.zipFileName == null)
            {
                throw new ToolkitException("No Zip File found");
			}

			byte[] bytes = File.ReadAllBytes(this.zipFileName);
			return bytes;
			
		} else {
            return ZipStreamReader.zipSubmissionDataToBytes(this);
		}
	}

        public Boolean getFilebased()
        {
            return isFileBased;
        }

        
    }
}