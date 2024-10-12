using System;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from MeF that contains IsMoreAvailable and an SubmissionIdList.
    /// </summary>
    public class GetSubmissionReconciliationListResult
    {
        /// <summary>
        /// Gets or sets the SubmissionId.
        /// </summary>
        /// <value>The submission id.</value>
        public virtual String[] SubmissionId { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets IsMoreAvailable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is more available; otherwise, <c>false</c>.
        /// </value>
        public virtual Boolean MoreAvailableInd { get; set; }
    }
}