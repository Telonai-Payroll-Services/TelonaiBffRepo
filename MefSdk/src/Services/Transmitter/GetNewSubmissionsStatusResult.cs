using System;
using MeF.Client.EfileAttachments;
using MeF.Client.Logging;

namespace MeF.Client.Services.TransmitterServices
{
    /// <summary>
    ///Response from GetNewSubmissionsStatusClient
    /// </summary>
    public class GetNewSubmissionsStatusResult
    {
        private StatusRecordList _statusRecordList;

        /// <summary>
        /// Gets or sets IsMoreAvailable.
        /// </summary>
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
        /// Gets the StatusRecordList.
        /// </summary>
        ///  <returns>The <see cref="StatusRecordList"/>.</returns>
        public StatusRecordList GetStatusRecordList()
        {
            if (AttachmentFilePath == null || AttachmentFilePath == "")
            {
                MeFLog.LogInfo("GetNewSubmissionsStatusResult.GetStatusRecordList() from Memory");
                _statusRecordList = new StatusRecordList();
                _statusRecordList = _statusRecordList.Load(unzippedcontent);
                return _statusRecordList;
            }
            else
            {

                MeFLog.LogInfo(string.Format("GetNewSubmissionsStatusResult.GetStatusRecordList() from file: {0}", AttachmentFilePath));
                _statusRecordList = new StatusRecordList();
                _statusRecordList = _statusRecordList.Load(AttachmentFilePath);
                return _statusRecordList;
            }
        }
    }
}