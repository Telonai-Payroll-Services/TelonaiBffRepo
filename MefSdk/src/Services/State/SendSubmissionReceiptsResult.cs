using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;
using System;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from SendSubmissionReceiptsClient.
    /// </summary>
    public class SendSubmissionReceiptsResult
    {
        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>The Count.</value>
        public string Cnt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        public SubmissionErrorListType ErrorList { get; set; }

        public string GetErrorCnt()
        {
            MeFLog.LogInfo("SendAcksResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }
    }
}