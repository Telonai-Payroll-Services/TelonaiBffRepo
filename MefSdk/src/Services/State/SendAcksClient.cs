namespace MeF.Client.Services.StateServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml;
    using MeF.Client.EfileAttachments;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;
    using MeFWCFClient.MeFStateServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the SendAcks service.
    /// </summary>
    public class SendAcksClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Constructor specifying location where response attachment will be maintained and extracted
        /// </summary>
        /// <param name = "folderName">Folder path - ensure the current user has the correct permission</param>
        public SendAcksClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("SendAcksClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Constructor accepting a Folder Object. This folder will be used to store/maintain attachments
        /// </summary>
        /// <param name = "dir">DirectoryInfo object</param>
        public SendAcksClient(DirectoryInfo dir)
        {
            MeFLog.LogInfo(string.Format("SendAcksClient.ctor with directory: {0}", dir.FullName));
            this.zipFolderLocation = dir.FullName;
        }

        /// <summary>
        ///   Constructor accepting no parameter for In-memory processing.
        /// </summary>        
        //public SendAcksClient()
        //{
        //    MeFLog.LogInfo("SendAcksClient.ctor with no parameter for In-memory processing");
        //    this.zipFolderLocation = "";
        //}

        #endregion

        #region Public Methods

        /// <summary>
        ///   Calls SendAcks from MeF StateService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "acknowledgementList">AcknowledgementList</param>
        /// <returns>Will return SendAcksResult, see <see cref = "SendAcksResult" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendAcksResult Invoke(ServiceContext serviceContext, AcknowledgementList acknowledgementList)
        {
            MeFLog.LogInfo("SendAcksClient.Invoke() with parameter AcknowledgementList");
            byte[] xmlData = null;

            if (acknowledgementList != null)
            {
                var xmlFileName = Path.GetRandomFileName();
                xmlFileName = xmlFileName.Remove(xmlFileName.LastIndexOf("."), 4);
                var fileToZip = this.zipFolderLocation + xmlFileName + ".xml";
                var zipFilename = xmlFileName + ".zip";
                MeFLog.LogDebug("SendAcksClient:  Serializing AcknowledgementList");
                Audit.Write(string.Format("AcknowledgementList Count: {0}", acknowledgementList.Cnt));
                var xDoc = XmlHelper.SerializeToXmlDocument(acknowledgementList, typeof(AcknowledgementList), "");
                var xmldecl = xDoc.CreateXmlDeclaration("1.0", null, null);
                xmldecl.Encoding = "utf-8";
                xmldecl.Standalone = "yes";
                var root = xDoc.DocumentElement;
                xDoc.InsertBefore(xmldecl, root);
                xDoc.PreserveWhitespace = true;
                                
                using (TextWriter writer = new StreamWriter(fileToZip, false))
                {
                    xDoc.Save(writer);
                    writer.Close();
                }

                MeFLog.LogDebug("SendAcksClient:  Creating zipped byteArray from AcknowledgementList");
                ZipHelper.CreateZipFile(this.zipFolderLocation, zipFilename, fileToZip);
                xmlData = StreamingHelper.ConvertToBytes(this.zipFolderLocation + zipFilename);                
                MeFLog.LogDebug("SendAcksClient:  Creating new AcknowledgementListAttachmentMTOM from byteArray");
            }
            return Invoke(serviceContext, xmlData);
        }

        /// <summary>
        ///   Calls SendAcks from MeF StateService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "acknowledgementZipFilePath">Zipped AcknowledgementList</param>
        /// <returns>Will return SendAcksResult, see <see cref = "SendAcksResult" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendAcksResult Invoke(ServiceContext serviceContext, string acknowledgementZipFilePath)
        {
            MeFLog.LogInfo("SendAcksClient.Invoke() with parameter AcknowledgementZipFilePath");
            Validate.IsValidFilePath(acknowledgementZipFilePath, "Invalid filePath", "SendAcksClient.Invoke");
            byte[] zipData = null;

            if (acknowledgementZipFilePath != null)
            {
                Audit.Write(string.Format("SendAcks AcknowledgementZipFilePath: {0}", acknowledgementZipFilePath));
                zipData = StreamingHelper.ConvertToBytes(acknowledgementZipFilePath);
                MeFLog.LogDebug("SendAcksClient:  Reading byte[] from local zipFile");
                MeFLog.LogDebug("SendAcksClient:  Creating new SubmissionReceiptListAttachmentMTOM from byteArray");
            }
            return Invoke(serviceContext, zipData);
        }

        #endregion

        #region Methods

        private SendAcksResult Invoke(ServiceContext serviceContext, byte[] data)
        {
            MeFLog.LogInfo("SendAcksClient.Invoke()");
            this._serviceContext = serviceContext;
            this.responseHandler = new ResponseHandler();
            Validate.NotNull(data, "Request Data cannot be null");
            RequestHelper.TestCallPrerequisites(serviceContext);            
            var proxy = new WCFSendAcksClient();
            var responseData = new SendAcksResponseType();
       
            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("SendAcks", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;
                
                MeFLog.LogDebug("SendAcksClient:  Sending Request");
                MessageBuilder.AuditRequest(mefheader);
                
                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var request = new SendAcksRequestType { AcknowledgementListAttMTOM = new base64Binary { Value = data } };
                    responseData = proxy.SendAcks(ref mefheader, request);
                    this.SetupCookies(serviceContext);
                }
              
                var result = new SendAcksResult();
                MeFLog.LogDebug("SendAcksClient:  Handling ResponseData");
              
                result.ErrorList = responseData.SubmissionErrorList;
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
                
                this.AuditResponseWithBody(mefheader, serviceContext, result);
                result.MessageID = mefheader.MessageID;
                result.RelatesTo = mefheader.RelatesTo;
                return result;
            }
            catch (TimeoutException timeProblem)
            {
                proxy.Abort();
                throw new ServiceException(Constants.SoapException, timeProblem);
            }
            catch (FaultException faultException)
            {
                MessageBuilder.AuditSoapFault(faultException);
                throw new ServiceException(Constants.SoapException, faultException);
            }
            catch (CommunicationException commProblem)
            {
                proxy.Abort();
                throw new ServiceException(Constants.SoapException, commProblem);
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
    }
}