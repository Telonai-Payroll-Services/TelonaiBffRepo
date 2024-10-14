namespace MeF.Client.Helpers
{
    using System;
    using System.IO;
    using System.Net;
  //  using System.Web.Services.Protocols;

    using MeF.Client.Configuration;
    using MeF.Client.Exceptions;
    using MeF.Client.Logging;
    using MeFWCFClient.MeFMSIServices;
    using WCFClient;
    using System.Xml;

    internal static class RequestHelper
    {
        #region Public Methods

        


        /// <summary>
        /// Tests the login call prerequisites.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="serviceContext">The service context.</param>
        /// <returns></returns>
        public static bool TestLoginCallPrerequisites(object sender, ServiceContext serviceContext)
        {
            MeFLog.LogInfo("RequestHelper:TestLoginCallPrerequisites()");

            if (serviceContext == null)
            {
                MeFLog.Write(
                    "LoginClient: ServiceContext not initialized properly.", LoggingCategory.Error, LoggingPriority.Low);

                return false;
            }
            var clientConfig = serviceContext.GetClientInfo();

            if (clientConfig.AppSysID.Length == 0 || clientConfig.Etin.Length == 0)
            {
                MeFLog.Write(
                    "LoginClient: ClientInfo's AppSysID, Etin, and/or CertificateSubject have not been initialized Properly.",
                    LoggingCategory.Error,
                    LoggingPriority.Low);

                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tests the call prerequisites.
        /// </summary>
        /// <param name="serviceContext">The service context.</param>
        /// <exception cref="MeF.Client.Exceptions.ToolkitException">MeFClientSDK000038</exception>
        public static void TestCallPrerequisites(ServiceContext serviceContext)
        {
            MeFLog.LogInfo("RequestHelper:TestCallPrerequisites()");
            if (serviceContext == null || !serviceContext.IsValidSession())
            {

                throw new ToolkitException("MeFClientSDK000038", ApiHelper.GetErrorInfo("MeFClientSDK000038"), new ArgumentNullException());
            }
        }

        internal static MeFHeaderType BuildRequestHeader(string action, ServiceContext serviceContext)
        {
            MeFLog.LogInfo(string.Format("Building MeFHeader for '{0}'.", action));
            var clientInfo = serviceContext.GetClientInfo();
            var messageId = clientInfo.Etin + DateTime.Now.ToString("yyyy") + DateTime.Now.DayOfYear.ToString("000")
                            + Path.GetRandomFileName().Substring(0, 8);
            XmlDocument doc = new XmlDocument();
            XmlAttribute idAttr = doc.CreateAttribute("Id");
            idAttr.Value = "MefHeader";
            var mefHeader = new MeFHeaderType
                {
                    Action = action,
                    ETIN = clientInfo.Etin,
                    AppSysID = clientInfo.AppSysID,
                    AnyAttr = new XmlAttribute[] {idAttr},
                    MessageID = messageId
                };
            if (action == "Logout")
            {
                mefHeader.SessionKeyCdSpecified = true;
                mefHeader.SessionKeyCd = SessionKeyCdType.N;
            }
            else
            {
                mefHeader.SessionKeyCdSpecified = true;
                mefHeader.SessionKeyCd = SessionKeyCdType.Y;
            }

            mefHeader.TestCdSpecified = true;
            mefHeader.TestCd = clientInfo.TestIndicator;
            mefHeader.MessageTs = DateTime.UtcNow.ToUniversalTime();
            mefHeader.WSDLVersionNum = "10.8";
            mefHeader.ClientSoftwareTxt = "MeFA2ADNetToolkit" + MeF.Client.Logging.Constants .VersionNumber ;
            return mefHeader;
        }       

        //internal static void SetupServicePointManager()
        //{
        //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //    ServicePointManager.Expect100Continue = false;
        //    ServicePointManager.DefaultConnectionLimit = 120;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
        //}

        #endregion
    }
}