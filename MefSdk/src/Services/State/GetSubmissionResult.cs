using System;
using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetSubmissionClient
    /// </summary>
    public class GetSubmissionResult
    {
        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the IrsDataList.
        /// </summary>
        /// <value>The IRS data.</value>
        public IRSDataForStateSubmissionType IRSData { get; set; }

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
            MeFLog.LogInfo("GetSubmissionResult.GetStateSubmissionContainer()");
            return StateSubmissionContainer;
        }
    }
}