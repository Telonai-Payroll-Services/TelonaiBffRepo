using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    /// Response from GetAckClient
    /// </summary>
    public class GetAckResult
    {
        private AcknowledgementList _acknowledgementList;

        public GetAckResult()
        {
            _acknowledgementList = new AcknowledgementList();
        }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the acknowledgement list.
        /// </summary>
        /// <returns>The <see cref="AcknowledgementList"/>.</returns>
        public AcknowledgementList GetAcknowledgementList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo(string.Format("GetAckResult.GetAcknowledgementList() from byte array"));
                var acks = _acknowledgementList.Load(unzippedcontent);
                return acks;
            }
            else
            {
                MeFLog.LogInfo(string.Format("GetAckResult.GetAcknowledgementList() from file: {0}", AttachmentFilePath));                
                var acks = _acknowledgementList.Load(AttachmentFilePath);
                return acks;
            }
        }
    }
}