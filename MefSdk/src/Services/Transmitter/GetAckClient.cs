namespace MeF.Client.Services.TransmitterServices
{
    using System;
    using System.IO;
    using System.Net;    
    using System.Runtime.CompilerServices;
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
    ///   This class is the service client for invoking the GetAck service.
    /// </summary>
    public class GetAckClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetAckClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetAckClient.ctor with folder: {0}", folderName));

            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetAckClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetAckClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckClient" /> class.
        /// </summary>        
        public GetAckClient()
        {
            MeFLog.LogInfo(string.Format("GetAckClient.ctor for In-Memory Processing"));
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionId">The submission id.</param>
        /// <returns></returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetAckResult Invoke(ServiceContext serviceContext, string submissionId)
        {
            MeFLog.LogInfo("GetAckClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDValid(submissionId, "SubmissionId", "GetAck");

            var proxy = new WCFGetAckClient();
            var responseData = new GetAckResponseType();
            byte[] bytes;
            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetAck", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                MessageBuilder.AuditRequest(mefheader, submissionId);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetAckRequestType { SubmissionId = submissionId };
                    responseData = proxy.GetAck(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AcknowledgementAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetAckResult();

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {

                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    MeFLog.LogDebug("GetAckClient:  Handling ResponseData - In-memeory");                       
                        
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetAckResult FileName: {0}", result.FileName));                           
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(                            
                        mefheader,
                        bytes,
                        responseData.AcknowledgementAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("GetAckClient:  Handling ResponseData - File processing");
                    this.outerZipFile = string.Format(this.zipFolderLocation + mefheader.MessageID);
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