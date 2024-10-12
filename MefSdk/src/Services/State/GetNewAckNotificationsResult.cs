using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    ///Response from GetNewAckNotificationsClient.
    /// </summary>
    public class GetNewAckNotificationsResult
    {
        private AckNotificationList _ackNotificationList;

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
        public virtual String FileName { get; set; }
        public virtual byte[] unzippedcontent { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets the acknotification list.
        /// </summary>
        /// <returns>The <see cref="AckNotificationList"/>.</returns>
        public AckNotificationList GetAckNotificationList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetNewAckNotificationsResult.GetNewAckNotificationsList() from Memory");
                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(unzippedcontent);
                return _ackNotificationList;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetNewAckNotificationsResult.GetNewAckNotificationsList() from file: {0}", AttachmentFilePath));
                _ackNotificationList = new AckNotificationList();
                _ackNotificationList = _ackNotificationList.Load(AttachmentFilePath);
                return _ackNotificationList;
            }
        }
    }
}