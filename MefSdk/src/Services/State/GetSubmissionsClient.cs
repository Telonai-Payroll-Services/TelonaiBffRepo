namespace MeF.Client.Services.StateServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    ///   This class is the service client for invoking the GetSubmissions service.
    /// </summary>
    public sealed class GetSubmissionsClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetSubmissionsClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionsClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetSubmissionsClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionsClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsClient" /> class for in memory processing.
        /// </summary>        
        public GetSubmissionsClient()
        {
            MeFLog.LogInfo(string.Format("GetSubmissionsClient.ctor for in memory processing"));
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionIdList">The list of submission ids.</param>
        /// <returns>A GetSubmissionsResult, see <see cref = "GetSubmissionsResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetSubmissionsResult Invoke(ServiceContext serviceContext, string[] submissionIdList)
        {
            MeFLog.LogInfo("GetSubmissionsClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDListValid(submissionIdList, "SubmissionIdList", "");

            var proxy = new WCFGetSubmissionsClient();
            var responseData = new GetSubmissionsResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetSubmissions", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;
                MessageBuilder.AuditRequest(mefheader, null);
                MessageBuilder.AuditRequestData(submissionIdList.ToList());

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetSubmissionsRequestType { SubmissionIdList = submissionIdList };
                    responseData = proxy.GetSubmissions(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.SubmissionsAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetSubmissionsResult();               
                
                result.ErrorList = responseData.SubmissionErrorList;
                result.IRSDataList = responseData.IRSDataForStateSubmissionList;

                if (!result.ErrorList.Cnt.Equals("0"))
                {
                    foreach (var err in result.ErrorList.SubmissionError)
                    {
                        MeFLog.LogInfo("Submission ID with Error : " + err.SubmissionId);
                        MeFLog.LogInfo("Error Classification : " + err.ErrorClassificationDetail.ErrorClassificationCd);
                        MeFLog.LogInfo("Error Code : " + err.ErrorClassificationDetail.ErrorMessageCd);
                        MeFLog.LogInfo("Error Message : " + err.ErrorClassificationDetail.ErrorMessageTxt);
                    }
                }

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

                this.AuditResponseWithBody(mefheader, serviceContext, result.IRSDataList, result.ErrorList);
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
            string fileSubmissionID, IRSDataForStateSubmissionListType iRSDataList)
        {
            MeFLog.LogInfo(string.Format("Checking for matching SubmissionID '{0}' in IrsData ", fileSubmissionID));
            foreach (var s in iRSDataList.IRSDataForStateSubmission)
            {
                if (fileSubmissionID == s.SubmissionId)
                {
                    MeFLog.LogInfo(string.Format("Found matching submission Id in IrsData '{0}'", s.SubmissionId));
                }
                return s;
            }
            MeFLog.LogInfo("No matching data found in IrsDataList");
            return null;
        }

        private StateSubmissionContainer GetStateSubmissionContainer(
            string outerZip, GetSubmissionsResponseType response)
        {
            Boolean zipSlip = false;

            var innerpath = Path.Combine(this.zipFolderLocation, outerZip);
            var innerDirectory = new DirectoryInfo(innerpath);
            var zipFiles = innerDirectory.GetFiles("*.zip");
            MeFLog.LogInfo("Checking for inner zips");

            if (zipFiles.ContainsAtLeast(1))
            {
                var stateContainer = new StateSubmissionContainer();

                foreach (var innerZip in zipFiles)
                {
                    MeFLog.LogInfo(string.Format("Found inner zip: {0}", innerZip.FullName));
                    var fileSubmissionID = Path.GetFileNameWithoutExtension(innerZip.FullName);
                    
                    if (ZipStreamReader.ZipSlipCheck(innerZip, fileSubmissionID))
                    {
                        zipSlip = true;
                        continue;
                    }

                    this.ExplodeInnerZip(innerZip, fileSubmissionID, outerZip);
                    var irsData = this.FindMatchingIRSData(fileSubmissionID, response.IRSDataForStateSubmissionList);

                    var submissionPath = Path.Combine(innerpath, fileSubmissionID);
                    var stateArchive = new StateSubmissionArchive();
                    stateArchive.archiveOutputLocation = innerZip.FullName;
                    stateArchive.submissionId = fileSubmissionID;
                    stateArchive.zipFileName = innerZip.Name;
                    if (irsData != null)
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

                        stateContainer.AddStateSubmissionData(stateData);
                    }
                    else
                    {
                        MeFLog.Write("IRSData not matching reponse attachment contents");
                        throw new ToolkitException("IRSData not matching reponse attachment contents");
                    }
                }
                if (zipSlip)
                {
                    throw new Exception("One or more inner zip Entry is outside of the target directory.");
                }
                return stateContainer;
            }
            else
            {
                MeFLog.Write("Response Data is Null.");
                return null;
            }
        }

        private StateSubmissionContainer GetStateSubmissionContainer(
            Dictionary<string, byte[]> innerzipsmap, GetSubmissionsResponseType responseData)
        {
            MeFLog.LogInfo(" Count of innerzips " + responseData.IRSDataForStateSubmissionList.Cnt);
            
            var stateContainer = new StateSubmissionContainer();

            if (Int32.Parse(responseData.IRSDataForStateSubmissionList.Cnt) >= 1)
            {
                foreach (var iData in responseData.IRSDataForStateSubmissionList.IRSDataForStateSubmission)
                {
                    var attachments = new List<BinaryAttachment>();
                    var irsattachments = new List<BinaryAttachment>();

                    var fileSubmissionID = iData.SubmissionId;

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

                        var stateData = new StateSubmissionData(iData, stateArchive);                        
                        stateContainer.AddStateSubmissionData(stateData);
                        
                    }
                    else
                    {
                        MeFLog.Write("IRSData not matching reponse attachment contents");
                    }
                    
                }
                return stateContainer;
            }
            else
            {
                MeFLog.LogInfo("Response Data has no attachments.");
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