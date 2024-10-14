using System;
using System.Collections.Generic;
using System.IO;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Services.InputComposition;
using MeF.Client.Util;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    /// Helper class used to create SubmissionArchives and construct SubmissionData
    /// </summary>
    public class SubmissionContainer
    {
        /// <summary>
        /// Gets or sets the post marked submission archives.
        /// </summary>
        /// <value>The post marked submission archives.</value>
        public List<PostmarkedSubmissionArchive> PostMarkedSubmissionArchives { get; private set; }

        public Boolean isFileBased = true;

        /// <summary>
        /// Gets or sets the submission archives.
        /// </summary>
        /// <value>The submission archives.</value>
        public List<SubmissionArchive> SubmissionArchives { get; private set; }

        /// <summary>
        /// Gets or sets the archive output location.
        /// </summary>
        /// <value>The archive output location.</value>
        public string ArchiveOutputLocation { get; set; }

        /// <summary>
        /// Gets or sets the zip filename.
        /// </summary>
        /// <value>The zip filename.</value>
        public string ZipFilename { get; set; }

        private SubmissionContainer()
        {
            ArchiveOutputLocation = ".\\SubmissionFiles";
            PostMarkedSubmissionArchives = new List<PostmarkedSubmissionArchive>();
            SubmissionArchives = new List<SubmissionArchive>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionContainer"/> class.
        /// </summary>
        /// <param name="submissionArchives">The submission archives.</param>
        /// <param name="submissionFilesLocation">The submission files location.</param>
        public SubmissionContainer(List<SubmissionArchive> submissionArchives, string submissionFilesLocation)
        {
            MeFLog.LogInfo(string.Format("SubmissionContainer.ctor with FilesLocation: {0}", submissionFilesLocation));
            ArchiveOutputLocation = submissionFilesLocation;
            SubmissionArchives = submissionArchives;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionContainer"/> class.
        /// </summary>
        /// <param name="postArchs">The post archs.</param>
        /// <param name="submissionFilesLocation">The submission files location.</param>
        public SubmissionContainer(List<PostmarkedSubmissionArchive> postArchs, string submissionFilesLocation)
        {
            MeFLog.LogInfo(string.Format("SubmissionContainer.ctor with FilesLocation: {0}", submissionFilesLocation));
            ArchiveOutputLocation = submissionFilesLocation;
            PostMarkedSubmissionArchives = postArchs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionContainer"/> class.
        /// </summary>
        /// <param name="postArchs">The post archs.</param>        
        public SubmissionContainer(List<PostmarkedSubmissionArchive> postArchs)
        {
            MeFLog.LogInfo("SubmissionContainer.ctor In-Memory");            
            PostMarkedSubmissionArchives = postArchs;
            this.isFileBased = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionContainer"/> class.
        /// </summary>
        /// <param name="postMarkedArchives">The post marked archives.</param>
        /// <param name="submissionArchives">The submission archives.</param>
        /// <param name="archiveOutputLocation">The archive output location on file system.</param>
        public SubmissionContainer(List<PostmarkedSubmissionArchive> postMarkedArchives, List<SubmissionArchive> submissionArchives, string archiveOutputLocation)
        {
            // TODO: Complete member initialization
            this.PostMarkedSubmissionArchives = postMarkedArchives;
            this.SubmissionArchives = submissionArchives;
            this.ArchiveOutputLocation = archiveOutputLocation;
        }

        /// <summary>
        /// Gets the zip file.
        /// </summary>
        /// <returns>The FileStream</returns>
        public FileStream GetZipFile()
        {
            return new FileStream(this.ZipFilename, FileMode.Open);
        }

        /// <summary>
        /// Gets the submissions attachment bytes.
        /// </summary>
        /// <returns>The attachment byte[]</returns>
        //public byte[] GetSubmissionsAttachmentBytes()
        //{
        //    //File.ReadAllBytes(this.ZipFilename);
        //    return StreamingHelper.Chunk(this.ZipFilename);
        //}

        /// <summary>
        /// Gets the submissions attachment bytes.
        /// </summary>
        /// <returns>the attachment byte[]</returns>
        public byte[] GetBytes()
        {
            if (this.isFileBased)
            {
                return ReadAllBytes(this.ZipFilename);
            }

            // Get all the innerzips
		Dictionary<String, byte[]> innerZipMap = new Dictionary<String, byte[]>();

            foreach (var archive in this.PostMarkedSubmissionArchives) 
            {
                SubmissionArchive subArchive = archive.PostMarkedArchive;
			    byte[] innerZipBytes = subArchive.getBytes();
			    String name = subArchive.submissionId + ".zip";
			    innerZipMap.Add(name, innerZipBytes);
		    }		
		
		// Now form outer zip
            byte[] zipBytes = ZipStreamReader.zipMapToBytes(innerZipMap);            
		    return zipBytes;
        }

        /// <summary>
        /// Converts the given filestructure to a byte[]
        /// </summary>
        /// <param name="FileName">Filename to process</param>
        /// <returns>The byte[]</returns>
        /*
        public static byte[] ConvertToBytes(string FileName)
        {
            //Create an instance of the FileInfo class
            FileInfo FInfo = new FileInfo(FileName);

            // Binary reader for the selected file
            using (BinaryReader br = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
            {
                // convert the file to a byte array
                byte[] data = br.ReadBytes((int)FInfo.Length);

                //close the binaryreader
                br.Close();

                //return the byte[]
                return data;
            }
        }
        */

        public static byte[] ReadAllBytes(String path)
        {
            byte[] bytes;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int index = 0;
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File too Long");
                int count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    if (n == 0)
                        throw new InvalidOperationException("End of file reached before expected");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }
    }
}