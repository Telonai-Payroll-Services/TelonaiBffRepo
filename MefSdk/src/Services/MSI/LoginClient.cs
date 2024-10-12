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
    using System.ServiceModel.Channels;

    /// <summary>
    ///   This class is the service client for invoking the Login service.
    /// </summary>
    public class LoginClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "LoginClient" /> class.
        /// </summary>
        public LoginClient()
        {
            MeFLog.LogInfo("LoginClient.ctor");            
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;            
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Calls Login from MeFMSIService
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <returns>Will return LoginResult, see <see cref = "LoginResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public LoginResult Invoke(ServiceContext serviceContext)
        {
            SetupServicePointManager();
            this.SetServiceContext(serviceContext);           

            MeFLog.LogInfo(
                string.Format("LoginClient.Invoke() on thread ID: {0}", Thread.CurrentThread.ManagedThreadId));

            WCFLoginClient proxy = null;
            try
            {
                proxy = new WCFLoginClient();
            }
            catch (Exception e)
            {
                var mefHeader = RequestHelper.BuildRequestHeader("Login", serviceContext);
                MessageBuilder.AuditRequest(mefHeader);
                Audit.Write(e.ToString());
                MeFLog.Write(e.ToString());
                throw e;
            }

            LoginResponseType loginresptype = new LoginResponseType();
            string samlstring = "";

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("Login", serviceContext);                
                MessageBuilder.AuditRequest(mefHeader, null);

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    LoginRequestType loginreqtype = new LoginRequestType();               
                    loginresptype = proxy.Login(ref mefHeader, loginreqtype);

                    this.SetupCookies(serviceContext);

                    if (OperationContext.Current.IncomingMessageProperties.ContainsKey("SAMLAssertion"))
                    {
                        samlstring = (string)OperationContext.Current.IncomingMessageProperties["SAMLAssertion"];                        
                    }                    

                    serviceContext.SetSamlToken(samlstring);                    
                }

                var result = new LoginResult();
                if (loginresptype != null)
                {
                    result = new LoginResult(loginresptype);
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