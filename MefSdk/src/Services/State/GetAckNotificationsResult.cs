using System;
using System.Collections.ObjectModel;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetAckNotificationsClient.
    /// </summary>
    public class GetAckNotificationsResult
    {
        private Collection<SubmissionErrorItem> _subErrors;

        private AckNotificationList _ackNotificationList;

        /// <summary>
        /// Gets or sets the ListOfSubmissionErrorType returned in the soap body.
        /// </summary>
        /// <value>The error list.</value>
        public SubmissionErrorListType ErrorList { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the submission errors.
        /// </summary>
        /// <returns>Collection&lt;SubmissionErrorItem&gt;</returns>
        public Collection<SubmissionErrorItem> GetSubmissionErrors()
        {
            MeFLog.LogInfo("GetAckNotificationsResult.GetSubmissionErrors()");
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
        /// <returns>ErrorList.Count</returns>
        public string GetErrorCnt()
        {
            MeFLog.LogInfo("GetAckNotificationsResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }

        /// <summary>
        /// Gets or sets the path where the attachment is stored.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }

        /// <summary>
        /// Gets the list of AckNotifications extracted from the response attachment.
        /// </summary>
        /// <returns>The <see cref="AckNotificationList"/>.</returns>
        public AckNotificationList GetAckNotificationList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetAckNotificationsResult.GetAckNotificationsList() from Memory");
                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(unzippedcontent);
                return _ackNotificationList;
            }
            else
            {
                MeFLog.LogInfo(string.Format("GetAckNotificationsResult.GetAckNotificationsList() from file: {0}", AttachmentFilePath));
                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(AttachmentFilePath);
                return _ackNotificationList;
            }
        }
    }
}