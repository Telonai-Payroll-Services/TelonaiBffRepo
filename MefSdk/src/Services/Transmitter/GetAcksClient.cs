namespace MeF.Client.Services.TransmitterServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;    
    using MeFWCFClient.MeFTransmitterServices;
    using MeF.Client.Validation;
    using System.ServiceModel;
        
    public class GetAcksClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetAcksClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetAcksClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetAcksClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetAcksClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetAcksClient" /> class.
        /// </summary>
        
        public GetAcksClient()
        {
            MeFLog.LogInfo("GetAcksClient.ctor for In-Memory Processing");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "submissionIdList">The list of submission ids.</param>
        /// <returns>A GetAcksResult, see <see cref = "GetAcksResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetAcksResult Invoke(ServiceContext serviceContext, string[] submissionIdList)
        {
            MeFLog.LogInfo("GetAcksClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsSubmissionIDListValid(submissionIdList, "SubmissionId", "GetAcks");
            var proxy = new WCFGetAcksClient();
            var responseData = new GetAcksResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetAcks", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                var trace = new List<string> { "GetAcks SubmissionIdList", string.Format("Count: {0}", submissionIdList.Length) };
                trace.AddRange(submissionIdList.Select(subid => string.Format("SubmissionId: {0}", subid)));

                MessageBuilder.AuditRequest(mefheader);
                MessageBuilder.AuditRequestData(trace);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetAcksRequestType { SubmissionIdList = submissionIdList };                
                    responseData = proxy.GetAcks(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AcknowledgementListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetAcksResult();
                result.ErrorList = responseData.SubmissionErrorList;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                            
                    MeFLog.LogDebug("GetAcksClient:  Handling ResponseData - In-memory");

                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetAcksResult FileName: {0}", result.FileName));
                }
                else
                {

                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.AcknowledgementListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("GetAcksClient:  Handling ResponseData - File processing");
                    this.outerZipFile = string.Format(this.zipFolderLocation + mefheader);                        
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