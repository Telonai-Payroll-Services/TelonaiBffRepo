namespace MeF.Client.Services.MSIServices
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeFWCFClient.MeFMSIServices;
    using System.ServiceModel;   

    /// <summary>
    ///   This class is the service client for invoking the Logout service.
    /// </summary>
    public class LogoutClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "LogoutClient" /> class.
        /// </summary>
        public LogoutClient()
        {
            MeFLog.LogInfo("LogoutClient.ctor");
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <returns>
        ///   LogoutResult, see <see cref = "LogoutResult" />.
        /// </returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public LogoutResult Invoke(ServiceContext serviceContext)
        {
            MeFLog.LogInfo("LogoutClient: validating parameters and creating proxy.");
            RequestHelper.TestCallPrerequisites(serviceContext);
            WCFLogoutClient proxy = new WCFLogoutClient();
            LogoutResponseType logoutresptype = new LogoutResponseType();

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("Logout", serviceContext);
                MessageBuilder.AuditRequest(mefHeader, null);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;                

                using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    LogoutRequestType logoutreqtype = new LogoutRequestType();
                    logoutresptype = proxy.Logout(ref mefHeader, logoutreqtype);
                }

                var result = new LogoutResult();
                if (logoutresptype != null)
                {
                    result = new LogoutResult(logoutresptype);
                    this.AuditResponseWithBody(mefHeader, serviceContext, result);
                }

                result.MessageID = mefHeader.MessageID;
                result.RelatesTo = mefHeader.RelatesTo;
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