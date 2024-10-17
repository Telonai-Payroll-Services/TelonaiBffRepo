using System;
using System.Collections.ObjectModel;
using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetSubmissionsClient
    /// </summary>
    public class GetSubmissionsResult
    {
        private Collection<SubmissionErrorItem> _subErrors;

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
        /// <value>The IRS data list.</value>
        public IRSDataForStateSubmissionListType IRSDataList { get; set; }

        /// <summary>
        /// Gets or sets the ErrorList.
        /// </summary>
        public SubmissionErrorListType ErrorList { get; set; }

        /// <summary>
        /// Gets the submission errors.
        /// </summary>
        /// <returns>SubmissionErrorItem</returns>
        public Collection<SubmissionErrorItem> GetSubmissionErrors()
        {
            MeFLog.LogInfo("GetSubmissionResult.GetSubmissionErrors()");
            this._subErrors = new Collection<SubmissionErrorItem>();

            for (int i = 0; i < Convert.ToInt32(ErrorList.Cnt); i++)
            {
                SubmissionErrorItem errorItem = new SubmissionErrorItem();
                errorItem.ErrorClassification = ErrorList.SubmissionError[i].ErrorClassificationDetail.ErrorClassificationCd.ToString();
                errorItem.ErrorCode = ErrorList.SubmissionError[i].ErrorClassificationDetail.ErrorMessageCd;
                errorItem.ErrorMessage = ErrorList.SubmissionError[i].ErrorClassificationDetail.ErrorMessageTxt;
                errorItem.SubmissionId = ErrorList.SubmissionError[i].SubmissionId;
                _subErrors.Add(errorItem);
            }
            return _subErrors;
        }

        /// <summary>
        /// Gets the error count.
        /// </summary>
        /// <returns>Count</returns>
        public string GetErrorCnt()
        {
            MeFLog.LogInfo("GetSubmissionResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }

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