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
    ///   This class is the service client for invoking the GetNewAcks service.
    /// </summary>
    public class GetNewAcksClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAcksClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetNewAcksClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetNewAcksClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAcksClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetNewAcksClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetNewAcksClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNewAcksClient" /> class.
        /// </summary>
        
        public GetNewAcksClient()
        {
            MeFLog.LogInfo("GetNewAcksClient.ctor for In-Memory Processing");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns></returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetNewAcksResult Invoke(ServiceContext serviceContext, string maxResults)
        {
            return this.Invoke(serviceContext, null, null, maxResults);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "agencyType">Type of the agency.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns></returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetNewAcksResult Invoke(ServiceContext serviceContext, GovernmentAgencyTypeCdType agencyType, string maxResults)
        {
            return this.Invoke(serviceContext, agencyType, null, maxResults);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "categoryType">Type of the category.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns></returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetNewAcksResult Invoke(ServiceContext serviceContext, ExtndAcknowledgementCategoryCdType categoryType, string maxResults)
        {
            return this.Invoke(serviceContext, null, categoryType, maxResults);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "agency">The agency.</param>
        /// <param name = "category">The category.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns></returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetNewAcksResult Invoke(
            ServiceContext serviceContext, GovernmentAgencyTypeCdType? agency, ExtndAcknowledgementCategoryCdType? category, string maxResults)
        {
            MeFLog.LogInfo("GetNewAcksClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsValidMaxResults(maxResults, "MaxResults", "GetNewAcks");

            var proxy = new WCFGetNewAcksClient();            
            var responseData = new GetNewAcksResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetNewAcks", serviceContext);
                MessageBuilder.AuditRequest(mefheader, maxResults);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetNewAcksRequestType();

                    var trace = new StringBuilder();
                    trace.AppendLine(string.Format("MAxResults: {0}", maxResults));

                    if (agency.HasValue)
                    {
                        requestData.GovernmentAgencyTypeCdSpecified = true;
                        requestData.GovernmentAgencyTypeCd = agency.Value;
                        trace.AppendLine(string.Format("Agency: {0}", agency.Value.ToString()));
                    }
                    else
                    {
                        requestData.GovernmentAgencyTypeCdSpecified = false;
                    }

                    if (category.HasValue)
                    {
                        requestData.ExtndAcknowledgementCategoryCdSpecified = true;
                        requestData.ExtndAcknowledgementCategoryCd = category.Value;
                        trace.AppendLine(string.Format("Category: {0}", category.Value.ToString()));
                    }
                    else
                    {
                        requestData.ExtndAcknowledgementCategoryCdSpecified = false;
                    }

                    var reqAud = trace.ToString();
                    requestData.MaxResultCnt = maxResults;

                    responseData = proxy.GetNewAcks(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.AcknowledgementListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetNewAcksResult();

                result.MoreAvailableInd = responseData.MoreAvailableInd;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                        
                    MeFLog.LogDebug("GetNewAcksClient:  Handling ResponseData - In-memory");
                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("GetNewAcksResult FileName: {0}", result.FileName));
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.AcknowledgementListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);
                        
                    MeFLog.LogDebug("GetNewAcksClient:  Handling ResponseData - File processing");
                    this.outerZipFile = this.zipFolderLocation + mefheader.MessageID;
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