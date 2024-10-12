namespace MeF.Client.Services.MSIServices
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeF.Client.Validation;
    using MeFWCFClient.MeFMSIServices;
    using System.ServiceModel;    

    /// <summary>
    ///   This class is the service client for invoking the EtinRetrieval service.
    /// </summary>
    public class EtinRetrievalClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EtinRetrievalClient" /> class.
        /// </summary>
        public EtinRetrievalClient()
        {
            MeFLog.LogInfo("EtinRetrievalClient.ctor()");
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Calls EtinRetrieval from MeF MSIServices
        /// </summary>
        /// <param name = "serviceContext">ServiceContext</param>
        /// <param name = "testIndicatorType">Type of the test indicator.</param>
        /// <returns>
        ///   Will return EtinRetrievalResult, see <see cref = "EtinRetrievalResult" />.
        /// </returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public EtinRetrievalResult Invoke(ServiceContext serviceContext, TestCdType testIndicatorType)
        {
            MeFLog.LogInfo(
                string.Format("EtinRetrievalClient.Invoke() on thread ID: {0}", Thread.CurrentThread.ManagedThreadId));

            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.NotNull(testIndicatorType, "TestIndicatorType");
            WCFEtinRetrievalClient proxy = new WCFEtinRetrievalClient();
            ETINRetrievalResponseType responseData = new ETINRetrievalResponseType();

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("EtinRetrieval", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new ETINRetrievalRequestType { TestCd = testIndicatorType };
                    MessageBuilder.AuditRequest(mefHeader, testIndicatorType.ToString());
                    responseData = proxy.EtinRetrieval(ref mefHeader, requestData);
                    this.SetupCookies(serviceContext);
                }

                var result = new EtinRetrievalResult();
                if (responseData != null)
                {
                    result.Cnt = responseData.Cnt;
                    result.ETINStatusGrp = responseData.ETINStatusGrp;
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