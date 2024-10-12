using System.IO;
using MeF.Client.Logging;
//using MeF.Client.Proxy.Wse3;
//using MeF.Client.Security.Tokens;
//using MeF.Client.Services;
using MeF.Client.Util;
//using Microsoft.Web.Services3;
using System.Collections.Generic;
using MeFWCFClient.MeFMSIServices;
using WCFClient;
using System;

namespace MeF.Client.Helpers
{
    public sealed class ResponseHandler 
    {
        
        private MeFHeaderType mefHeader;

        #region IResponseHandler Members

        

        public MeFHeaderType GetMefHeader()
        {
            return mefHeader;
        }

        private void GetAttachments(byte[] file, string contentType, string zipFolder, string zipFilename)
        {
            
            MeFLog.LogInfo("ResponseHandler: SavingAttachment");
            CheckDirectory(zipFolder);
            AttachmentUtil.ExtractAttachment(file, contentType, zipFolder, zipFilename);
        }

        private static void CreateSubfolder(string baseFolder, string folderName)
        {
        }

        private void CheckDirectory(string zipFolder)
        {
            if (!Directory.Exists(zipFolder))
            {
                Directory.CreateDirectory(zipFolder);
            }
        }

        public Dictionary<string, byte[]> getmapofContents(byte[] file)
        {       
            MeFLog.LogInfo("ResponseHandler: In-memory Unzipping");
                return ZipStreamReader.UnzipGetmap(file);         
        }

        public string HandleResponseSaveOnly( MeFHeaderType mef, byte[] file, string contentType, string folderPath, string messageId)
        {
            
            string extractedFile = string.Empty;
            CheckDirectory(folderPath);
            
            mefHeader = mef;
            GetAttachments(file, contentType, folderPath, messageId);
            MeFLog.LogInfo("ResponseHandler: Finished Saving ZipFile");
            extractedFile = folderPath + messageId + ".zip";
            return extractedFile;
        }

        public string HandleResponse( MeFHeaderType mef, byte[] file, string contentType, string folderPath, string messageId)
        {
            
            string extractedFile = string.Empty;
            CheckDirectory(folderPath);
            
            
            mefHeader = mef;
            GetAttachments(file, contentType, folderPath, messageId);
            extractedFile = ZipStreamReader.UnzipSaveGetPath(file, folderPath);
            return extractedFile;
        }

       

        

        #endregion IResponseHandler Members
    }
}