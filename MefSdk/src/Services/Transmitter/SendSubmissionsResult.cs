using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    ///Response from SendSubmissionsClient.
    /// </summary>
    public class SendSubmissionsResult
    {
        private SubmissionReceiptList _submissionRecieptList;

        public String MessageID { get; set; }

        public String RelatesTo { get; set; }

        public String DepositId { get; set; }

        public SendSubmissionsResult()
        {
            _submissionRecieptList = new SubmissionReceiptList();
        }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public String AttachmentFilePath { get; set; }

        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }

        /// <summary>
        /// Gets the <see cref="SubmissionReceiptList"/>.
        /// </summary>
        /// <returns>The <see cref="SubmissionReceiptList"/>.</returns>
        public SubmissionReceiptList GetSubmissionReceiptList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("SendSubmissionsResult.GetSubmissionReceiptList() from Memory");
                var recs = _submissionRecieptList.Load(unzippedcontent);
                return recs;
            }
            else
            {
                MeFLog.LogInfo(string.Format("SendSubmissionsResult.GetSubmissionReceiptList() from file: {0}", AttachmentFilePath));                
                var recs = _submissionRecieptList.Load(AttachmentFilePath);
                return recs;
            }
        }
    }
}