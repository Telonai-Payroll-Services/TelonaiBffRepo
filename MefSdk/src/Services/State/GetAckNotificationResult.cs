using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetAckNotificationClient.
    /// </summary>
    public class GetAckNotificationResult
    {
        private AckNotificationList _ackNotificationList;

        /// <summary>
        /// Gets or sets the path where the attachment is stored.
        /// </summary>
        /// <value>The attachment file path.</value>
        public virtual String AttachmentFilePath { get; set; }
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the list of AckNotifications extracted from the response attachment.
        /// </summary>
        /// <returns>The <see cref="AckNotificationList"/>.</returns>
        public AckNotificationList GetAckNotificationList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo(string.Format("GetAckNotificationResult.GetAckNotificationList() from byte array"));
                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(unzippedcontent);
                return _ackNotificationList;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetAckNotificationResult.GetAckNotificationList() from file: {0}", AttachmentFilePath));

                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(AttachmentFilePath);
                return _ackNotificationList;
            }
        }
    }
}