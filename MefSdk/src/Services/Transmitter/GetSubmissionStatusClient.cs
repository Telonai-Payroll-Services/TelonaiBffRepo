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
    ///   This class is the service client for invoking the GetSubmissionStatus service.
    /// </summary>
    public class GetSubmissionStatusClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionStatusClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetSubmissionStatusClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionStatusClient.ctor with folder: {0}", folderName));

            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionStatusClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetSubmissionStatusClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionStatusClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionStatusClient" /> class.
        /// </summary>

        public GetSubmissionStatusClient()
        {
            MeFLog.LogInfo("GetSubmissionStatusClient.ctor for In-Memory");
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
        public GetSubmissionStatusResult Invoke(ServiceContext serviceContext, string submissionId)
        {
            MeFLog.LogInfo("GetSubmissionStatusClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDValid(submissionId, "SubmissionId", "GetSubmissionStatus");

            var proxy = new WCFGetSubmissionStatusClient();
            var responseData = new GetSubmissionStatusResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetSubmissionStatus", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetSubmissionStatusRequestType { SubmissionId = submissionId };
                    MessageBuilder.AuditRequest(mefheader, submissionId);
                    responseData = proxy.GetSubmissionStatus(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.StatusRecordListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetSubmissionStatusResult();

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    MeFLog.LogDebug("GetSubmissionStatusClient:  Handling ResponseData - In-memory");

                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetSubmissionStatusResult FileName: {0}", result.FileName));
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.StatusRecordListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("GetSubmissionStatusClient:  Handling ResponseData - File procesing");
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