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
    ///   This class is the service client for invoking the GetNewAckNotifications service.
    /// </summary>
    public class GetNewAckNotificationsClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAckNotificationsClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetNewAckNotificationsClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetNewAckNotificationsClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAckNotificationsClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetNewAckNotificationsClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetNewAckNotificationsClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAckNotificationsClient" /> class.
        /// </summary>
        
        public GetNewAckNotificationsClient()
        {
            MeFLog.LogInfo("GetNewAckNotificationsClient.ctor for In-Memory Processing");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns>A GetNewAckNotificationsResult, see <see cref = "GetNewAckNotificationsResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetNewAckNotificationsResult Invoke(ServiceContext serviceContext, string maxResults)
        {
            MeFLog.LogInfo("GetNewAckNotificationsClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsValidMaxResults(maxResults, "maxResults", "");

            var proxy = new WCFGetNewAckNotificationsClient();
            var responseData = new GetNewAckNotificationsResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetNewAckNotifications", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;
                MessageBuilder.AuditRequest(mefheader, maxResults);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetNewAckNotificationsRequestType { MaxResultCnt = maxResults };
                    responseData = proxy.GetNewAckNotifications(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AckNotificationListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetNewAckNotificationsResult();

                result.MoreAvailableInd = responseData.MoreAvailableInd;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetNewAckNotificationsResult FileName: {0}", result.FileName));
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.AckNotificationListAttMTOM.contentType,
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