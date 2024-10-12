using System;
using System.Collections.ObjectModel;
using MeFWCFClient.ETECTransmitterServices;

namespace MeF.Client.Services.ETECTransmitterServices
{
    /// <summary>
    ///Response from Get2290Schedule1sClient.
    /// </summary>
    public class Get2290Schedule1sResult
    {
        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>The count.</value>
        public virtual int Cnt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }

        
        /// <summary>
        /// Gets or sets the ErrorList.
        /// </summary>
        /// <value>The error list.</value>
        public SubmissionErrorType[] ErrorList { get; set; }

        /// <summary>
        /// Gets or sets schedule1Attachment.
        /// </summary>
        public Collection<FileItem> schedule1Attachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Get2290Schedule1sResult"/> class.
        /// </summary>
        public Get2290Schedule1sResult()
        {
            schedule1Attachments = new Collection<FileItem>();
        }
    }
}