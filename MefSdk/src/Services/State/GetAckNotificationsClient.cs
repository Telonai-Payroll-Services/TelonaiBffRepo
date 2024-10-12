namespace MeF.Client.Services.StateServices
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
    using MeFWCFClient.MeFStateServices;
    using System.ServiceModel;
        
    /// <summary>
    ///   This class is the service client for invoking the GetAckNotifications service.
    /// </summary>
    public class GetAckNotificationsClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationsClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetAckNotificationsClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationsClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationsClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetAckNotificationsClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationsClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAckNotificationsClient" /> class.
        /// </summary>
        
        public GetAckNotificationsClient()
        {
            MeFLog.LogInfo(string.Format("GetAckNotificationsClient.ctor for In-Memory Processing"));
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionIdList">The list of submission ids.</param>
        /// <returns>A GetAckNotificationsResult, see <see cref = "GetAckNotificationsResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetAckNotificationsResult Invoke(ServiceContext serviceContext, string[] submissionIdList)
        {
            MeFLog.LogInfo("GetAckNotificationsClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDListValid(submissionIdList, "SubmissionIdList", "");

            var proxy = new WCFGetAckNotificationsClient();
            var responseData = new GetAckNotificationsResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetAckNotifications", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                MessageBuilder.AuditRequest(mefheader, null);
                MessageBuilder.AuditRequestData(submissionIdList.ToList());

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetAckNotificationsRequestType { SubmissionIdList = submissionIdList };
                    responseData = proxy.GetAckNotifications(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AckNotificationListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetAckNotificationsResult();
                                
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
                

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetAckNotificationsResult FileName: {0}", result.FileName));
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