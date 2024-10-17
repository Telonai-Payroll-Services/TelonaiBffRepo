using System.Xml.Linq;
using System.Xml.Serialization;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains properties related to an SubmissionReceiptGrp.
    /// </summary>
    /// <remarks/>
    public partial class SubmissionReceiptGrp : EntityBase<SubmissionReceiptGrp>, IEfileAttachment
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
        /// Initializes a new instance of the <see cref="SubmissionReceiptGrp"/> class.
        /// </summary>
        public SubmissionReceiptGrp()
        {
            MeFLog.LogInfo("SubmissionReceiptGrp.ctor()");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionReceiptGrp"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public SubmissionReceiptGrp(string filePath)
        {
            MeFLog.LogInfo(string.Format("SubmissionReceiptGrp.ctor(FilePath: '{0}'", filePath));
            this.Load(filePath);
        }

        /// <summary>
        /// Gets the SubmissionReceiptGrp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The SubmissionReceiptGrp</returns>
        public SubmissionReceiptGrp Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptGrp.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "SubmissionReceiptGrp", "", "SubmissionReceiptGrp.Load");
            SubmissionReceiptGrp sr = LoadFromFile(filePath);

            MeFLog.LogInfo("sr.SubmissionId " + sr.SubmissionId);
            MeFLog.LogInfo("sr.FilePath " + sr.FilePath);
            MeFLog.LogInfo("sr.SubmissionReceivedTs " + sr.SubmissionReceivedTs.ToString());

            return sr;
        }

        /// <summary>
        /// Saves the SubmissionReceiptGrp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("StatusRecord.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");

            XElement doc = new XElement("SubmissionReceiptGrp",
                     new XElement("SubmissionId", SubmissionId),
                     new XElement("Timestamp", SubmissionReceivedTs));
            doc.Save(filePath);
        }
    }
}