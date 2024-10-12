namespace MeF.Client.Services.StateServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
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
    ///   This class is the service client for invoking the SendSubmissionReceipts service.
    /// </summary>
    public class SendSubmissionReceiptsClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Constructor specifying location where response attachment will be maintained and extracted
        /// </summary>
        /// <param name = "folderName">Folder path - ensure the current user has the correct permission</param>
        public SendSubmissionReceiptsClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("SendSubmissionReceiptsClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Constructor accepting a Folder Object. This folder will be used to store/maintain attachments
        /// </summary>
        /// <param name = "dir">DirectoryInfo object</param>
        public SendSubmissionReceiptsClient(DirectoryInfo dir)
        {
            MeFLog.LogInfo(string.Format("SendSubmissionReceiptsClient.ctor with directory: {0}", dir.FullName));
            this.zipFolderLocation = dir.FullName;
        }

        /// <summary>
        ///   Constructor accepting no parameter for In-memory Processing
        /// </summary>        
        //public SendSubmissionReceiptsClient()
        //{
        //    MeFLog.LogInfo("SendSubmissionReceiptsClient.ctor with no parameter for In-memory processing");
        //    this.zipFolderLocation = "";
        //}

        #endregion

        private string auditdata = "";

        #region Public Methods

        /// <summary>
        ///   Calls SendSubmissionReceipts from MeF StateService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "submissionReceiptList">SubmissionReceiptList</param>
        /// <returns>Will return SendSubmissionReceiptsResult, see <see cref = "SendSubmissionReceiptsResult" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendSubmissionReceiptsResult Invoke(
            ServiceContext serviceContext, SubmissionReceiptList submissionReceiptList)
        {
            MeFLog.LogInfo("SendSubmissionReceiptsClient.Invoke() with SubmissionReceiptList");

            var requestData = new SendSubmissionReceiptsRequestType();

            if (submissionReceiptList != null)
            {
                var xmlFileName = Path.GetRandomFileName();
                xmlFileName = xmlFileName.Remove(xmlFileName.LastIndexOf("."), 4);
                var fileToZip = this.zipFolderLocation + xmlFileName + ".xml";
                var zipFilename = xmlFileName + ".zip";
                MeFLog.LogDebug("SendSubmissionReceiptsClient:  Serializing SubmissionReceiptList");

                var xDoc = XmlHelper.SerializeToXmlDocument(submissionReceiptList, typeof(SubmissionReceiptList), "");
                auditdata = GetReqAud(xDoc);
                MeFLog.LogDebug("SendSubmissionReceiptsClient:  Creating zipped byteArray from SubmissionReceiptList");

                using (TextWriter writer = new StreamWriter(fileToZip, false))
                {
                    xDoc.Save(writer);
                    writer.Close();
                }
                ZipHelper.CreateZipFile(this.zipFolderLocation, zipFilename, fileToZip);
                var xmlData = StreamingHelper.ConvertToBytes(this.zipFolderLocation + zipFilename);
                MeFLog.LogDebug(
                    "SendSubmissionReceiptsClient:  Creating new SubmissionReceiptListAttachmentMTOM from byteArray");
                requestData.SubmissionRcptListAttMTOM = new base64Binary { Value = xmlData };
            }
            return this.Invoke(serviceContext, requestData);
        }

        /// <summary>
        ///   Calls SendSubmissionReceipts from MeF StateService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "receiptsZipFilePath">receiptsZipFilePath</param>
        /// <returns>Will return SendSubmissionReceiptsResult, see <see cref = "SendSubmissionReceiptsResult" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendSubmissionReceiptsResult Invoke(ServiceContext serviceContext, string receiptsZipFilePath)
        {
            MeFLog.LogInfo("SendSubmissionReceiptsClient.Invoke() with receiptsZipFilePath");
            Validate.IsValidFilePath(receiptsZipFilePath, "Invalid filePath", "SendSubmissionReceiptsClient.Invoke");
            var requestData = new SendSubmissionReceiptsRequestType();
            if (receiptsZipFilePath != null)
            {
                auditdata = string.Format("SendSubmissionReceipts ReceiptsZipFilePath: {0}", receiptsZipFilePath);
                MeFLog.LogDebug("SendSubmissionReceiptsClient:  Reading byte[] from local zipFile");
                requestData.SubmissionRcptListAttMTOM = new base64Binary
                    { Value = StreamingHelper.Chunk(receiptsZipFilePath) };
                MeFLog.LogDebug(
                    "SendSubmissionReceiptsClient:  Creating new SubmissionReceiptListAttachmentMTOM from byteArray");
            }

            return this.Invoke(serviceContext, requestData);
        }

        /// <summary>
        ///   Calls SendSubmissionReceipts from MeF StateService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "receiptsBytearray">receiptsBytearray</param>
        /// <returns>Will return SendSubmissionReceiptsResult, see <see cref = "SendSubmissionReceiptsResult" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendSubmissionReceiptsResult Invoke(ServiceContext serviceContext, byte[] receiptsBytearray)
        {
            MeFLog.LogInfo("SendSubmissionReceiptsClient.Invoke() with receipts as byte[]");
            
            var requestData = new SendSubmissionReceiptsRequestType();
            if (receiptsBytearray != null)
            {   
                requestData.SubmissionRcptListAttMTOM = new base64Binary { Value = receiptsBytearray };
                MeFLog.LogDebug(
                    "SendSubmissionReceiptsClient:  Creating new SubmissionReceiptListAttachmentMTOM from byteArray");
            }

            return this.Invoke(serviceContext, requestData);
        }

        #endregion

        #region Methods

        private static string GetReqAud(XmlDocument auditDoc)
        {
            var trace = new StringBuilder();
            trace.AppendLine("Receipt data");
            var writer = XmlWriter.Create(trace);

            auditDoc.WriteContentTo(writer);
            writer.Flush();
            var reqAud = trace.ToString();
            return reqAud;
        }

        private SendSubmissionReceiptsResult Invoke(
            ServiceContext serviceContext, SendSubmissionReceiptsRequestType request)
        {
            MeFLog.LogInfo("SendSubmissionReceiptsClient.Invoke()");
            this._serviceContext = serviceContext;
            Validate.NotNull(request, "Request Data cannot be null");
            RequestHelper.TestCallPrerequisites(serviceContext);
            
            var proxy = new WCFSendSubmissionReceiptsClient();
            var responseData = new SendSubmissionReceiptsResponseType();
            
            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("SendSubmissionReceipts", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;           
                                
                MeFLog.LogDebug("SendSubmissionReceiptsClient:  Sending Request");
                MessageBuilder.AuditRequest(mefheader,auditdata);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    responseData = proxy.SendSubmissionReceipts(ref mefheader, request);
                    this.SetupCookies(serviceContext);
                }
                
                var result = new SendSubmissionReceiptsResult();
                MeFLog.LogDebug("SendSubmissionReceiptsClient:  Handling ResponseData");
                result.Cnt = responseData.Cnt;
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