namespace MeF.Client.Services.StateServices
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
    using MeFWCFClient.MeFStateServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the GetAckNotification service.
    /// </summary>
    public class GetAckNotificationClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetAckNotificationClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationClient.ctor with folder: {0}", folderName));

            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetAckNotificationClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationClient" /> class.
        /// </summary>        
        public GetAckNotificationClient()
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationClient.ctor for In-Memory Processing"));
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
        public GetAckNotificationResult Invoke(ServiceContext serviceContext, string submissionId)
        {
            MeFLog.LogInfo("GetAckNotificationClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDValid(submissionId, "SubmissionId", "");

            var proxy = new WCFGetAckNotificationClient();
            var responseData = new GetAckNotificationResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetAckNotification", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                MessageBuilder.AuditRequest(mefheader, submissionId);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetAckNotificationRequestType { SubmissionId = submissionId };
                    responseData = proxy.GetAckNotification(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AckNotificationAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetAckNotificationResult();

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetAckNotificationResult FileName: {0}", result.FileName));                        
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(                            
                        mefheader,
                        bytes,
                        responseData.AckNotificationAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

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