using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using MeF.Client.Logging;
using System.Collections.Generic;
using MeF.Client.Services.InputComposition;
using MeF.Client.Exceptions;
using System.Text.RegularExpressions;

namespace MeF.Client.Util
{
    /// <summary>
    ///
    /// </summary>
    internal static class ZipStreamReader
    {
       	    // private static  int buffSize = 1024;
	    private static  String MANIFEST_ENTRY = "/manifest/manifest.xml";
	    private static  String SUBMISSION_ENTRY = "/xml/";
	    private static  String ATTACHMENT_ENTRY_LOCATION = "/attachment/";
	    private static  String IRS_SUBMISSION_ENTRY = "/irs/xml/";
	    private static  String IRS_ATTACHMENT_ENTRY_LOCATION = "/irs/attachment/";
	    
	// No special characters are allowed	    
        private static Regex regex1 = new Regex("(.*?)([^\x20-\x7E])(.*?)");
        
        // Double dots not allowed - Any of "!(),-.{}" followed by / not allowed
        private static Regex regex2 = new Regex("(.*?)([.]{2,}|[ !(),-.\\{\\}]\\/)(.*?)");
        
        // No characters between the double quotes are allowed
        private static char[] specialChars = ";|[]<>^`&\"':?*".ToCharArray();
        

        /// <summary>
        /// Opens the byte array contained in the zip file and returns the contents of the first file.
        /// </summary>
        public static Stream UnzipData(byte[] zippedData)
        {
            MeFLog.LogInfo("ZipStreamReader: Unzipping response attachment");

            using (Stream zippedStream = new MemoryStream(zippedData))
            {
                ZipFile zipFile = new ZipFile(zippedStream);
                ZipEntry zipEntry = zipFile[0];

                byte[] buffer;
                using (Stream unzipStream = zipFile.GetInputStream(zipEntry))
                using (BinaryReader reader = new BinaryReader(unzipStream))
                {
                    buffer = reader.ReadBytes((int)zipEntry.Size);
                }

                // Workaround Bug in SharpZipLib 0.85.4:
                // From http://community.icsharpcode.net/forums/p/6881/20305.aspx
                // "After the call to GetInputStream, the ZipFile is not used anymore
                // and might get finalized. As a workaround, add GC.KeepAlive after
                // calling GetInputStream".

                GC.KeepAlive(zipFile);

                return new MemoryStream(buffer);
            }
        }

        //public static void UnzipAndSave(byte[] zippedData, string filePath)
        //{
        //    using (Stream zippedStream = new MemoryStream(zippedData))
        //    {
        //        ZipInputStream s = new ZipInputStream(zippedStream);

        //        ZipEntry theEntry;
        //        string virtualPath = filePath;
        //        string fileName = string.Empty;
        //        string fileExtension = string.Empty;
        //        string fileSize = string.Empty;

        //        while ((theEntry = s.GetNextEntry()) != null)
        //        {
        //            fileName = Path.GetFileName(theEntry.Name);
        //            fileExtension = Path.GetExtension(fileName);

        //            if (!string.IsNullOrEmpty(fileName))
        //            {
        //                try
        //                {
        //                    FileStream streamWriter = File.Create(virtualPath + fileName);
        //                    int size = 2048;
        //                    byte[] data = new byte[2048];

        //                    do
        //                    {
        //                        size = s.Read(data, 0, data.Length);
        //                        streamWriter.Write(data, 0, size);
        //                    } while (size > 0);

        //                    fileSize = Convert.ToDecimal(streamWriter.Length / 1024).ToString() + "KB";

        //                    streamWriter.Close();
        //                }
        //                catch (Exception ex)
        //                {
        //                    MeFLog.Write(ex.ToString());
        //                }
        //            }
        //        }

        //        s.Close();

        //    }
        //}

