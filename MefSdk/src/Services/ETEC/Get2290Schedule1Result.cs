using System;

namespace MeF.Client.Services.ETECTransmitterServices
{
    /// <summary>
    ///Response from the Get2290Schedule1Client.
    /// </summary>
    public class Get2290Schedule1Result
    {
        /// <summary>
        /// Gets or sets the attachment file path.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the schedule1attachment.
        /// </summary>
        /// <value>The schedule1attachment.</value>
        public FileItem schedule1Attachment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Get2290Schedule1Result"/> class.
        /// </summary>
        public Get2290Schedule1Result()
        {
            schedule1Attachment = new FileItem();
        }
    }
}