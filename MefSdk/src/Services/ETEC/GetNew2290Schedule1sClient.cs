namespace MeF.Client.Services.ETECTransmitterServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using ICSharpCode.SharpZipLib.Zip;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;
    using System.Collections.Generic;
    using MeFWCFClient.ETECTransmitterServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the GetNew2290Schedule1s service.
    /// </summary>
    public class GetNew2290Schedule1sClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNew2290Schedule1sClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public GetNew2290Schedule1sClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("GetNew2290Schedule1sClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNew2290Schedule1sClient" /> class.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        public GetNew2290Schedule1sClient(DirectoryInfo directory)
        {
            MeFLog.LogInfo(string.Format("GetNew2290Schedule1sClient.ctor with directory: {0}", directory.FullName));
            this.zipFolderLocation = directory.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetNew2290Schedule1sClient" /> class in-memory processing.
        /// </summary>        
        public GetNew2290Schedule1sClient()
        {
            MeFLog.LogInfo(string.Format("GetNew2290Schedule1sClient.ctor for in-memory processing"));
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns>A GetNew2290Schedule1sResult, see <see cref = "GetNew2290Schedule1sResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetNew2290Schedule1sResult Invoke(ServiceContext serviceContext, string maxResults)
        {
            MeFLog.LogInfo("GetNew2290Schedule1sClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsValidMaxResults(maxResults, "MaxResults", "");

            var proxy = new WCFGetNew2290Schedule1sClient();
            var responseData = new GetNew2290Schedule1sResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetNew2290Schedule1s", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetNew2290Schedule1sRequestType { MaxResultCnt = maxResults };
                    MessageBuilder.AuditRequest(mefheader, maxResults);
                    responseData = proxy.GetNew2290Schedule1s(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.Schedule1AttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }

                var result = new GetNew2290Schedule1sResult();
                if (responseData != null)
                {
                    result.MoreAvailableInd = responseData.MoreAvailableInd;
                    result.Cnt = Convert.ToInt32(responseData.Cnt);                    

                    if (this.zipFolderLocation == null || this.zipFolderLocation.Trim().Equals(""))
                    {
                        Dictionary<string, byte[]> filemap = this.responseHandler.getmapofContents(bytes);

                        MeFLog.LogDebug("GetNew2290Schedule1sClient:  Handling ResponseData - In-memory");
                        this.GetSchedule1Files(result, filemap);
                                         
                    }
                    else
                    {                        
                        var extractedFile = this.responseHandler.HandleResponseSaveOnly(                            
                            mefheader,
                            bytes,
                            responseData.Schedule1AttMTOM.contentType,
                            this.zipFolderLocation,
                            mefheader.MessageID);

                        MeFLog.LogDebug("GetNew2290Schedule1sClient:  Handling ResponseData - File processing");
                        this.outerZipFile = mefheader.MessageID;
                        this.ExplodeZip(this.outerZipFile);
                        this.GetSchedule1Files(result);                        
                        result.AttachmentFilePath = extractedFile;                        
                    }
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

        #region Methods

        private void GetSchedule1Files(GetNew2290Schedule1sResult result)
        {
            DirectoryInfo dirInfoExtract = new DirectoryInfo(zipFolderLocation + outerZipFile);
            FileInfo[] pdfFile;
            pdfFile = dirInfoExtract.GetFiles("*.*");
            foreach (FileInfo pdfFiles in pdfFile)
            {
                FileItem pdfItem = new FileItem();
                pdfItem.Filename = pdfFiles.FullName;
                result.schedule1Attachments.Add(pdfItem);
                MeFLog.LogDebug(string.Format("GetNew2290Schedule1sResult.schedule1Attachments FileName: {0}", pdfItem.Filename));
            }
        }

        private void GetSchedule1Files(GetNew2290Schedule1sResult result, Dictionary<string, byte[]> filemap)
        {
            foreach (var entry in filemap)
            {
                FileItem pdfItem = new FileItem(filemap[entry.Key]);
                pdfItem.Filename = Path.GetFileName(entry.Key);
                result.schedule1Attachments.Add(pdfItem);
                MeFLog.LogDebug(string.Format("GetNew2290Schedule1sResult.schedule1Attachments FileName: {0}", pdfItem.Filename));
            }
        }

        private void ExplodeZip(string outerZipFile)
        {
            FastZip oFastZip = new FastZip();

            string outerZip = zipFolderLocation + @"\" + outerZipFile + ".zip";
            string explodedZipFolder = zipFolderLocation + @"\" + outerZipFile;
            oFastZip.ExtractZip(outerZip, explodedZipFolder, "");
        }

        #endregion
    }
}