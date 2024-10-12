namespace MeF.Client
{
    using System;
    using System.IO;
    using System.Net;
    using ICSharpCode.SharpZipLib.Zip;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;
    using MeFWCFClient.MeFMSIServices;
    using WCFClient;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    

    public abstract class MeFClientBase
    {
        #region Constants and Fields

        protected ServiceContext _serviceContext;
        protected string outerZipFile;
        protected ResponseHandler responseHandler = new ResponseHandler();
        protected string zipFolderLocation;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "MeFClientBase" /> class.
        /// </summary>
        protected MeFClientBase()
        {
            Audit.Instance();
            MeFLog.Instance();
            this._serviceContext = new ServiceContext();
        }

        

        #endregion

        #region Public Methods

        public static void SetupServicePointManager()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 120;
           // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
        }

        /// <summary>
        ///   This method can be used to create the Service Context from the<see cref = "ClientInfo" /> class..
        /// </summary>
        /// <param name = "clientInfo">Pass in a fully initialized ClientInfo</param>
        /// <returns>ServiceContext</returns>
        public ServiceContext CreateServiceContext(ClientInfo clientInfo)
        {
            this._serviceContext = new ServiceContext(clientInfo);

            return this._serviceContext;
        }

        /// <summary>
        ///   Gets the service context.
        /// </summary>
        /// <returns></returns>
        public ServiceContext GetServiceContext()
        {
            return this._serviceContext;
        }

        /// <summary>
        ///   Sets the service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        public void SetServiceContext(ServiceContext serviceContext)
        {
            this._serviceContext = serviceContext;
        }

        #endregion

        #region Methods

        internal void SetupProxy(ServiceContext context)
        {
            MeFLog.LogInfo(string.Format("Setting up proxy for the client"));
            Validate.NotNull(context, "ServiceContext");

            try
            {
                var prop = new HttpRequestMessageProperty();
                prop.Headers.Add(HttpRequestHeader.Cookie, context.GetSessionInfo().getCookieColl());    
                OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, prop);
                OperationContext.Current.OutgoingMessageProperties.Add("SAMLAssertion", context.GetSAMLToken());
            }            
            catch (Exception exception)
            {
                MessageBuilder.ProcessUnexpectedException(exception);
                throw new ToolkitException("MeFClientSDK000029", exception);
            }
        }

        internal byte[] getSoapAttachment()
        {
            return (byte[])OperationContext.Current.IncomingMessageProperties["SOAP_Attachment"];
        }

        internal void SetupCookies(ServiceContext context)
        {
            var response = (HttpResponseMessageProperty)OperationContext.Current.IncomingMessageProperties[HttpResponseMessageProperty.Name];
            var sharedCookie = response.Headers[HttpResponseHeader.SetCookie];

            if (sharedCookie != null && !sharedCookie.Trim().Equals(""))
            {
                MeFLog.LogInfo("Cookie from the response: " + sharedCookie);
                context.SetCookies(sharedCookie);
            }
            else
            {
                MeFLog.LogInfo("No Cookie was found in the response."); 
            }
            
        }

        protected virtual void AuditResponse(MeFHeaderType mefHeader, ServiceContext context)
        {
            if (mefHeader == null || context == null) return;
            MessageBuilder.AuditResponse(mefHeader,context);
        }


        protected virtual void AuditResponseWithBody(MeFHeaderType mefHeader, ServiceContext context, object bodyData)
        {
            MessageBuilder.AuditResponse(mefHeader, context);
            if (bodyData == null) return;
            Audit.Write(@"*** Response Data:");
            CustomObjectDumper.DumpObject(bodyData, bodyData.GetType().Name, true);
            Audit.Write(@"*** End Response Data ***");
        }

        protected virtual void AuditResponseWithBody(
            MeFHeaderType mefHeader, ServiceContext context, object bodyData, object bodyData2)
        {
            MessageBuilder.AuditResponse(mefHeader, context);
            if (bodyData == null) return;
            Audit.Write(@"*** Response Data:");
            CustomObjectDumper.DumpObject(bodyData, bodyData.GetType().Name, true);
            if (bodyData2 == null) return;
            CustomObjectDumper.DumpObject(bodyData2, bodyData2.GetType().Name, true);
            Audit.Write(@"*** End Response Data ***");
        }


        #endregion
    }
}