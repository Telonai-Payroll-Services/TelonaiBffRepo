namespace MeF.Client.Services.TransmitterServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;   
    using MeF.Client.Validation;
    using System.Collections.Generic;
    using System.Linq;
    using MeFWCFClient.MeFTransmitterServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the GetAcksByMsgID service.
    /// </summary>
    public class GetAcksByMsgIDClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksByMsgIDClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetAcksByMsgIDClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetAcksByMsgIDClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksByMsgIDClient" /> class.
        /// </summary>
        /// <param name = "dir">The dir.</param>
        public GetAcksByMsgIDClient(DirectoryInfo dir)
        {
            MeFLog.LogInfo(string.Format("GetAcksByMsgIDClient.ctor with directory: {0}", dir.FullName));
            this.zipFolderLocation = dir.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksByMsgIDClient" /> class.
        /// </summary>
        
        public GetAcksByMsgIDClient()
        {
            MeFLog.LogInfo("GetAcksByMsgIDClient.ctor for In-Memory Processing");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "messageId">The message id.</param>
        /// <returns>A GetAcksByMsgIDResult, see <see cref = "GetAcksByMsgIDResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetAcksByMsgIDResult Invoke(ServiceContext serviceContext, string messageId)
        {
            MeFLog.LogInfo("GetAcksByMsgIDClient.Invoke()");            
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsMessageIDValid(messageId, "Invalid Message ID", "GetAcksByMsgIDClient");

            WCFGetAcksByMsgIDClient proxy = new WCFGetAcksByMsgIDClient();
            var responseData = new GetAcksByMsgIDResponseType();
            byte[] bytes;

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("GetAcksByMsgID", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;
                
                var reqAud = string.Format("MessageId: {0}", messageId);
                MessageBuilder.AuditRequest(mefHeader, reqAud);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetAcksByMsgIDRequestType { MessageId = messageId };
                    responseData = proxy.GetAcksByMsgID(ref mefHeader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AcknowledgementListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetAcksByMsgIDResult();
                result.MoreAvailableInd = responseData.MoreAvailableInd;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                            
                    MeFLog.LogDebug("GetAcksByMsgIDClient:  Handling ResponseData - In-memory");
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetAcksByMsgIDResult FileName: {0}", result.FileName));   
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(                            
                        mefHeader,
                        bytes,
                        responseData.AcknowledgementListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefHeader.MessageID);

                    MeFLog.LogDebug("GetAcksByMsgIDClient:  Handling ResponseData - File procesing");
                    this.outerZipFile = string.Format(this.zipFolderLocation + mefHeader.MessageID);                        
                    result.AttachmentFilePath = extractedFile;                        
                }

                this.AuditResponseWithBody(mefHeader, serviceContext, result);
                result.MessageID = mefHeader.MessageID;
                result.RelatesTo = mefHeader.RelatesTo;
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