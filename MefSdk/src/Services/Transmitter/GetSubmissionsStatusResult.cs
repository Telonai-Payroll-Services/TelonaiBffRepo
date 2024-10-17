using System;
using System.Collections.ObjectModel;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;
using MeFWCFClient.MeFTransmitterServices;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    ///Response from GetSubmissionsStatusClient.
    /// </summary>
    public class GetSubmissionsStatusResult
    {
        private Collection<SubmissionErrorItem> _subErrors;
        private StatusRecordList _statusRecordList;

        /// <summary>
        /// Gets or sets the SubmissionErrorList.
        /// </summary>
        public SubmissionErrorListType ErrorList { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the list of submission errors.
        /// </summary>
        /// <returns>The List of Errors</returns>
        public Collection<SubmissionErrorItem> GetSubmissionErrors()
        {
            MeFLog.LogInfo("GetSubmissionStatusResult.GetSubmissionErrors()");
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
        /// <returns>The Count</returns>
        public string GetErrorCnt()
        {
            MeFLog.LogInfo("GetSubmissionStatusResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }

        /// <summary>
        /// Gets the StatusRecordList.
        /// </summary>
        ///  <returns>The <see cref="StatusRecordList"/>.</returns>
        public StatusRecordList GetStatusRecordList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetSubmissionsStatusResult.GetStatusRecordList() from Memory");
                _statusRecordList = new StatusRecordList();
                _statusRecordList = _statusRecordList.Load(unzippedcontent);
                return _statusRecordList;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetSubmissionsStatusResult.GetStatusRecordList() from file: {0}", AttachmentFilePath));
                _statusRecordList = new StatusRecordList();
                _statusRecordList = _statusRecordList.Load(AttachmentFilePath);
                return _statusRecordList;
            }
        }
    }
}