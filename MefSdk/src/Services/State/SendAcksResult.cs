using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;
using System;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from SendAcksClient
    /// </summary>
    public class SendAcksResult
    {
        public SubmissionErrorListType ErrorList { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the Error list Count.
        /// </summary>
        /// <value>The error list Count.</value>
        public string GetErrorCnt()
        {
            MeFLog.LogInfo("SendAcksResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }
    }
}