namespace MeF.Client.Services.TransmitterServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;   
    using System.Xml.Linq;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;    
    using System.Linq;
    using MeFWCFClient.MeFTransmitterServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Xml;
    using MeFWCFClient.Services.CustomTextMessageEncoderMTOM;

    /// <summary>
    ///   This class is the service client for invoking the SendSubmissions service.
    /// </summary>
    public class SendSubmissionsClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SendSubmissionsClient" /> class.
        /// </summary>
        /// <param name = "folderName">Name of the folder.</param>
        public SendSubmissionsClient(string folderName)
        {
            MeFLog.LogInfo(string.Format("SendSubmissionsClient.ctor with folder: {0}", folderName));
            this.zipFolderLocation = folderName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SendSubmissionsClient" /> class.
        /// </summary>
        /// <param name = "dir">The dir.</param>
        public SendSubmissionsClient(DirectoryInfo dir)
        {
            MeFLog.LogInfo(string.Format("SendSubmissionsClient.ctor with directory: {0}", dir.FullName));
            this.zipFolderLocation = dir.FullName;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SendSubmissionsClient" /> class.
        /// </summary>
        
        public SendSubmissionsClient()
        {
            MeFLog.LogInfo("SendSubmissionsClient.ctor for In-Memory Processing");
            this.zipFolderLocation = "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Calls SendSubmissions
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "submissionContainer">SubmissionsAttachment</param>
        /// <returns>Will return SendSubmissionsResult, see <see cref = "SendSubmissionsResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SendSubmissionsResult Invoke(ServiceContext serviceContext, SubmissionContainer submissionContainer)
        {
            MeFLog.LogInfo("SendSubmissionsClient.Invoke()");
            this._serviceContext = serviceContext;
            this.responseHandler = new ResponseHandler();
            var result = new SendSubmissionsResult();
            RequestHelper.TestCallPrerequisites(serviceContext);

            MeFLog.LogDebug("SendSubmissionsClient:  Creating RequestData from SubmissionContainer");
            var requestData = CreateRequest(submissionContainer);
            Validate.IsSubmissionDataListValid(requestData.SubmissionDataList, "Invalid Submission ID", "SendSubmissionsClient");

            var proxy = new WCFSendSubmissionsClient();
            var responseData = new SendSubmissionsResponseType();
            byte[] bytes;

            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("SendSubmissions", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                var trace = new StringBuilder();
                trace.AppendLine("Transmission Submissions Data");
                trace.AppendLine(string.Format("Count: {0}", requestData.SubmissionDataList.Length.ToString()));
                foreach (var s in requestData.SubmissionDataList)
                {
                    trace.AppendLine(string.Format("SubmissionId: {0}", s.SubmissionId));
                }
                var reqAud = trace.ToString();
                MessageBuilder.AuditRequest(mefheader, reqAud);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    requestData.SubmissionsAttMTOM = submissionContainer.GetBytes() ;

                    //copy the binary data
                        
                    bool mtomEncode=copyBytes(requestData);
                    byte [] dummyArray=new byte [1024];
                    if (mtomEncode)
                    {
                        //set the value to be a dummy array to save time in base64 encoding / mtom encoding
                        requestData.SubmissionsAttMTOM = dummyArray;
                    }

                    responseData = proxy.SendSubmissions(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                    bytes = responseData.SubmissionRcptListAttMTOM.Value;
                    if (bytes == null)
                    {
                        bytes = this.getSoapAttachment();
                    }
                }                   
                
                result.MessageID = mefheader.MessageID;
                result.RelatesTo = mefheader.RelatesTo;
                result.DepositId = responseData.DepositId;

                if (this.zipFolderLocation == null || this.zipFolderLocation.Trim() == "")
                {
                    Dictionary<string, byte[]> zipcontent = this.responseHandler.getmapofContents(bytes);
                    MeFLog.LogDebug("SendSubmissionsClient:  Handling ResponseData - In-memory");

                    var filename = zipcontent.Keys.First();
                    result.FileName = Path.GetFileName(filename);
                    result.unzippedcontent = zipcontent[filename];
                    MeFLog.LogDebug(string.Format("SendSubmissionsResult FileName: {0}", result.FileName));
                }
                else
                {
                    var extractedFile = this.responseHandler.HandleResponse(
                        mefheader,
                        bytes,
                        responseData.SubmissionRcptListAttMTOM.contentType,
                        this.zipFolderLocation,
                        mefheader.MessageID);

                    MeFLog.LogDebug("SendSubmissionsClient:  Handling ResponseData - File procesing");
                    this.outerZipFile = string.Format(this.zipFolderLocation + mefheader.MessageID);
                    result.AttachmentFilePath = extractedFile;                                              
                }

                this.AuditResponseWithBody(mefheader, serviceContext, result);
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

        private static SendSubmissionsRequestType CreateRequest(SubmissionContainer iCont)
        {
            MeFLog.LogDebug("Creating Request from SubmissionContainer");
            var requestData = new SendSubmissionsRequestType();
            try
            {
                var sData = new List<SubmissionDataType>();

                if (iCont.PostMarkedSubmissionArchives != null)
                {
                    foreach (var arch in iCont.PostMarkedSubmissionArchives)
                    {
                        var data = new SubmissionDataType
                            {
                                SubmissionId = arch.PostMarkedArchive.submissionId,
                                ElectronicPostmarkTsSpecified = true,
                                ElectronicPostmarkTs = arch.PostMark
                            };
                        MeFLog.LogDebug(
                            string.Format(
                                "SubmissionContainer.SubmissionData SubmissionId: {0}  ElectronicPostmark:  {1}",
                                arch.PostMarkedArchive.submissionId,
                                arch.PostMark.ToString()));
                        sData.Add(data);
                    }

                    
                }
                if (iCont.SubmissionArchives != null)
                {
                    

                    foreach (var arch in iCont.SubmissionArchives)
                    {
                        var data = new SubmissionDataType { SubmissionId = arch.submissionId, ElectronicPostmarkTsSpecified = false };

                        sData.Add(data);
                        MeFLog.LogDebug(
                            string.Format("SubmissionContainer.SubmissionData SubmissionId: {0}", arch.submissionId));
                    }
                    
                }
                requestData.SubmissionDataList = sData.ToArray();
            }
            catch (Exception ex)
            {
                MeFLog.LogError(
                    "MeFClientSDK000029",
                    "Error while creating submission data for SendSubmissions. Check the log file for more details.",
                    ex);
                throw new ToolkitException(
                    "MeFClientSDK000029", "Unexpected Error: Check the log file for more details.", ex);
            }
            return requestData;
        }

        //for mtom encoding
        private const int mtomSwitchLength = 204800; //200KB

        //copy the bytes of request and set content type
        private static bool copyBytes(SendSubmissionsRequestType reuqest)
        {
            bool mtomEncode = false;
            if (reuqest.SubmissionsAttMTOM.Length > mtomSwitchLength)
            {
                mtomEncode = true;

                //reset the content type
                Random rnd = new Random();
                string boundary = "uuid:" + Guid.NewGuid().ToString();
                string startUri = "http://tempuri.org/0/" + rnd.Next();
                string startInfo = "text/xml";
                string contentType = "multipart/related;type=\"application/xop+xml\";start=\"<" + startUri +
                    ">\";boundary=\"" + boundary +
                    "\";start-info=\"" + startInfo + "\"";

                CustomTextMessageEncoder.setParameters(boundary, startUri, contentType, mtomEncode, reuqest.SubmissionsAttMTOM);
            }

            return mtomEncode;
        }

        #endregion
    }
   
}