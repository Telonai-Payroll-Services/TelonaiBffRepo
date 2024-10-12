using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    ///Response from GetNewAcksClient.
    /// </summary>
    public class GetNewAcksResult
    {
        private AcknowledgementList _acknowledgementList;

        public GetNewAcksResult()
        {
            _acknowledgementList = new AcknowledgementList();
        }

        /// <summary>
        /// Gets or sets IsMoreAvailable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is more available; otherwise, <c>false</c>.
        /// </value>
        public virtual Boolean MoreAvailableInd { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }

        /// <summary>
        /// Gets the acknowledgement list.
        /// </summary>
        /// <returns>The <see cref="AcknowledgementList"/>.</returns>
        public AcknowledgementList GetAcknowledgementList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetNewAcksResult.GetAcknowledgementList() from Memory");
                var acks = _acknowledgementList.Load(unzippedcontent);
                return acks;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetNewAcksResult.GetAcknowledgementList() from file: {0}", AttachmentFilePath));
                var acks = _acknowledgementList.Load(AttachmentFilePath);
                return acks;
            }
        }
    }
}