        public static string UnzipSaveGetPath(byte[] zippedData, string filePath)
        {

                Stream zippedStream = null;
                ZipInputStream s = null;
                string virtualPath = filePath;
                string fileName = string.Empty;
                try
                {
                    zippedStream = new MemoryStream(zippedData);
                    s = new ZipInputStream(zippedStream);

                    ZipEntry theEntry;
                    
                    string fileExtension = string.Empty;
                    string fileSize = string.Empty;

                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        fileName = Path.GetFileName(theEntry.Name);
                        fileExtension = Path.GetExtension(fileName);

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            FileStream streamWriter = null;
                            try
                            {
                                streamWriter = File.Create(virtualPath + fileName);
                                int size = 2048;
                                byte[] data = new byte[2048];

                                do
                                {
                                    size = s.Read(data, 0, data.Length);
                                    streamWriter.Write(data, 0, size);
                                } while (size > 0);

                                fileSize = Convert.ToDecimal(streamWriter.Length / 1024).ToString() + "KB";



                            }
                            catch (Exception ex)
                            {
                                MeFLog.Write(ex.ToString());
                            }
                            finally
                            {
                                streamWriter.Close();
                            }
                        }
                    }
                }
                finally
                {
                    zippedStream.Close();
                    s.Close();
                    
                }
                return virtualPath + fileName;
            
        }

        public static Dictionary<string, byte[]> UnzipGetmap(byte[] zippedData)
        {
            Dictionary<string, byte[]> map = new Dictionary<string, byte[]>();

            Stream zippedStream = null;
            ZipInputStream s = null;

            string fileName = string.Empty;
            MemoryStream outputstream = new MemoryStream();


            try
            {
                zippedStream = new MemoryStream(zippedData);
                s = new ZipInputStream(zippedStream);

                ZipEntry theEntry;

                string fileExtension = string.Empty;
                string fileSize = string.Empty;
                byte[] returnbytes;

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    fileName = theEntry.Name;
                                        
                    fileExtension = Path.GetExtension(fileName);
                    
                    if (!string.IsNullOrEmpty(fileName))
                    {   
                            int size = 2048;
                            byte[] data = new byte[2048];

                            do
                            {
                                size = s.Read(data, 0, data.Length);
                                outputstream.Write(data, 0, size);
                            } while (size > 0);

                            returnbytes = outputstream.ToArray();
                            map.Add(fileName, returnbytes);

                            outputstream.SetLength(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBuilder.ProcessUnexpectedException(ex);
                throw new ToolkitException("MeFClientSDK000029", ex);
            }
            finally
            {                
                s.Close();
                zippedStream.Close();
                outputstream.Close();

            }

            return map;

        }        


        public static byte[] zipSubmissionDataToBytes(SubmissionArchive subArchive) {
						
		Dictionary<string, byte[]> innerZipMap = new Dictionary<string, byte[]>();

		// manifest entry
		String manifestString = subArchive.submissionManifest.GetXMLString();
		innerZipMap.Add(MANIFEST_ENTRY, System.Text.Encoding.UTF8.GetBytes (manifestString));

		// submission.xml entry
		String xmlString = subArchive.submissionXml.GetXMLString();
		String xmlName = SUBMISSION_ENTRY + subArchive.submissionId + ".xml";
		innerZipMap.Add(xmlName, System.Text.Encoding.UTF8.GetBytes (xmlString));
		
		// binary attachments	
		if (subArchive.binaryAttachments != null) {
            foreach (var binAttach in subArchive.binaryAttachments)
			{
				byte[] attachBytes = binAttach.getData();
				String attachFileName = binAttach.FileName;				
				String shortName = Path.GetFileName(attachFileName);
				innerZipMap.Add(ATTACHMENT_ENTRY_LOCATION + shortName, attachBytes);
			}
		}
		
		// create additional entries for states
		if (subArchive is StateSubmissionArchive) {
			StateSubmissionArchive stateArchive = (StateSubmissionArchive)subArchive;

			// process IRS submission
			String irsXml = stateArchive.IrsSubmissionXML.GetXMLString();
			String irsXmlName = IRS_SUBMISSION_ENTRY + "irs" + ".xml";
			innerZipMap.Add(irsXmlName, System.Text.Encoding.UTF8.GetBytes (irsXml));
										
			// process IRS attachments	
			if (stateArchive.IrsBinaryAttachments != null) {
				foreach (var binAttach in stateArchive.IrsBinaryAttachments) 
                {
					byte[] attachBytes = binAttach.getData();
					String attachFileName = binAttach.FileName;
					String shortName = Path.GetFileName(attachFileName);
					innerZipMap.Add(IRS_ATTACHMENT_ENTRY_LOCATION + shortName, attachBytes);
				}
			}
		}
		
		return zipMapToBytes(innerZipMap);
	}

	public static byte[] zipMapToBytes(Dictionary<String, byte[]> contentsMap) {		
		
		MemoryStream baos = null;
		ZipOutputStream zout = null;
		
		try{
			baos = new MemoryStream();
			zout = new ZipOutputStream(baos);

            foreach (KeyValuePair<string, byte[]> entry in contentsMap)
            {
				byte[] data = contentsMap[entry.Key];
                zout.PutNextEntry(new ZipEntry(entry.Key));
                zout.Write(data, 0, data.Length);
                zout.CloseEntry();
			}

            zout.Close();
            baos.Flush();
			
		} 
        finally 
        {			
			zout.Close();
            baos.Close();
		}		

		byte[] zipBytes = baos.ToArray();
		return zipBytes;
		


		
	}

        public static Boolean ZipSlipCheck(FileInfo innerZip, string submissionId)
        {
            ZipFile zf = null;
            DirectoryInfo d = innerZip.Directory;
            string destDirectory = Path.Combine(d.FullName, submissionId);

            try
            {
                FileStream fs = File.OpenRead(innerZip.FullName);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    String entryFileName = zipEntry.Name;                    
                    
                    MeFLog.LogInfo("Zip Entry File Name = " + entryFileName);
                    if (!isValidPath(entryFileName))
                    {
                        MeFLog.LogInfo(string.Format("The inner zip for submission ID: {0} has problematic content. It is not unzipped.", submissionId));
                        MeFLog.LogInfo("Zip Entry name contains an invalid character: ");
                        return true;
                    }                   

                    if (entryFileName.StartsWith("/") )
                    {
                        entryFileName = entryFileName.Substring(1);
                        MeFLog.LogInfo("Zip Entry FileName = " + entryFileName);
                    }

                    string destFileName = Path.GetFullPath(Path.Combine(destDirectory, entryFileName));
                    MeFLog.LogInfo("destFileName = " + destFileName);
                    string fullDestDirPath = Path.GetFullPath(destDirectory + Path.DirectorySeparatorChar);
                    MeFLog.LogInfo("fullDestDirPath = " + fullDestDirPath);
                    if (!destFileName.StartsWith(fullDestDirPath))
                    {
                        MeFLog.LogInfo(string.Format("The inner zip for submission ID: {0} has problematic content. It is not unzipped.", submissionId));
                        return true;
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }

            return false;
        }

        public static Boolean isValidPath(string aPath)
        {            
            Match match1 = regex1.Match(aPath);
            if (match1.Success)
                return false;

            Match match2 = regex2.Match(aPath);
            if (match2.Success)
                return false;

            int indexOf = aPath.IndexOfAny(specialChars);
            if (indexOf != -1)
                return false;

            return true;
        }

    }
}