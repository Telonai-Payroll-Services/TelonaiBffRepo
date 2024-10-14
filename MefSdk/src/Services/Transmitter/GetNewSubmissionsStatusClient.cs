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
    ///   This class is the service client for invoking the GetNewSubmissionsStatus service.
    /// </summary>
    public class GetNewSubmissionsStatusClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewSubmissionsStatusClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetNewSubmissionsStatusClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetNewSubmissionsStatusClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewSubmissionsStatusClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetNewSubmissionsStatusClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetNewSubmissionsStatusClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewSubmissionsStatusClient" /> class.
        /// </summary>
        
        public GetNewSubmissionsStatusClient()
        {
            MeFLog.LogInfo("GetNewSubmissionsStatusClient.ctor for In-Memory");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns>A GetNewSubmissionsStatusResult, see <see cref = "GetNewSubmissionsStatusResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetNewSubmissionsStatusResult Invoke(ServiceContext serviceContext, string maxResults)
        {
            MeFLog.LogInfo("GetNewSubmissionsStatusClient.Invoke()");
            Validate.IsValidMaxResults(maxResults, "MaxResults", "GetNewSubmissionsStatusClient");
            RequestHelper.TestCallPrerequisites(serviceContext);

            var proxy = new WCFGetNewSubmissionsStatusClient();
            var responseData = new GetNewSubmissionsStatusResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetNewSubmissionsStatus", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetNewSubmissionsStatusRequestType { MaxResultCnt = maxResults };
                    MessageBuilder.AuditRequest(mefheader, maxResults);
                    responseData = proxy.GetNewSubmissionsStatus(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.StatusRecordListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetNewSubmissionsStatusResult();

                result.MoreAvailableInd = responseData.MoreAvailableInd;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    MeFLog.LogDebug("GetNewSubmissionsStatusClient:  Handling ResponseData - In-memory");

                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetNewSubmissionsStatusResult FileName: {0}", result.FileName));
                }
                else
                {

                    var extractedFile = this.responseHandler.HandleResponse(                            
                        mefheader,
                        bytes,
                        responseData.StatusRecordListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("GetNewSubmissionsStatusClient:  Handling ResponseData - File processing");
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