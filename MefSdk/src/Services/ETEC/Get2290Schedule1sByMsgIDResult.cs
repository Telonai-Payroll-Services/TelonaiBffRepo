using System;

using System.Collections.ObjectModel;

namespace MeF.Client.Services.ETECTransmitterServices
{
    /// <summary>
    ///Response from Get2290Schedule1sByMsgIDClient.
    /// </summary>
    public class Get2290Schedule1sByMsgIDResult
    {
        /// <summary>
        /// Gets or sets Count.
        /// </summary>
        /// <value>The count.</value>
        public virtual int Cnt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets IsMoreAvailable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is more available; otherwise, <c>false</c>.
        /// </value>
        public virtual Boolean MoreAvailableInd { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }

        /// <summary>
        /// Gets or sets the schedule1attachments.
        /// </summary>
        /// <value>The schedule1attachments.</value>
        public Collection<FileItem> schedule1Attachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Get2290Schedule1sByMsgIDResult"/> class.
        /// </summary>
        public Get2290Schedule1sByMsgIDResult()
        {
            schedule1Attachments = new Collection<FileItem>();
        }
    }
}