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
    ///   This class is the service client for invoking the EtinStatus service.
    /// </summary>
    public class EtinStatusClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EtinStatusClient" /> class.
        /// </summary>
        public EtinStatusClient()
        {
            MeFLog.LogInfo("EtinStatusClient.ctor()");
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "etin">The etin.</param>
        /// <exception cref = "ServiceException"></exception>
        /// <returns>EtinStatusResult, see <see cref = "EtinStatusResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public EtinStatusResult Invoke(ServiceContext serviceContext, string etin)
        {
            MeFLog.LogInfo("EtinStatusClient: validating parameters and creating proxy.");
            RequestHelper.TestCallPrerequisites(serviceContext);
            Validate.IsEtinValid(etin, "Etin", "");
            WCFEtinStatusClient proxy = new WCFEtinStatusClient();
            ETINStatusResponseType responseData = new ETINStatusResponseType();

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("EtinStatus", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new ETINStatusRequestType { ETIN = etin };
                    MessageBuilder.AuditRequest(mefHeader, etin);
                    responseData = proxy.EtinStatus(ref mefHeader, requestData);
                    this.SetupCookies(serviceContext);
                }
                var result = new EtinStatusResult();

                if (responseData != null)
                {
                    result.FormStatusGrp = responseData.FormStatusGrp;
                    result.ETIN = responseData.ETIN;
                    result.StatusTxt = responseData.StatusTxt;
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