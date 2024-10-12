namespace MeF.Client.Services.StateServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using ICSharpCode.SharpZipLib.Zip;
    using MeF.Client.Exceptions;
    using MeF.Client.Extensions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Services.InputComposition;
    using MeF.Client.Validation;
    using MeFWCFClient.MeFStateServices;
    using System.ServiceModel;
    using MeF.Client.Util;

    /// <summary>
    ///   This class is the service client for invoking the GetSubmission service.
    /// </summary>
    public class GetSubmissionClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetSubmissionClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetSubmissionClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionClient" /> class for in memory processing.
        /// </summary>        
        public GetSubmissionClient()
        {
            MeFLog.LogInfo(string.Format("GetSubmissionClient.ctor for in memory processing"));
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionId">The submission id.</param>
        /// <returns>A GetSubmissionResult, see <see cref = "GetSubmissionResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetSubmissionResult Invoke(ServiceContext serviceContext, string submissionId)
        {
            MeFLog.LogInfo("GetSubmissionClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDValid(submissionId, "SubmissionId", "");

            var proxy = new WCFGetSubmissionClient();
            var responseData = new GetSubmissionResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetSubmission", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                MessageBuilder.AuditRequest(mefheader, submissionId);
                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetSubmissionRequestType { SubmissionId = submissionId };
                    responseData = proxy.GetSubmission(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.SubmissionsAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetSubmissionResult();

                result.IRSData = responseData.IRSDataForStateSubmission;                    
                	
                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim().Equals(""))
                {                        
                    Dictionary<string, byte[]> innerzipsmap = this.responseHandler.getmapofContents(bytes);                                                  
                    result.StateSubmissionContainer = this.GetStateSubmissionContainer(innerzipsmap, responseData);
                }
                else
                {

                    var extractedFile = this.responseHandler.HandleResponseSaveOnly(                            
                        mefheader,
                        bytes,
                        responseData.SubmissionsAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    this.outerZipFile = mefheader.MessageID;
                    this.ExplodeZip(extractedFile);
                    result.StateSubmissionContainer = this.GetStateSubmissionContainer(this.outerZipFile, responseData);                        
                    result.AttachmentFilePath = extractedFile;                        
                }

                this.AuditResponseWithBody(mefheader, serviceContext, result);
                result.MessageID = mefheader.MessageID;
                result.RelatesTo = mefheader.RelatesTo;
                return result;
            }
            catch (TimeoutException timeProblem)
            {
                proxy.Abort();
                throw new ServiceException(Logging.Constants.SoapException, timeProblem);
            }
            catch (FaultException faultException)
            {
                MessageBuilder.AuditSoapFault(faultException);
                throw new ServiceException(Logging.Constants.SoapException, faultException);
            }
            catch (CommunicationException commProblem)
            {
                proxy.Abort();
                throw new ServiceException(Logging.Constants.SoapException, commProblem);
            }
            catch (ToolkitException e)
            {
                MessageBuilder.ProcessToolkitException(e);
                throw;
            }
            catch (Exception exception)
            {
                MessageBuilder.ProcessUnexpectedException(exception);
                throw new ToolkitException("MeFClientSDK000029", exception);
            }
            finally
            {
                proxy.Close();
            }
        }

        #endregion

        #region Methods

        private IRSDataForStateSubmissionType FindMatchingIRSData(
            string fileSubmissionID, IRSDataForStateSubmissionType iRSDataList)
        {
            MeFLog.LogInfo(string.Format("Checking for matching SubmissionID '{0}' in IrsData ", fileSubmissionID));
            if (fileSubmissionID == iRSDataList.SubmissionId)
            {
                MeFLog.LogInfo("Found matching submission Id in IrsData");
                return iRSDataList;
            }
            else
            {
                MeFLog.LogInfo("No matching submission Id found in IrsData");
                return null;
            }
        }

        private StateSubmissionContainer GetStateSubmissionContainer(
            string outerZipFilename, GetSubmissionResponseType responseData)
        {
            Boolean zipSlip = false;

            var dirInfoExtract = new DirectoryInfo(Path.Combine(this.zipFolderLocation, outerZipFilename));
            var zipFiles = dirInfoExtract.GetFiles("*.zip");
            MeFLog.LogInfo("Checking for inner zips");

            if (zipFiles.ContainsAtLeast(1))
            {
                foreach (var innerZip in zipFiles)
                {
                    MeFLog.LogInfo(string.Format("Found inner zip: {0}", innerZip.FullName));
                    var fileSubmissionID = Path.GetFileNameWithoutExtension(innerZip.FullName);

                    if (ZipStreamReader.ZipSlipCheck(innerZip, fileSubmissionID))
                    {
                        zipSlip = true;
                        continue;
                    }

                    this.ExplodeInnerZip(innerZip, fileSubmissionID, outerZipFilename);                    

                    var irsData = this.FindMatchingIRSData(fileSubmissionID, responseData.IRSDataForStateSubmission);
                    var innerpath = Path.Combine(this.zipFolderLocation, outerZipFilename);
                    var submissionPath = Path.Combine(innerpath, fileSubmissionID);
                    var stateArchive = new StateSubmissionArchive();
                    stateArchive.archiveOutputLocation = innerZip.FullName;
                    stateArchive.submissionId = fileSubmissionID;
                    stateArchive.zipFileName = innerZip.Name;
                    if (irsData != null && Directory.Exists(submissionPath))
                    {
                        MeFLog.LogInfo("Found Matching IrsData");
                        var submissionDir = new DirectoryInfo(submissionPath);
                        var manifestPath = Path.Combine(submissionPath, "manifest");
                        var xmlPath = Path.Combine(submissionPath, "xml");
                        var attachmentPath = Path.Combine(submissionPath, "attachment");
                        MeFLog.LogInfo(string.Format("Checking for submission manifest directory: {0}", manifestPath));
                        MeFLog.LogInfo(string.Format("Checking for submission xml directory: {0}", xmlPath));
                        MeFLog.LogInfo(
                            string.Format("Checking for submission attachment directory: {0}", attachmentPath));
                        var manifestDir = new DirectoryInfo(manifestPath);
                        var xmlDir = new DirectoryInfo(xmlPath);

                        if (xmlDir.Exists && manifestDir.Exists)
                        {
                            MeFLog.LogInfo("Preparing to map StateSubmissionData");

                            var manifestFile = manifestDir.GetFiles();
                            MeFLog.LogInfo(string.Format("Creating manifest from: {0}", manifestFile[0].FullName));
                            stateArchive.submissionManifest = new SubmissionManifest(manifestFile[0].FullName);
                            var xmlFile = xmlDir.GetFiles();
                            stateArchive.submissionXml = new SubmissionXml(xmlFile[0].FullName);
                            MeFLog.LogInfo(string.Format("Creating SubmissionXml from: {0}", xmlFile[0].FullName));

                            if (Directory.Exists(attachmentPath))
                            {
                                MeFLog.LogInfo(string.Format("Adding attachments from: {0}", attachmentPath));
                                var attachmentDir = new DirectoryInfo(attachmentPath);
                                var binaryFiles = attachmentDir.GetFiles();
                                var attachments = new List<BinaryAttachment>();
                                foreach (var b in binaryFiles)
                                {
                                    MeFLog.LogInfo(string.Format("Creating attachment from: {0}", b.FullName));
                                    var attachment = new BinaryAttachment(b.FullName);
                                    attachments.Add(attachment);
                                }
                                stateArchive.binaryAttachments = attachments;
                            }
                        }

                        var stateData = new StateSubmissionData(irsData, stateArchive);

                        var stateContainer = new StateSubmissionContainer();
                        stateContainer.AddStateSubmissionData(stateData);
                        return stateContainer;
                    }
                    else
                    {
                        MeFLog.Write("IRSData not matching reponse attachment contents");
                    }
                }

                if (zipSlip)
                {
                    throw new Exception("One or more inner zip Entry is outside of the target directory." );
                }

            }
            else
            {
                MeFLog.Write("Response Data is Null.");
            }
            return new StateSubmissionContainer();
        }

        private StateSubmissionContainer GetStateSubmissionContainer(
            Dictionary<string, byte[]> innerzipsmap, GetSubmissionResponseType responseData)
        {   
            var attachments = new List<BinaryAttachment>();
            var irsattachments = new List<BinaryAttachment>();

            
                var fileSubmissionID = responseData.IRSDataForStateSubmission.SubmissionId;

                MeFLog.LogInfo(" fileSubmissionID " + fileSubmissionID);

                var stateArchive = new StateSubmissionArchive();

                stateArchive.submissionId = fileSubmissionID;

                byte[] innerzip = innerzipsmap[fileSubmissionID + ".zip"]; 

                if (innerzip != null)
                {
                    MeFLog.LogInfo("Found Matching IrsData");

                    Dictionary<string, byte[]> innerzipcontents = this.responseHandler.getmapofContents(innerzip);

                    foreach (var entry in innerzipcontents)
                    {
                        MeFLog.LogInfo(" Processing " + entry.Key);

                        if (entry.Key.EndsWith("manifest.xml"))
                        {
                            byte[] manifestbytes = innerzipcontents[entry.Key];
                            var manStr = System.Text.Encoding.Default.GetString(manifestbytes);
                            SubmissionManifest subman = new SubmissionManifest(Path.GetFileName(entry.Key), manStr);                            
                            stateArchive.submissionManifest = subman;

                        }

                        if (entry.Key.Contains("xml/") && !entry.Key.Contains("irs/xml/"))
                        {
                            byte[] xmlbytes = innerzipcontents[entry.Key];
                            var xmlStr = System.Text.Encoding.Default.GetString(xmlbytes);
                            SubmissionXml subxml = new SubmissionXml(Path.GetFileName(entry.Key), xmlStr);                                                        
                            stateArchive.submissionXml = subxml;
                        }

                        if (entry.Key.Contains("attachment/") && !entry.Key.Contains("irs/attachment/"))
                        {
                            byte[] attachmentbytes = innerzipcontents[entry.Key];
                            BinaryAttachment binAtt = new BinaryAttachment(Path.GetFileName(entry.Key), attachmentbytes);
                            
                            attachments.Add(binAtt);
                        }

                        if (entry.Key.Contains("irs/xml/"))
                        {
                            byte[] xmlbytes = innerzipcontents[entry.Key];
                            var xmlStr = System.Text.Encoding.Default.GetString(xmlbytes);
                            SubmissionXml subxml = new SubmissionXml(Path.GetFileName(entry.Key), xmlStr);

                            stateArchive.IrsSubmissionXML = subxml;
                        }

                        if (entry.Key.Contains("irs/attachment/"))
                        {
                            byte[] attachmentbytes = innerzipcontents[entry.Key];
                            BinaryAttachment binAtt = new BinaryAttachment(Path.GetFileName(entry.Key), attachmentbytes);

                            irsattachments.Add(binAtt);
                        }

                    }

                    stateArchive.binaryAttachments = attachments;
                    stateArchive.IrsBinaryAttachments = irsattachments;

                    var stateData = new StateSubmissionData(responseData.IRSDataForStateSubmission, stateArchive);

                    var stateContainer = new StateSubmissionContainer();
                    stateContainer.AddStateSubmissionData(stateData);
                    return stateContainer;
                }
                else
                {
                    MeFLog.Write("IRSData not matching reponse attachment contents");
                }                
                     
            return new StateSubmissionContainer();
        }

        private void ExplodeZip(string outerZipFile)
        {
            MeFLog.LogInfo(string.Format("Extracting Zip Archive: {0}", outerZipFile));
            FastZip oFastZip = new FastZip();
            string innerZipsFolder;
            innerZipsFolder = zipFolderLocation + Path.GetFileNameWithoutExtension(outerZipFile);
            oFastZip.ExtractZip(outerZipFile, innerZipsFolder, "");
            MeFLog.LogInfo(string.Format("Finished extraction to the folder: {0}", innerZipsFolder));
        }

        private void ExplodeInnerZip(FileInfo innerZip, string submissionId, string header)
        {
            MeFLog.LogInfo(string.Format("Extracting inner zip file: {0}", innerZip.FullName));
            DirectoryInfo d = innerZip.Directory;
            DirectoryInfo extractTo = d.CreateSubdirectory(submissionId);
            FastZip oFastZip = new FastZip();
            string zipFileToExplode = innerZip.FullName;
            oFastZip.ExtractZip(zipFileToExplode, extractTo.FullName, "");
            MeFLog.LogInfo(string.Format("Finished extraction to the folder: {0}", extractTo.FullName));         
           
        }

        

       
        #endregion
    }
}