namespace MeF.Client.Services.TransmitterServices
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;
    using System.Collections.Generic;
    using MeFWCFClient.MeFTransmitterServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the GetSubmissionsStatus service.
    /// </summary>
    public class GetSubmissionsStatusClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsStatusClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetSubmissionsStatusClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionsStatusClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsStatusClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetSubmissionsStatusClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetSubmissionsStatusClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionsStatusClient" /> class.
        /// </summary>
        
        public GetSubmissionsStatusClient()
        {
            MeFLog.LogInfo("GetSubmissionsStatusClient.ctor for In-Memory");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionIdList">The list of submission ids.</param>
        /// <returns>A GetSubmissionsStatusResult, see <see cref = "GetSubmissionsStatusResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetSubmissionsStatusResult Invoke(ServiceContext serviceContext, string[] submissionIdList)
        {
            MeFLog.LogInfo("GetSubmissionsStatusClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDListValid(submissionIdList, "SubmissionId", "");

            var proxy = new WCFGetSubmissionsStatusClient();
            var responseData = new GetSubmissionsStatusResponseType();
            byte[] bytes;
           
            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetSubmissionsStatus", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetSubmissionsStatusRequestType { SubmissionIdList = submissionIdList };
                    MessageBuilder.AuditRequest(mefheader, null);
                    MessageBuilder.AuditRequestData(submissionIdList.ToList());

                    responseData = proxy.GetSubmissionsStatus(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.StatusRecordListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetSubmissionsStatusResult();
                
                result.ErrorList = responseData.SubmissionErrorList;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    
                    MeFLog.LogDebug("GetSubmissionsStatusClient:  Handling ResponseData - In-memory");

                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetSubmissionsStatusResult FileName: {0}", result.FileName));
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.StatusRecordListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("GetSubmissionsStatusClient:  Handling ResponseData - File procesing");
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