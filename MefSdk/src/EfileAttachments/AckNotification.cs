using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains properties related to an AckNotification.
    /// </summary>
    /// <remarks/>
    public partial class AckNotification : EntityBase<AckNotification>, IEfileAttachment
    {
        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        [XmlIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotification"/> class.
        /// </summary>
        public AckNotification()
        {
            MeFLog.LogInfo("AckNotification.ctor()");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotification"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public AckNotification(string filePath)
        {
            MeFLog.LogInfo(string.Format("AckNotification.ctor(FilePath: '{0}'", filePath));
            Load(filePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotification"/> class.
        /// </summary>
        /// <param name="submissionID">The submission ID.</param>
        /// <param name="timestamp">The timestamp.</param>
        public AckNotification(String submissionID, DateTime timestamp)
        {
            MeFLog.LogInfo(string.Format("AckNotification.ctor(SubmissionID: '{0}', Timestamp: '{1}')", submissionID, timestamp));
            Create(submissionID, timestamp);
        }

        /// <summary>
        /// Creates a new AckNotification.
        /// </summary>
        /// <param name="submissionID">The SubmissionID.</param>
        /// <param name="timeStamp">The TimeStamp.</param>
        /// <returns>A AckNotification</returns>
        private AckNotification Create(String submissionID, DateTime timeStamp)
        {
            this.SubmissionId = submissionID;
            this.Ts = timeStamp;
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotification"/> class.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public AckNotification(XElement xml)
        {

        }

        /// <summary>
        /// Gets the ackNotification.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A AckNotification</returns>
        public AckNotification Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("AckNotification.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "AckNotification", "", "AckNotification.Load");
            AckNotification ack = LoadFromFile(filePath);

            return ack;
        }

        /// <summary>
        /// Saves the AckNotification as an Xml fragment.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            var ack = this;
            MeFLog.LogDebug(string.Format("AckNotification.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "SaveAckNotification");
            XElement doc = new XElement("AckNotification",
                    new XElement("SubmissionId", ack.SubmissionId),
                    new XElement("Timestamp", ack.Ts));
            doc.Save(filePath);
        }
    }
}