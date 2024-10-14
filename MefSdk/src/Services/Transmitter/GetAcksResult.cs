using System;
using System.Collections.ObjectModel;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;
using MeFWCFClient.MeFTransmitterServices;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    ///Response from GetAcksClient
    /// </summary>
    public class GetAcksResult
    {
        private Collection<SubmissionErrorItem> _subErrors;

        /// <summary>
        /// Gets or sets the SubmissionErrorList.
        /// </summary>
        /// <value>The error list.</value>
        public SubmissionErrorListType ErrorList { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public string AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the submission errors.
        /// </summary>
        /// <returns>The List of Errors</returns>
        public Collection<SubmissionErrorItem> GetSubmissionErrors()
        {
            MeFLog.LogInfo("GetAcksResult.GetSubmissionErrors()");
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
            MeFLog.LogInfo("GetAcksResult.GetErrorCount()");
            return this.ErrorList.Cnt;
        }

        private AcknowledgementList _acknowledgementList;

        public GetAcksResult()
        {
            _acknowledgementList = new AcknowledgementList();
        }

        /// <summary>
        /// Gets the acknowledgement list.
        /// </summary>
        /// <returns>The <see cref="AcknowledgementList"/>.</returns>
        public AcknowledgementList GetAcknowledgementList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetAcksResult.GetAcknowledgementList() from Memory");
                var acks = _acknowledgementList.Load(unzippedcontent);
                return acks;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetAcksResult.GetAcknowledgementList() from file: {0}", AttachmentFilePath));
                var acks = _acknowledgementList.Load(AttachmentFilePath);
                return acks;
            }
        }
    }
}