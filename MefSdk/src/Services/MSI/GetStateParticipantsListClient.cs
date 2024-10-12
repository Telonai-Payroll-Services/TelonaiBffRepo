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
    ///   This class is the service client for invoking the GetStateParticipantsList service.
    /// </summary>
    public class GetStateParticipantsListClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetStateParticipantsListClient" /> class.
        /// </summary>
        public GetStateParticipantsListClient()
        {
            MeFLog.LogInfo("GetStateParticipantsListClient.ctor()");
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <returns>
        ///   GetStateParticipantsListResult, see <see cref = "GetStateParticipantsListResult" />.
        /// </returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetStateParticipantsListResult Invoke(ServiceContext serviceContext)
        {
            MeFLog.LogInfo("GetStateParticipantsListClient Invoke().");
            RequestHelper.TestCallPrerequisites(serviceContext);
            WCFGetStateParticipantsListClient proxy = new WCFGetStateParticipantsListClient();
            GetStateParticipantsListResponseType responseData = new GetStateParticipantsListResponseType();

            try
            {
                var mefHeader = RequestHelper.BuildRequestHeader("GetStateParticipantsList", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);
                    var requestData = new GetStateParticipantsListRequestType();
                    MessageBuilder.AuditRequest(mefHeader, null);
                    responseData = proxy.GetStateParticipantsList(ref mefHeader, requestData);
                    this.SetupCookies(serviceContext);
                }

                var result = new GetStateParticipantsListResult();
                if (responseData != null)
                {
                    result.Cnt = responseData.Cnt;
                    result.StateParticipantGrp = responseData.StateParticipantGrp;
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