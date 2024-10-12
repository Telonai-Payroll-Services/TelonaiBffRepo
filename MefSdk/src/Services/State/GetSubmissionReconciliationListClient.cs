namespace MeF.Client.Services.StateServices
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeF.Client.Logging;
    using MeFWCFClient.MeFStateServices;
    using System.ServiceModel;
    
    /// <summary>
    ///   This class is the service client for invoking the GetSubmissionReconciliationList service.
    /// </summary>
    public class GetSubmissionReconciliationListClient : MeFClientBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "GetSubmissionReconciliationListClient" /> class.
        /// </summary>
        public GetSubmissionReconciliationListClient()
        {
            MeFLog.LogInfo("GetSubmissionReconciliationListClient.ctor");
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(ServiceContext serviceContext, string maxResults)
        {
            return this.Invoke(serviceContext, maxResults, null, null, null, null);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "categoryType">Type of the category.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext, string maxResults, SubsetStSubmissionCategoryCdType categoryType)
        {
            return this.Invoke(serviceContext, maxResults, null, null, categoryType, null);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "statusType">Type of the status.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext, string maxResults, MeFSubmissionStatusCdType statusType)
        {
            return this.Invoke(serviceContext, maxResults, null, null, null, statusType);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "categoryType">Type of the category.</param>
        /// <param name = "statusType">Type of the status.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext,
            string maxResults,
            SubsetStSubmissionCategoryCdType? categoryType,
            MeFSubmissionStatusCdType? statusType)
        {
            return this.Invoke(serviceContext, maxResults, null, null, categoryType, statusType);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "startDate">The start date.</param>
        /// <param name = "endDate">The end date.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext, string maxResults, DateTime? startDate, DateTime? endDate)
        {
            return this.Invoke(serviceContext, maxResults, startDate, endDate, null, null);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "startDate">The start date.</param>
        /// <param name = "endDate">The end date.</param>
        /// <param name = "categoryType">Type of the category.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext,
            string maxResults,
            DateTime? startDate,
            DateTime? endDate,
            SubsetStSubmissionCategoryCdType? categoryType)
        {
            return this.Invoke(serviceContext, maxResults, startDate, endDate, categoryType, null);
        }

        /// <summary>
        ///   Invokes the specified service context.
        /// </summary>
        /// <param name = "serviceContext">The service context.</param>
        /// <param name = "maxResults">The max results.</param>
        /// <param name = "startDate">The start date.</param>
        /// <param name = "endDate">The end date.</param>
        /// <param name = "categoryType">Type of the category.</param>
        /// <param name = "statusType">Type of the status.</param>
        /// <returns>Returns a GetSubmissionReconciliationListResult, see <see cref = "GetSubmissionReconciliationListResult" />.</returns>
        /// <exception cref = "ToolkitException"></exception>
        /// <exception cref = "ServiceException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public GetSubmissionReconciliationListResult Invoke(
            ServiceContext serviceContext,
            string maxResults,
            DateTime? startDate,
            DateTime? endDate,
            SubsetStSubmissionCategoryCdType? categoryType,
            MeFSubmissionStatusCdType? statusType)
        {
            MeFLog.LogInfo("GetSubmissionReconciliationListClient.Invoke()");
            RequestHelper.TestCallPrerequisites(serviceContext);

            var proxy = new WCFGetSubmissionReconciliationListClient();
            var responseData = new GetSubmissionReconciliationListResponseType();
          
            try
            {
                var mefheader = RequestHelper.BuildRequestHeader("GetSubmissionReconciliationList", serviceContext);
                proxy.ClientCredentials.UserName.UserName = serviceContext.GetClientInfo().AppSysID;

                using (OperationContextScope Scope = new OperationContextScope(proxy.InnerChannel))
                {
                    this.SetupProxy(serviceContext);

                    var trace = new StringBuilder();
                    var requestData = new GetSubmissionReconciliationListRequestType();
                    trace.AppendLine(string.Format("MaxResults: {0}", maxResults));
                    if (categoryType.HasValue)
                    {
                        requestData.SubsetStSubmissionCategoryCdSpecified = true;
                        requestData.SubsetStSubmissionCategoryCd = categoryType.Value;
                        trace.AppendLine(string.Format("Category: {0}", categoryType.Value.ToString()));
                    }
                    else
                    {
                        requestData.SubsetStSubmissionCategoryCdSpecified = false;
                    }
                    if (startDate != null)
                    {
                        requestData.StartDtSpecified = true;
                        requestData.StartDt = startDate.GetValueOrDefault();
                        trace.AppendLine(string.Format("StartDate: {0}", startDate.ToString()));
                    }
                    else
                    {
                        requestData.StartDtSpecified = false;
                    }
                    if (endDate != null)
                    {
                        requestData.EndDtSpecified = true;
                        requestData.EndDt = endDate.GetValueOrDefault();
                        trace.AppendLine(string.Format("EndDate: {0}", endDate.ToString()));
                    }
                    else
                    {
                        requestData.EndDtSpecified = false;
                    }
                    if (statusType.HasValue)
                    {
                        requestData.MeFSubmissionStatusCdSpecified = true;
                        requestData.MeFSubmissionStatusCd = statusType.Value;
                        trace.AppendLine(string.Format("SubmissionStatus: {0}", statusType.ToString()));
                    }
                    else
                    {
                        requestData.MeFSubmissionStatusCdSpecified = false;
                    }
                    requestData.MaxResultCnt = maxResults;

                    MessageBuilder.AuditRequest(mefheader, trace.ToString());

                    responseData = proxy.GetSubmissionReconciliationList(ref mefheader, requestData);
                    this.SetupCookies(serviceContext);
                }

                var result = new GetSubmissionReconciliationListResult();
                if (responseData != null)
                {
                    MeFLog.LogDebug("GetSubmissionReconciliationListClient:  Handling ResponseData");
                    result.MoreAvailableInd = responseData.MoreAvailableInd;
                    result.SubmissionId = responseData.SubmissionId;
                    this.AuditResponseWithBody(mefheader, serviceContext, result); 
                }
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