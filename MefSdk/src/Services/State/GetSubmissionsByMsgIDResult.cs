using System;
using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetSubmissionsByMsgIDClient
    /// </summary>
    public class GetSubmissionsByMsgIDResult
    {
        /// <summary>
        /// Gets or sets IsMoreAvailable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is more available; otherwise, <c>false</c>.
        /// </value>
        public Boolean MoreAvailableInd { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }

        /// <summary>
        /// Gets or sets the IrsDataList.
        /// </summary>
        /// <value>The IRS data list.</value>
        public IRSDataForStateSubmissionListType IRSDataList { get; set; }

        /// <summary>
        /// Gets or sets the state submission container.
        /// </summary>
        /// <value>The state submission container.</value>
        public StateSubmissionContainer StateSubmissionContainer { get; set; }

        /// <summary>
        /// Gets the state submission container.
        /// </summary>
        /// <returns>The <see cref="StateSubmissionContainer"/>.</returns>
        public StateSubmissionContainer GetStateSubmissionContainer()
        {
            MeFLog.LogInfo("GetSubmissionsByMsgIDResult.GetStateSubmissionContainer()");
            return StateSubmissionContainer;
        }
    }
}